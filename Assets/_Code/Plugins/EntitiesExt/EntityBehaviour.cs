using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EntitiesExt;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace EntitiesExt {
   /// <summary>
   /// Similar to GameObjectEntity, contains GameObject -> Entity bridge and handles its initialization / cleanup.
   /// Injects entities to the DefaultInjectionWorld. Can only be one on the gameObject
   /// </summary>
   [DisallowMultipleComponent]
   public sealed class EntityBehaviour : MonoBehaviour {
      [SerializeField]
      private byte _insertToWorld = 0;
      
      [SerializeField, HideInInspector]
      private ulong[] _componentHashes = Array.Empty<ulong>();

      [SerializeField, HideInInspector]
      private ulong _uniqueComponentsHash;

      [SerializeField, HideInInspector]
      private List<MonoBehaviour> _suppliers = new List<MonoBehaviour>();
      
      #region [Properties]

      public Entity Entity { get; private set; }

      public byte WorldIndex => _insertToWorld;

      public EntityManager EntityManager => _entityManager;

      public bool IsInitialized => _isInitialized;

      public EntityCommandBuffer Buffer => _ecbSystem.ECB;

      #endregion
      
      #region [Fields]

      private World _world;
      private EntityManager _entityManager;
      private BeginFrameEntityCommandBufferSystem _ecbSystem;

      private bool _isInitialized;

      #endregion

      private void OnEnable() {
#if UNITY_EDITOR
         if (gameObject.scene.isSubScene) return;
#endif
         Initialize();
      }

      private void OnDisable() => CleanupEntity();

      public void Initialize() {
#if DEBUG
         GameObject go = gameObject;
         if (!go.activeInHierarchy) {
            Debug.LogError("Initializing entity on disabled gameObject will cause desync in MonoBehaviour <-> Entities state."
                           + $" This is most-likely a bug. Investigate a callstack and prevent this. Caused by {gameObject}",
                           go);
            Debug.Break();
            return;
         }
#endif
         
         Profiler.BeginSample("EntityContainer:: Initialize");
         if (_isInitialized) {
            Profiler.EndSample();
            return;
         }
         
         // Before everything -> Ensure no double initialization occurs when iterating IEntitySuppliers
         _isInitialized = true;

         _world = EntitiesBridge.TryGetWorldAt(_insertToWorld);
#if UNITY_EDITOR
         if (_world == null) {
            Debug.LogError($"{nameof(EntityBehaviour)}:: Unable to get World at index {_insertToWorld} ");
            return;
         }
#endif
         
         _entityManager = _world.EntityManager;
         _ecbSystem = _world.GetExistingSystemManaged<BeginFrameEntityCommandBufferSystem>();
         
#if UNITY_EDITOR
         if (_ecbSystem == null) {
            Debug.LogError($"{nameof(EntityBehaviour)}:: Unable to get {nameof(BeginFrameEntityCommandBufferSystem)} "
                           + $"from World at index {_insertToWorld} ({World.All[_insertToWorld].Name}). "
                           + $"Make sure it is created by the bootstrap");
            return;
         }
#endif
         
         EntityArchetype archetype = ArchetypeLookup.GetCreateArchetype(_uniqueComponentsHash,
                                                                        _componentHashes,
                                                                        _entityManager);
         Entity entity = _entityManager.CreateEntity(archetype);
         Entity = entity;
         
         Profiler.BeginSample("EntityContainer:: SetupEntity");
         
         EntityCommandBuffer ecb = Buffer;
         
         foreach (MonoBehaviour behaviour in _suppliers) {
            IEntitySupplier supplier = behaviour as IEntitySupplier;
            
            // ReSharper disable once PossibleNullReferenceException -> Inputs filtered
            supplier.SetupEntity(entity, ecb);
         }
         
         Profiler.EndSample();

#if UNITY_EDITOR
         SetName(name);
#endif
         Profiler.EndSample();
      }

      private void CleanupEntity() {
         if (!_isInitialized) return;
         
         if (_world != null && _world.IsCreated) {
            if (_entityManager.Exists(Entity)) {
               var ecb = Buffer;
               
               // Check if buffer ready, can occur in cases when entity is spawned before buffer is ready
               if (ecb.IsCreated) {
                  ecb.DestroyEntity(Entity);
               } else {
                  _entityManager.DestroyEntity(Entity);
               }
            }
         }
         
         _world = null;
         Entity = Entity.Null;

         _isInitialized = false;
      }
      
      /// <summary>
      /// Attempts to obtain data from the entity 
      /// </summary>
      /// <returns>In case if there isn't one, or its not initialized returns default value</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public T Get<T>() where T : unmanaged, IComponentData {
         try {
            return !_isInitialized ? default : _entityManager.GetComponentData<T>(Entity);
         } catch (ArgumentException) {
#if UNITY_EDITOR
            Debug.LogError($"Unable to get {typeof(T).Name} from {name}. Entity: {EntityManager.GetName(Entity)}",
                           gameObject);
#endif
            throw;
         }
      }

      /// <summary>
      /// Simultaneously adds and sets component to the entity
      /// </summary>
      /// <remarks>
      /// Use GatherEntityTypes to add components during initialization instead.
      /// 
      /// Use this method only if you're modifying entity archetype during simulation or runtime,
      /// as using this method is more costly than instantiation via archetype.
      /// </remarks>
      public void AddSet<T>(T data) where T : unmanaged, IComponentData {
         AssertEntityNotNull();
            
         Buffer.AddComponent(Entity, data);
      }

      /// <summary>
      /// Sets component data to the entity  
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Set<T>(T data) where T : unmanaged, IComponentData {
         AssertEntityNotNull();
         
         Buffer.SetComponent(Entity, data);
      }
      
      /// <summary>
      /// Sets component data to the entity (without using Buffer)  
      /// </summary>
      /// <remarks>Should only be used in Main thread only groups / context</remarks>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void SetDirect<T>(T data) where T : unmanaged, IComponentData {
         AssertEntityNotNull();
         
         _entityManager.SetComponentData(Entity, data);
      }

      /// <summary>
      /// Adds a component to the contained entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Add<T>() where T : unmanaged, IComponentData {
         AssertEntityNotNull();
         
         Buffer.AddComponent<T>(Entity);
      }

      /// <summary>
      /// Adds components to the contained entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Add<T, T1>() where T : unmanaged, IComponentData where T1 : unmanaged, IComponentData {
         AssertEntityNotNull();
         
         Buffer.AddComponent<T>(Entity);
         Buffer.AddComponent<T1>(Entity);
      }
      
      /// <summary>
      /// Adds components to the contained entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Add<T, T1, T2>() where T : unmanaged, 
                                   IComponentData where T1 : unmanaged, 
                                   IComponentData where T2 : unmanaged, 
                                   IComponentData {
         AssertEntityNotNull();
         
         Buffer.AddComponent<T>(Entity);
         Buffer.AddComponent<T1>(Entity);
         Buffer.AddComponent<T2>(Entity);
      }
      
      /// <summary>
      /// Adds components to the contained entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Add<T, T1, T2, T3>()
         where T : unmanaged, IComponentData
         where T1 : unmanaged, IComponentData
         where T2 : unmanaged, IComponentData
         where T3 : unmanaged, IComponentData {
         AssertEntityNotNull();
         
         Buffer.AddComponent<T>(Entity);
         Buffer.AddComponent<T1>(Entity);
         Buffer.AddComponent<T2>(Entity);
         Buffer.AddComponent<T3>(Entity);
      }
      
      /// <summary>
      /// Adds components to the contained entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Add<T, T1, T2, T3, T4>()
         where T : unmanaged, IComponentData
         where T1 : unmanaged, IComponentData
         where T2 : unmanaged, IComponentData
         where T3 : unmanaged, IComponentData
         where T4 : unmanaged, IComponentData{
         AssertEntityNotNull();
         
         Buffer.AddComponent<T>(Entity);
         Buffer.AddComponent<T1>(Entity);
         Buffer.AddComponent<T2>(Entity);
         Buffer.AddComponent<T3>(Entity);
         Buffer.AddComponent<T4>(Entity);
      }

      /// <summary>
      /// Adds a dynamic buffer to the Entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void AddBuffer<T>() where T : unmanaged, IBufferElementData {
         AssertEntityNotNull();
         Buffer.AddBuffer<T>(Entity);
      }
      
      /// <summary>
      /// Adds a dynamic buffer to the Entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void AddBuffer<T, T1>() where T : unmanaged, IBufferElementData where T1 : unmanaged, IBufferElementData {
         AssertEntityNotNull();
         
         Buffer.AddBuffer<T>(Entity);
         Buffer.AddBuffer<T1>(Entity);
      }

      /// <summary>
      /// Removes dynamic buffer from the Entity
      /// </summary>
      public void RemoveBuffer<T>() where T : struct, IBufferElementData {
         AssertEntityNotNull();
         
         Buffer.RemoveComponent<T>(Entity);
      }

      /// <summary>
      /// Generates a dynamic buffer reference and allows to modify DynamicBuffer via ECB
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public DynamicBuffer<T> SetBuffer<T>() where T : unmanaged, IBufferElementData {
         AssertEntityNotNull();
         
         return Buffer.SetBuffer<T>(Entity);
      }
      
      /// <summary>
      /// Adds a buffer, and generates a dynamic buffer reference and allows to modify DynamicBuffer via ECB
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public DynamicBuffer<T> AddSetBuffer<T>() where T : unmanaged, IBufferElementData {
         AssertEntityNotNull();
         
         Buffer.AddBuffer<T>(Entity);
         return Buffer.SetBuffer<T>(Entity);
      }

      /// <summary>
      /// Obtains a dynamic buffer of T IBufferElementData
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public DynamicBuffer<T> GetBuffer<T>() where T : unmanaged, IBufferElementData {
         AssertEntityNotNull();
         
         return _entityManager.GetBuffer<T>(Entity);
      }

      /// <summary>
      /// Removes a component from the contained entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Remove<T>() {
         AssertEntityNotNull();
         
         Buffer.RemoveComponent<T>(Entity);
      }
      
      /// <summary>
      /// Removes a component from the contained entity.
      /// Checks if this EntityBehaviour is initialized before doing so.
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void RemoveSafe<T>() {
         if (!IsInitialized) return;
         if (!Buffer.IsCreated) return;
         
         AssertEntityNotNull();
         
         Buffer.RemoveComponent<T>(Entity);
      }
      
      /// <summary>
      /// Removes a component from the contained entity directly via EntityManager
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void RemoveDirect<T>() {
         AssertEntityNotNull();
         
         EntityManager.RemoveComponent<T>(Entity);
      }
      
      /// <summary>
      /// Removes components from the contained entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Remove<T, T1>() {
         AssertEntityNotNull();
         
         Buffer.RemoveComponent<T>(Entity);
         Buffer.RemoveComponent<T1>(Entity);
      }

      /// <summary>
      /// Removes components from the contained entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Remove<T, T1, T2>() {
         AssertEntityNotNull();
         
         Buffer.RemoveComponent<T>(Entity);
         Buffer.RemoveComponent<T1>(Entity);
         Buffer.RemoveComponent<T2>(Entity);
      }

      /// <summary>
      /// Clears DynamicBuffer T
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Clear<T>() where T : unmanaged, IBufferElementData {
         AssertEntityNotNull();
         
         Buffer.SetBuffer<T>(Entity);
      }

      /// <summary>
      /// Clears DynamicBuffer T
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Clear<T, T1>() where T : unmanaged, IBufferElementData where T1 : unmanaged, IBufferElementData {
         AssertEntityNotNull();

         Buffer.SetBuffer<T>(Entity);
         Buffer.SetBuffer<T1>(Entity);
      }
      
      /// <summary>
      /// Clears DynamicBuffer T
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Clear<T, T1, T2>() where T : unmanaged, IBufferElementData 
                                     where T1 : unmanaged, IBufferElementData 
                                     where T2 : unmanaged, IBufferElementData  {
         AssertEntityNotNull();
         
         Buffer.SetBuffer<T>(Entity);
         Buffer.SetBuffer<T1>(Entity);
         Buffer.SetBuffer<T2>(Entity);
      }

      /// <summary>
      /// Checks if the entity has a specific component
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public bool HasComponent<T>() { return _isInitialized && _entityManager.HasComponent<T>(Entity); }

      /// <summary>
      /// Attaches a managed object to the entity via AddComponentObject
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Add<T>(T obj) where T : Object {
         _entityManager.AddComponentObject(Entity, obj);
      }
      
      /// <summary>
      /// Attaches managed objects to the entity via AddComponentObject
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Add<T, T1>(T obj, T1 obj2) where T : Object where T1 : Object {
         _entityManager.AddComponentObject(Entity, obj);
         _entityManager.AddComponentObject(Entity, obj2);
      }
      
      /// <summary>
      /// Attaches managed objects to the entity via AddComponentObject
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Add<T, T1, T2>(T obj, T1 obj2, T2 obj3) where T : Object where T1 : Object where T2 : Object {
         _entityManager.AddComponentObject(Entity, obj);
         _entityManager.AddComponentObject(Entity, obj2);
         _entityManager.AddComponentObject(Entity, obj3);
      }

      [Conditional("DEBUG")]
      private void AssertEntityNotNull() {
         if (Entity == default) {
            Debug.LogError("Entity == default. "
                           + $"Ensure EntityContainer.Initialize has been called before accessing ({gameObject})",
                           this);
         }
      }

