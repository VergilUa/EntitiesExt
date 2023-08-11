using EntitiesExt;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace HybridTransformations {
   /// <summary>
   /// Synchronizes position data from the transform to the ECS world
   /// </summary>
   // Replaced by codegen
   // ReSharper disable HeapView.DelegateAllocation
   // ReSharper disable HeapView.ClosureAllocation
   [UpdateInGroup(typeof(BeforeSimulationGroup))]
   [UpdateAfter(typeof(PhysSimGroup))]
   public partial class SyncPositionToEntitySystem : SystemBase {
      #region [Fields]

      private TransformContainerSystem _transformContainerSystem;
      
      private ComponentLookup<Position> _positions;
      private ComponentLookup<SyncPositionToEntity> _tagData;
      private ComponentLookup<SyncPositionOffset> _syncPositionOffsets;

      #endregion

      protected override void OnCreate() {
         base.OnCreate();

         _positions = GetComponentLookup<Position>();
         _tagData = GetComponentLookup<SyncPositionToEntity>(true);
         _syncPositionOffsets = GetComponentLookup<SyncPositionOffset>(true);
         
         _transformContainerSystem = World.GetExistingSystemManaged<TransformContainerSystem>();
         
         EntityManager entityManager = EntityManager;
         EntityQuery posQuery = new EntityQueryBuilder(WorldUpdateAllocator).WithAllRW<Position>()
                                                                            .WithAll<SyncPositionToEntity>()
                                                                            .Build(entityManager);
         
         RequireForUpdate(posQuery);
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncPositionToEntitySystem::OnUpdate (Main Thread)");

         _positions.Update(this);
         _tagData.Update(this);
         _syncPositionOffsets.Update(this);
         
         var alignedEntities = _transformContainerSystem.AlignedEntities;
         
         SyncPositionToEntityJob syncPosJob = new SyncPositionToEntityJob
                                              {
                                                 Entities = alignedEntities,
                                                 TagData = _tagData,
                                                 Positions = _positions,
                                                 OffsetArray = _syncPositionOffsets
                                              };

         var trmArray = _transformContainerSystem.RefArray;
         
         Dependency = syncPosJob.Schedule(trmArray, Dependency);
         alignedEntities.Dispose(Dependency);
         Profiler.EndSample();
      }
      
      [BurstCompile]
      private struct SyncPositionToEntityJob : IJobParallelForTransform {
         [ReadOnly]
         public NativeArray<Entity> Entities;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<SyncPositionToEntity> TagData;
         
         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<SyncPositionOffset> OffsetArray;

         [NativeDisableParallelForRestriction]
         public ComponentLookup<Position> Positions;

         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];

            if (!TagData.HasComponent(entity)) return;
            if (!Positions.TryGetComponent(entity, out Position data)) return;

            data.Value = transform.position;

            if (OffsetArray.TryGetComponent(entity, out SyncPositionOffset offset)) {
               data.Value += offset.Value;
            }
            
            Positions[entity] = data;
         }
      }
   }
}