using EntitiesExt;
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
   public partial class SyncLocalScaleToTransformSystem : SystemBase {
      #region [Fields]

      private TransformContainerSystem _transformContainerSystem;
      
      private ComponentLookup<SyncLocalScaleToTransform> _tagData;
      private ComponentLookup<DontSyncOneFrame> _dontSyncTags;
      private ComponentLookup<LocalScale> _upArray;

      #endregion

      protected override void OnCreate() {
         _tagData = GetComponentLookup<SyncLocalScaleToTransform>(true);
         _dontSyncTags = GetComponentLookup<DontSyncOneFrame>(true);
         _upArray = GetComponentLookup<LocalScale>(true);
         
         EntityQuery query = new EntityQueryBuilder(WorldUpdateAllocator)
                             .WithAll<SyncLocalScaleToTransform, LocalScale>()
                             .Build(EntityManager);
         
         RequireForUpdate(query);
         
         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncLocalScaleToTransformSystem::OnUpdate (Main Thread)");

         _tagData.Update(this);
         _dontSyncTags.Update(this);
         _upArray.Update(this);
         
         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncLocalScaleJob syncPosJob = new SyncLocalScaleJob
                                        {
                                           Entities = alignedEntities,
                                           TagData = _tagData,
                                           DontSyncTags = _dontSyncTags,
                                           LocalScaleArray = _upArray
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
            if (!LocalScaleArray.TryGetComponent(entity, out LocalScale data)) return;

            transform.localScale = data.Value;
         }
      }
   }
}