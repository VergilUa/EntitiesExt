using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Jobs;

namespace EntitiesExt {
   /// <summary>
   /// Single instance transform container array for all transform references
   /// </summary>
   [UpdateInGroup(typeof(AfterSimulationGroup))]
   public partial class TransformContainerSystem : SystemBase {
      #region [Properties]

      public TransformAccessArray RefArray => _refArray;
      public NativeArray<Entity> AlignedEntities => _refEntities.ToArray(Allocator.TempJob);

      public bool IsCreated => _isCreated;

      #endregion

      #region [Fields]

      private TransformAccessArray _refArray;
      private NativeList<Entity> _refEntities;
      
      private const int InitialCapacity = 128;

      private NativeParallelHashSet<int> _freeIds;
      private bool _isCreated;

      #endregion

      protected override void OnCreate() {
         base.OnCreate();

         _freeIds = new NativeParallelHashSet<int>(InitialCapacity, Allocator.Persistent);
         
         _refArray = new TransformAccessArray(InitialCapacity);
         _refEntities = new NativeList<Entity>(InitialCapacity, Allocator.Persistent);

         // Utility system, do not run it
         Enabled = false;
         _isCreated = true;
      }

      protected override void OnUpdate() {  }

      /// <summary>
      /// Adds transform to the transform container,
      /// and assigns an id for the referencing
      /// </summary>
      public int AddTransform(Entity entity, Transform trm) {
         int refId;

         // If there's a free id -> use it
         if (!_freeIds.IsEmpty) {
            var enumerator = _freeIds.GetEnumerator();
            
            enumerator.MoveNext();
            refId = enumerator.Current;
            enumerator.Dispose();

            _freeIds.Remove(refId);
            
            _refArray[refId] = trm;
            _refEntities[refId] = entity;
            
            return refId;
         }

         // Otherwise generate id / add transform and return new refId
         refId = _refArray.length;
         
         _refArray.Add(trm);
         _refEntities.Add(entity);

         return refId;
      }

      /// <summary>
      /// Releases transform reference from the transform container
      /// </summary>
      public void ReleaseTransform(int id) => _freeIds.Add(id);

      protected override void OnDestroy() {
         base.OnDestroy();
         
         if (_refArray.isCreated) _refArray.Dispose();
         if (_refEntities.IsCreated) _refEntities.Dispose();
         if (_freeIds.IsCreated) _freeIds.Dispose();

         _isCreated = false;
      }
   }
}