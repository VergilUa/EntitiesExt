using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;

namespace EntitiesExt {
   /// <summary>
   /// Generates a transform reference for the entity
   /// </summary>
   public class EntityTransform : MonoBehaviour, IEntitySupplier {
      [SerializeField]
      private Transform _targetTransform = default;

      [SerializeField]
      private EntityBehaviour _entityBehaviour = default;

      #region [Properties]

      public Vector3 Position => _targetTransform.position;

      public Transform Transform => _targetTransform;

      #endregion

      #region [Fields]

      private int _transformId = -1;
      private TransformContainerSystem _container;

      #endregion

      public void SetupEntity(Entity entity, EntityCommandBuffer ecb) {
#if DEBUG
         if (_transformId != -1) {
            Debug.LogError("EntityTransform::SetupEntity() Transform is already added to this entity. "
                           + "Cannot add another one. This will lead to errors, remove this call",
                           _targetTransform);
            return;
         }
#endif
         Profiler.BeginSample("EntityTransform::AddTransform (Main Thread)");

         var world = EntitiesBridge.TryGetWorldAt(_entityBehaviour.WorldIndex);
         _container = world.GetOrCreateSystemManaged<TransformContainerSystem>();
         _transformId = _container.AddTransform(entity, _targetTransform);
         
         _entityBehaviour.Add(this);
         
         Profiler.EndSample();
      }

      private void OnDisable() => CleanupTransforms();

      private void CleanupTransforms() {
         Profiler.BeginSample("EntityTransform::CleanupTransforms (Main Thread)");

         if (_transformId != -1 && _container != null && _container.IsCreated) {
            _container.ReleaseTransform(_transformId);
            _transformId = -1;
         }

         Profiler.EndSample();
      }

#if UNITY_EDITOR
      public void GatherEntityTypes(HashSet<Type> types) {
         types.Add<EntityTransform>();
      }

      public void SetTransform_Editor(Transform trm) {
         _targetTransform = trm;
      }

      protected virtual void OnValidate() {
         if (_targetTransform == null) _targetTransform = transform;
         if (_entityBehaviour == null) _entityBehaviour = GetComponentInChildren<EntityBehaviour>(true);
      }
#endif
   }

   public static class EntityTransformExt {
      public static Transform GetTransform(this EntityManager em, Entity entity) {
         return em.GetComponentObject<EntityTransform>(entity).Transform;
      }
   }
}