using EntitiesExt;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace HybridTransformations {
   /// <summary>
   /// Synchronizes local position data from ECS to transform
   /// </summary>
   [UpdateInGroup(typeof(AfterSimulationGroup))]
   public partial class SyncLocalPositionToTransformSystem : SystemBase {
      #region [Fields]

      private TransformContainerSystem _transformContainerSystem;

      #endregion

      protected override void OnCreate() {
         EntityQuery query = new EntityQueryBuilder(WorldUpdateAllocator)
                             .WithAll<LocalPosition, SyncLocalPositionToTransform>()
                             .Build(EntityManager);
         
         RequireForUpdate(query);
         
         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncLocalPositionToTransformSystem::OnUpdate (Main Thread)");

         var tagData = GetComponentLookup<SyncLocalPositionToTransform>(true);
         var dontSyncTags = GetComponentLookup<DontSyncOneFrame>(true);
         var localPositionArr = GetComponentLookup<LocalPosition>(true);
         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncLocalPositionJob syncPosJob = new SyncLocalPositionJob
                                           {
                                              Entities = alignedEntities,
                                              TagData = tagData,
                                              DontSyncTags = dontSyncTags,
                                              LocalPositionArray = localPositionArr
                                           };

         Dependency = syncPosJob.Schedule(_transformContainerSystem.RefArray, Dependency);
         alignedEntities.Dispose(Dependency);

         Profiler.EndSample();
      }

      [BurstCompile]
      private struct SyncLocalPositionJob : IJobParallelForTransform {
         [ReadOnly]
         public NativeArray<Entity> Entities;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<SyncLocalPositionToTransform> TagData;
         
         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<DontSyncOneFrame> DontSyncTags;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<LocalPosition> LocalPositionArray;

         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];
            
            if (DontSyncTags.HasComponent(entity)) return;
            if (!TagData.HasComponent(entity)) return;
            if (!LocalPositionArray.HasComponent(entity)) return;

            LocalPosition data = LocalPositionArray[entity];
            transform.localPosition = data.Value;
         }
      }
   }
}