using EntitiesExt;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace HybridTransformations {
   /// <summary>
   /// Synchronizes rotation data from the transform to the ECS world
   /// </summary>
   // Replaced by codegen
   // ReSharper disable HeapView.DelegateAllocation
   // ReSharper disable HeapView.ClosureAllocation
   [UpdateInGroup(typeof(BeforeSimulationGroup))]
   [UpdateAfter(typeof(PhysSimGroup))]
   public partial class SyncRotationToEntitySystem : SystemBase {
      #region [Fields]

      private TransformContainerSystem _transformContainerSystem;
      
      private ComponentLookup<Rotation> _rotations;
      private ComponentLookup<SyncRotationToEntity> _tagData;

      #endregion

      protected override void OnCreate() {
         _rotations = GetComponentLookup<Rotation>();
         _tagData = GetComponentLookup<SyncRotationToEntity>(true);
         
         EntityQuery query = new EntityQueryBuilder(WorldUpdateAllocator).WithAllRW<Rotation>()
                                                                         .WithAll<SyncRotationToEntity>()
                                                                         .Build(EntityManager);
         RequireForUpdate(query);

         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncPositionToEntitySystem::OnUpdate (Main Thread)");

         _rotations.Update(this);
         _tagData.Update(this);

         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncRotationToEntityJob syncPosJob = new SyncRotationToEntityJob
                                              {
                                                 Entities = alignedEntities,
                                                 TagData = _tagData,
                                                 Rotations = _rotations
                                              };

         Dependency = syncPosJob.Schedule(_transformContainerSystem.RefArray, Dependency);
         alignedEntities.Dispose(Dependency);

         Profiler.EndSample();
      }

      [BurstCompile]
      private struct SyncRotationToEntityJob : IJobParallelForTransform {
         [ReadOnly]
         public NativeArray<Entity> Entities;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<SyncRotationToEntity> TagData;

         [NativeDisableParallelForRestriction]
         public ComponentLookup<Rotation> Rotations;

         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];

            if (!TagData.HasComponent(entity)) return;
            if (!Rotations.TryGetComponent(entity, out Rotation data)) return;

            data.Value = transform.rotation;
            Rotations[entity] = data;
         }
      }
   }
}