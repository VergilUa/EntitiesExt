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
      private ComponentLookup<SyncLocalPositionToTransform> _tagData;
      private ComponentLookup<DontSyncOneFrame> _dontSyncTags;
      private ComponentLookup<LocalPosition> _localPositions;

      #endregion

      protected override void OnCreate() {
         _tagData = GetComponentLookup<SyncLocalPositionToTransform>(true);
         _dontSyncTags = GetComponentLookup<DontSyncOneFrame>(true);
         _localPositions = GetComponentLookup<LocalPosition>(true);
         
         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
         
         EntityQuery query = new EntityQueryBuilder(WorldUpdateAllocator)
                             .WithAll<LocalPosition, SyncLocalPositionToTransform>()
                             .Build(EntityManager);
         
         RequireForUpdate(query);
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncLocalPositionToTransformSystem::OnUpdate (Main Thread)");

         _tagData.Update(this);
         _dontSyncTags.Update(this);
         _localPositions.Update(this);
         
         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncLocalPositionJob syncPosJob = new SyncLocalPositionJob
                                           {
                                              Entities = alignedEntities,
                                              TagData = _tagData,
                                              DontSyncTags = _dontSyncTags,
                                              LocalPositions = _localPositions
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
         public ComponentLookup<LocalPosition> LocalPositions;

         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];
            
            if (DontSyncTags.HasComponent(entity)) return;
            if (!TagData.HasComponent(entity)) return;
            if (!LocalPositions.TryGetComponent(entity, out LocalPosition data)) return;

            transform.localPosition = data.Value;
         }
      }
   }
}