﻿#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace -> Ignore to shorten up
namespace EntitiesExt {
   public static class EntitiesBridge {
      /// <summary>
      /// Generates an entity with the specified component type setup.
      /// EntityManager is automatically taken from the World.DefaultGameObjectInjectionWorld
      /// </summary>
      public static void CreateEntity(this ComponentType[] archetype,
                                      out Entity entity,
                                      out EntityManager entityManager,
                                      byte worldIndex = 0) {
         World world = TryGetWorldAt(worldIndex);
         
         entityManager = world.EntityManager;
         entity = entityManager.CreateEntity(archetype);
      }

      /// <summary>
      /// Obtains a GameObject from the entity
      /// </summary>
      public static GameObject GetGameObject(this Entity entity, byte worldIndex = 0) {
         World world = TryGetWorldAt(worldIndex);
         
         EntityManager em = world.EntityManager;

         return em.GetComponentObject<GameObject>(entity);
      }

      /// <summary>
      /// Packs IBufferElementData to the list
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Pack<T>(this List<T> list, DynamicBuffer<T> buffer) where T : unmanaged, IBufferElementData {
         for (int i = 0; i < buffer.Length; i++) {
            list.Add(buffer[i]);
         }
      }

      /// <summary>
      /// Packs an IBufferElementData to the list (with list allocation)
      /// </summary>
      public static List<T> Pack<T>(this EntityBehaviour entityBehaviour) where T : unmanaged, IBufferElementData {
         DynamicBuffer<T> buffer = entityBehaviour.GetBuffer<T>();
         if (buffer.Length == 0) return null; // Save some space and alloc
         
         List<T> dataList = new List<T>();
         dataList.Pack(entityBehaviour.GetBuffer<T>());

         return dataList;
      }

      /// <summary>
      /// UnPacks IBufferElementData from list to the buffer
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void UnPackTo<T>(this List<T> list, EntityBehaviour entityBehaviour)
         where T : unmanaged, IBufferElementData {
         DynamicBuffer<T> buffer = entityBehaviour.SetBuffer<T>();
         
         if (list == null || list.Count == 0) return; // Nothing to unpack
         
         foreach (T data in list) {
            buffer.Add(data);
         }
      }

      /// <summary>
      /// UnPacks IBufferElementData from list to the buffer and sets tag component if there's elements
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void UnPackAddTag<T1, T2>(this List<T1> list, EntityBehaviour entityBehaviour)
         where T1 : unmanaged, IBufferElementData where T2 : unmanaged, IComponentData {
         DynamicBuffer<T1> buffer = entityBehaviour.SetBuffer<T1>();
         
         // Nothing to unpack, remove tag component
         if (list == null || list.Count == 0) {
            entityBehaviour.Remove<T2>();
            return; 
         }
         
         foreach (T1 data in list) {
            buffer.Add(data);
         }
         
         entityBehaviour.Add<T2>();
      }

      /// <summary>
      /// Uses World.DefaultGameObjectInjectionWorld.GetOrCreateSystem T to get appropriate system
      /// </summary>
      /// <remarks>
      /// Pretty much a shortcut
      /// </remarks>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static T FetchSystem<T>(byte worldIndex = 0) where T : SystemBase {
         World world = TryGetWorldAt(worldIndex);
         
         return world.GetOrCreateSystemManaged<T>();
      }

      /// <summary>
      /// Tries to obtain requested World from World.All by index
      /// </summary>
      /// <returns>
      /// Requested world if index is valid, null otherwise.
      /// </returns>
      public static World TryGetWorldAt(int index) {
         var worlds = World.All;
#if DEBUG
         if (index >= worlds.Count) {
            Debug.LogError($"{nameof(TryGetWorldAt)}:: Invalid world index passed. Currently available {worlds.Count}");
            return null;
         }
#endif

         return worlds[index];
      }

#if UNITY_EDITOR
      private static readonly List<EntityBehaviour> Buffer = new List<EntityBehaviour>();

      /// <summary>
      /// Utility for inserting multiple types in a single call
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Add<T>(this HashSet<Type> buffer) { buffer.Add(typeof(T)); }

