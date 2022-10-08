using EntitiesExt;
using EntitiesExt.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace HybridTransformations {
   /// <summary>
   /// Synchronizes local scale data from ECS to transform
   /// </summary>
   [UpdateInGroup(typeof(AfterSimulationGroup))]
   // ReSharper disable once UnusedType.Global -> Handled by Auto-creation
   public partial class SyncLocalScaleToTransformSystem : SystemBase {
      #region [Fields]

      private TransformContainerSystem _transformContainerSystem;

      #endregion

      protected override void OnCreate() {
         EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<SyncLocalScaleToTransform>(),
                                            ComponentType.ReadOnly<LocalScale>());
         RequireForUpdate(query);
         
         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncLocalScaleToTransformSystem::OnUpdate (Main Thread)");

         var tagData = GetComponentLookup<SyncLocalScaleToTransform>(true);
         var dontSyncTags = GetComponentLookup<DontSyncOneFrame>(true);
         var upArray = GetComponentLookup<LocalScale>(true);
         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncLocalScaleJob syncPosJob = new SyncLocalScaleJob
                                        {
                                           Entities = alignedEntities,
                                           TagData = tagData,
                                           DontSyncTags = dontSyncTags,
                                           LocalScaleArray = upArray
                                        };

         Dependency = syncPosJob.Schedule(_transformContainerSystem.RefArray, Dependency);
         alignedEntities.Dispose(Dependency);

         Profiler.EndSample();
      }

      [BurstCompile]
      private struct SyncLocalScaleJob : IJobParallelForTransform {
         [ReadOnly]
         public NativeArray<Entity> Entities;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<SyncLocalScaleToTransform> TagData;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<DontSyncOneFrame> DontSyncTags;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<LocalScale> LocalScaleArray;

         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];

            if (DontSyncTags.HasComponent(entity)) return;
            if (!TagData.HasComponent(entity)) return;
            if (!LocalScaleArray.HasComponent(entity)) return;

            LocalScale data = LocalScaleArray[entity];
            transform.localScale = data.Value;
         }
      }
   }
}