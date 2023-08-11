using EntitiesExt;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace HybridTransformations {
   /// <summary>
   /// Synchronizes rotation data from ECS to transform
   /// </summary>
   [UpdateInGroup(typeof(AfterSimulationGroup))]
   // ReSharper disable once UnusedType.Global -> Handled by Auto-creation
   public partial class SyncUpToTransformSystem : SystemBase {
      #region [Fields]

      private TransformContainerSystem _transformContainerSystem;
      
      private ComponentLookup<SyncUpToTransform> _tagData;
      private ComponentLookup<DontSyncOneFrame> _dontSyncTags;
      private ComponentLookup<UpDirection> _upArray;

      #endregion

      protected override void OnCreate() {
         _tagData = GetComponentLookup<SyncUpToTransform>(true);
         _dontSyncTags = GetComponentLookup<DontSyncOneFrame>(true);
         _upArray = GetComponentLookup<UpDirection>(true);
         
         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
         
         EntityQuery query = new EntityQueryBuilder(WorldUpdateAllocator).WithAll<UpDirection, SyncUpToTransform>()
                                                                         .Build(EntityManager);
         RequireForUpdate(query);
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncUpDirectionToTransformSystem::OnUpdate (Main Thread)");

         _tagData.Update(this);
         _dontSyncTags.Update(this);
         _upArray.Update(this);
         
         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncUpToTransformJob syncPosJob = new SyncUpToTransformJob
                                           {
                                              Entities = alignedEntities,
                                              
                                              TagData = _tagData,
                                              UpArray = _upArray,
                                              DontSyncTags = _dontSyncTags
                                           };

         Dependency = syncPosJob.Schedule(_transformContainerSystem.RefArray, Dependency);
         alignedEntities.Dispose(Dependency);

         Profiler.EndSample();
      }

      [BurstCompile]
      private struct SyncUpToTransformJob : IJobParallelForTransform {
         [ReadOnly]
         public NativeArray<Entity> Entities;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<SyncUpToTransform> TagData;
         
         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<DontSyncOneFrame> DontSyncTags;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<UpDirection> UpArray;

         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];
            
            if (DontSyncTags.HasComponent(entity)) return;
            if (!TagData.HasComponent(entity)) return;
            if (!UpArray.TryGetComponent(entity, out UpDirection data)) return;

            transform.rotation = quaternion.LookRotation(math.forward(), data.Value);
         }
      }
   }
}