      /// <summary>
      /// Utility for inserting multiple types in a single call
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Add<T, T1>(this HashSet<Type> buffer) {
         buffer.Add(typeof(T));
         buffer.Add(typeof(T1));
      }

      /// <summary>
      /// Utility for inserting multiple types in a single call
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Add<T, T1, T2>(this HashSet<Type> buffer) {
         buffer.Add(typeof(T));
         buffer.Add(typeof(T1));
         buffer.Add(typeof(T2));
      }

      /// <summary>
      /// Utility for inserting multiple types in a single call
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Add<T, T1, T2, T3>(this HashSet<Type> buffer) {
         buffer.Add(typeof(T));
         buffer.Add(typeof(T1));
         buffer.Add(typeof(T2));
         buffer.Add(typeof(T3));
      }
      
      /// <summary>
      /// Cleanups multiple EntityBehaviour components on the passed gameObject and creates a new one if its missing
      /// </summary>
      public static void SetupEntityBehaviour(this GameObject gameObject, ref EntityBehaviour entityBehaviour) {
         if (entityBehaviour == null) entityBehaviour = gameObject.SetupEntityBehaviour();
      }

      /// <summary>
      /// Cleanups multiple EntityBehaviour components on the passed gameObject and creates a new one if its missing
      /// </summary>
      private static EntityBehaviour SetupEntityBehaviour(this GameObject gameObject) {
         Buffer.Clear();
         gameObject.GetComponents(Buffer);

         if (Buffer.Count > 1) {
            for (int i = 0; i < Buffer.Count; i++) {
               EntityBehaviour comp = Buffer[i];

               PrefabAssetType type = PrefabUtility.GetPrefabAssetType(comp);
               if (type != PrefabAssetType.NotAPrefab) continue;

               EditorApplication.delayCall += () => {
                  Object.DestroyImmediate(comp);
                  MarkDirtyIfPrefab(gameObject);
               };
               Buffer.RemoveAt(i);

               if (Buffer.Count <= 1) {
                  break;
               }
            }
         }

         return Buffer.Count <= 0 ? gameObject.AddComponent<EntityBehaviour>() : Buffer[0];
      }

      /// <summary>
      /// Finds an EntityBehaviour in the hierarchy if EntityBehaviour is null
      /// </summary>
      public static void FindEntityBehaviour(this MonoBehaviour behaviour, ref EntityBehaviour entityBehaviour) {
         if (entityBehaviour != null) return;
         
         if (!behaviour.TryGetComponent(out entityBehaviour)) {
            entityBehaviour = behaviour.GetComponentInParent<EntityBehaviour>();
         }
      }

      private static void MarkDirtyIfPrefab(GameObject go) {
         PrefabAssetType type = PrefabUtility.GetPrefabAssetType(go);
         if (type == PrefabAssetType.NotAPrefab) return;

         EditorUtility.SetDirty(go);
      }
      
            
      /// <summary>
      /// Generates component hashes for the ArchetypeLookup.
      /// </summary>
      /// <remarks>
      /// Should be used in editor only
      /// </remarks>
      public static void GenerateComponentHashes(this HashSet<Type> typeBuffer,
                                                 out ulong combinedComponentHash,
                                                 out ulong[] componentHashes) {
         NativeList<ulong> hashes = new NativeList<ulong>(Allocator.Temp);
         
         // Use simple Josh Bloch hashing algo, two primes, and some extra value
         combinedComponentHash = 17;
         
         TypeManager.Initialize();

         unchecked { // Ignore overflow, wrap
            using (hashes) {
               foreach (Type type in typeBuffer) {
                  int typeIndex = TypeManager.GetTypeIndex(type);
                  TypeManager.TypeInfo typeInfo = TypeManager.GetTypeInfo(typeIndex);

                  ulong hash = typeInfo.StableTypeHash;
                  hashes.Add(hash);

                  combinedComponentHash = combinedComponentHash * 23 + hash;
               }

               componentHashes = hashes.AsArray().ToArray();
            }   
         }
      }
      
      public static SerializedArchetype GenerateArchetype(this HashSet<Type> types) {
         SerializedArchetype archetype = new SerializedArchetype();
         types.GenerateComponentHashes(out archetype.ArchetypeUniqueHash, out archetype.ComponentHashes);

         return archetype;
      }
#endif
   }
}