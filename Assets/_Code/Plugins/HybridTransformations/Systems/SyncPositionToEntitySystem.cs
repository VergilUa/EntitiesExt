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

      #endregion

      protected override void OnCreate() {
         base.OnCreate();

         EntityManager entityManager = EntityManager;
         EntityQuery posQuery = new EntityQueryBuilder(WorldUpdateAllocator).WithAll<SyncPositionToEntity, Position>()
                                                                            .Build(entityManager);
         
         EntityQuery posOffsetQuery = new EntityQueryBuilder(WorldUpdateAllocator)
                                      .WithAll<SyncPositionToEntity, SyncPositionOffset>()
                                      .WithAllRW<Position>()
                                      .Build(entityManager);

         NativeArray<EntityQuery> queries = new NativeArray<EntityQuery>(2, Allocator.Temp);
         queries[0] = posQuery;
         queries[1] = posOffsetQuery;
         
         RequireAnyForUpdate(queries);

         _transformContainerSystem = World.GetExistingSystemManaged<TransformContainerSystem>();         
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncPositionToEntitySystem::OnUpdate (Main Thread)");

         var positions = GetComponentLookup<Position>();
         var tagData = GetComponentLookup<SyncPositionToEntity>(true);
         var positionOffsets = GetComponentLookup<SyncPositionOffset>(true);

         var alignedEntities = _transformContainerSystem.AlignedEntities;
         
         SyncPositionToEntityJob syncPosJob = new SyncPositionToEntityJob
                                              {
                                                 Entities = alignedEntities,
                                                 TagData = tagData,
                                                 PositionArray = positions,
                                                 OffsetArray = positionOffsets
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
         public ComponentLookup<Position> PositionArray;

         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];

            if (!TagData.HasComponent(entity)) return;
            if (!PositionArray.HasComponent(entity)) return;

            Vector3 pos = transform.position;

            Position data = PositionArray[entity];
            data.Value = pos;

            if (OffsetArray.HasComponent(entity)) {
               data.Value += OffsetArray[entity].Value;
            }
            
            PositionArray[entity] = data;
         }
      }
   }
}