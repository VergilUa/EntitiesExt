using EntitiesExt.Data;
using EntitiesExt.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace HybridTransformations {
   /// <summary>
   /// Synchronizes position data from ECS to Transform
   /// </summary>
   [UpdateInGroup(typeof(AfterSimulationGroup), OrderLast = true)]
   // ReSharper disable once UnusedType.Global -> Handled by Auto-creation
   public partial class SyncPositionToTransformSystem : SystemBase {
      #region [Fields]

      private TransformContainerSystem _transformContainerSystem;

      #endregion

      protected override void OnCreate() {
         EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<SyncPositionToTransform>(),
                                            ComponentType.ReadOnly<Position>());
         
         RequireForUpdate(query);

         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncPositionToTransformSystem::OnUpdate (Main Thread)");

         var tagData = GetComponentLookup<SyncPositionToTransform>(true);
         var dontSyncTags = GetComponentLookup<DontSyncOneFrame>(true);
         var positions = GetComponentLookup<Position>(true);

         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncPositionToTransformJob syncPosJob = new SyncPositionToTransformJob
                                                 {
                                                    Entities = alignedEntities,
                                                    TagData = tagData,
                                                    DontSyncTags = dontSyncTags,
                                                    PositionArray = positions
                                                 };

         Dependency = syncPosJob.Schedule(_transformContainerSystem.RefArray, Dependency);
         alignedEntities.Dispose(Dependency);

         Profiler.EndSample();
      }

      [BurstCompile]
      private struct SyncPositionToTransformJob : IJobParallelForTransform {
         [ReadOnly]
         public NativeArray<Entity> Entities;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<SyncPositionToTransform> TagData;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<DontSyncOneFrame> DontSyncTags;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<Position> PositionArray;


         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];
            if (DontSyncTags.HasComponent(entity)) return;
            if (!TagData.HasComponent(entity)) return;
            if (!PositionArray.HasComponent(entity)) return;

            Position data = PositionArray[entity];
            transform.position = data.Value;
         }
      }
   }
}