#if UNITY_EDITOR
      public ulong[] CompHashesEditorOnly => _componentHashes;
      public List<MonoBehaviour> SuppliersEditorOnly => _suppliers;
      
      private static readonly HashSet<Type> TypeBuffer = new HashSet<Type>();

      public void SetName(string str) { _entityManager.SetName(Entity, str); }

      public void RebuildEntity_Editor() {
         // Simplest hack to notify all objects OnDisable + perform cleanup and re-initialization
         // Otherwise would require having extra callbacks
         gameObject.SetActive(false);
         gameObject.SetActive(true);
      }

      private void OnValidate() {
         TypeBuffer.Clear();
         _suppliers.Clear();
         
         TypeManager.Initialize();
         
         // Always insert suppliers that are attached to this GameObject
         IEntitySupplier[] suppliers = GetComponents<IEntitySupplier>();
         foreach (IEntitySupplier supplier in suppliers) {
            supplier.GatherEntityTypes(TypeBuffer);
            
            _suppliers.Add(supplier as MonoBehaviour);
         }
         
         ProcessSuppliersRecursive(transform);
         TypeBuffer.GenerateComponentHashes(out _uniqueComponentsHash, out _componentHashes);
      }

      private void ProcessSuppliersRecursive(Transform parent) {
         // Walk through children of transform, find ones that do not have EntityContainer,
         // and them as suppliers as well
         foreach (Transform trm in parent) {
            GameObject childGO = trm.gameObject;
            if (childGO.TryGetComponent(out EntityBehaviour _)) continue;

            IEntitySupplier[] suppliers = trm.GetComponents<IEntitySupplier>();
            foreach (IEntitySupplier supplier in suppliers) {
               supplier.GatherEntityTypes(TypeBuffer);
            
               _suppliers.Add(supplier as MonoBehaviour);
            }
            
            ProcessSuppliersRecursive(trm);
         }
      }
#endif
   }
}