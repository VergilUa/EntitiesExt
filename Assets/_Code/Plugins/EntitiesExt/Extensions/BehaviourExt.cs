using System.Runtime.CompilerServices;
using Unity.Entities;
using UnityEngine;

namespace EntitiesExt {
   /// <summary>
   /// Extensions for the EntityBehaviour & Entities API
   /// </summary>
   public static class BehaviourExt {
      /// <summary>
      /// Attaches a managed object to the entity via EntityManager.AddComponentObject
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Add<T>(this EntityManager entityManager, Entity entity, T obj)
         where T : Object {
         
         entityManager.AddComponentObject(entity, obj);
      }

      /// <summary>
      /// Attaches managed objects to the entity via EntityManager.AddComponentObject
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Add<T, T1>(this EntityManager entityManager, Entity entity, T obj, T1 obj2)
         where T : Object
         where T1 : Object {
         
         entityManager.AddComponentObject(entity, obj);
         entityManager.AddComponentObject(entity, obj2);
      }

      /// <summary>
      /// Attaches managed objects to the entity via AddComponentObject
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Add<T, T1, T2>(this EntityManager entityManager, Entity entity, T obj, T1 obj2, T2 obj3)
         where T : Object
         where T1 : Object
         where T2 : Object {
         
         entityManager.AddComponentObject(entity, obj);
         entityManager.AddComponentObject(entity, obj2);
         entityManager.AddComponentObject(entity, obj3);
      }

      /// <summary>
      /// Adds components to the buffer
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void AddComponent<T, T1>(this EntityCommandBuffer ecb, Entity entity)
         where T : unmanaged, IComponentData
         where T1 : unmanaged, IComponentData {
         
         ecb.AddComponent<T>(entity);
         ecb.AddComponent<T1>(entity);
      }

      /// <summary>
      /// Adds components to the entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void AddComponent<T, T1, T2>(this EntityCommandBuffer ecb, Entity entity)
         where T : unmanaged, IComponentData
         where T1 : unmanaged, IComponentData
         where T2 : unmanaged, IComponentData {
         
         ecb.AddComponent<T>(entity);
         ecb.AddComponent<T1>(entity);
         ecb.AddComponent<T2>(entity);
      }

      /// <summary>
      /// Adds a buffer, generates a dynamic buffer reference and allows to modify DynamicBuffer via ECB
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static DynamicBuffer<T> AddSetBuffer<T>(this EntityCommandBuffer ecb, Entity entity)
         where T : unmanaged, IBufferElementData {
         
         ecb.AddBuffer<T>(entity);
         return ecb.SetBuffer<T>(entity);
      }

      /// <summary>
      /// Removes components from the entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void RemoveComponent<T, T1>(this EntityCommandBuffer ecb, Entity entity) {
         ecb.RemoveComponent<T>(entity);
         ecb.RemoveComponent<T1>(entity);
      }

      /// <summary>
      /// Removes components from the entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void RemoveComponent<T, T1, T2>(this EntityCommandBuffer ecb, Entity entity) {
         ecb.RemoveComponent<T>(entity);
         ecb.RemoveComponent<T1>(entity);
         ecb.RemoveComponent<T2>(entity);
      }

      /// <summary>
      /// Adds a dynamic buffer to the Entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void AddBuffer<T, T1>(this EntityCommandBuffer ecb, Entity entity)
         where T : unmanaged, IBufferElementData
         where T1 : unmanaged, IBufferElementData {
         
         ecb.AddBuffer<T>(entity);
         ecb.AddBuffer<T1>(entity);
      }

      /// <summary>
      /// Adds a dynamic buffer to the Entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void AddBuffer<T, T1, T2>(this EntityCommandBuffer ecb, Entity entity)
         where T : unmanaged, IBufferElementData
         where T1 : unmanaged, IBufferElementData
         where T2 : unmanaged, IBufferElementData {
         
         ecb.AddBuffer<T>(entity);
         ecb.AddBuffer<T1>(entity);
         ecb.AddBuffer<T2>(entity);
      }

      /// <summary>
      /// Clears DynamicBuffer T
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Clear<T>(this EntityCommandBuffer ecb, Entity entity)
         where T : unmanaged, IBufferElementData {
         
         ecb.SetBuffer<T>(entity);
      }

      /// <summary>
      /// Clears DynamicBuffer T
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Clear<T, T1>(this EntityCommandBuffer ecb, Entity entity)
         where T : unmanaged, IBufferElementData
         where T1 : unmanaged, IBufferElementData {
         
         ecb.SetBuffer<T>(entity);
         ecb.SetBuffer<T1>(entity);
      }

      /// <summary>
      /// Clears DynamicBuffer T
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Clear<T, T1, T2>(this EntityCommandBuffer ecb, Entity entity)
         where T : unmanaged, IBufferElementData
         where T1 : unmanaged, IBufferElementData
         where T2 : unmanaged, IBufferElementData {
         
         ecb.SetBuffer<T>(entity);
         ecb.SetBuffer<T1>(entity);
         ecb.SetBuffer<T2>(entity);
      }
   }
}