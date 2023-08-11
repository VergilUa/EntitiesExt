using EntitiesExt;
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
      private ComponentLookup<SyncPositionToTransform> _tagData;
      private ComponentLookup<DontSyncOneFrame> _dontSyncTags;
      private ComponentLookup<Position> _positions;

      #endregion

      protected override void OnCreate() {
         _tagData = GetComponentLookup<SyncPositionToTransform>(true);
         _dontSyncTags = GetComponentLookup<DontSyncOneFrame>(true);
         _positions = GetComponentLookup<Position>(true);
         
         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
         
         EntityQuery query = new EntityQueryBuilder(WorldUpdateAllocator).WithAll<Position, SyncPositionToTransform>()
                                                                         .Build(EntityManager);
         RequireForUpdate(query);
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncPositionToTransformSystem::OnUpdate (Main Thread)");

         _tagData.Update(this);
         _dontSyncTags.Update(this);
         _positions.Update(this);

         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncPositionToTransformJob syncPosJob = new SyncPositionToTransformJob
                                                 {
                                                    Entities = alignedEntities,
                                                    TagData = _tagData,
                                                    DontSyncTags = _dontSyncTags,
                                                    Positions = _positions
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
         public ComponentLookup<Position> Positions;


         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];
            
            if (DontSyncTags.HasComponent(entity)) return;
            if (!TagData.HasComponent(entity)) return;
            if (!Positions.TryGetComponent(entity, out Position data)) return;

            transform.position = data.Value;
         }
      }
   }
}