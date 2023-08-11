using EntitiesExt;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace HybridTransformations {
   /// <summary>
   /// Synchronizes local rotation data from ECS to transform
   /// </summary>
   [UpdateInGroup(typeof(AfterSimulationGroup))]
   public partial class SyncLocalRotationToTransformSystem : SystemBase {
      #region [Fields]

      private TransformContainerSystem _transformContainerSystem;
      
      private ComponentLookup<SyncLocalRotationToTransform> _tagData;
      private ComponentLookup<DontSyncOneFrame> _dontSyncTags;
      private ComponentLookup<LocalRotation> _rotationArray;

      #endregion

      protected override void OnCreate() {
         _tagData = GetComponentLookup<SyncLocalRotationToTransform>(true);
         _dontSyncTags = GetComponentLookup<DontSyncOneFrame>(true);
         _rotationArray = GetComponentLookup<LocalRotation>(true);
         
         EntityQuery query = new EntityQueryBuilder(WorldUpdateAllocator)
                             .WithAll<LocalRotation, SyncLocalRotationToTransform>()
                             .Build(EntityManager);
         
         RequireForUpdate(query);
         
         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
      }
      
      protected override void OnUpdate() {
         Profiler.BeginSample("SyncRotationToTransformSystem::OnUpdate (Main Thread)");

         _tagData.Update(this);
         _dontSyncTags.Update(this);
         _rotationArray.Update(this);

         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncLocalRotationToTransformJob syncPosJob = new SyncLocalRotationToTransformJob
                                                      {
                                                         Entities = alignedEntities,
                                                         TagData = _tagData,
                                                         DontSyncTags = _dontSyncTags,
                                                         RotationArray = _rotationArray
                                                      };
         Dependency = syncPosJob.Schedule(_transformContainerSystem.RefArray, Dependency);
         alignedEntities.Dispose(Dependency);

         Profiler.EndSample();
      }

      [BurstCompile]
      private struct SyncLocalRotationToTransformJob : IJobParallelForTransform {
         [ReadOnly]
         public NativeArray<Entity> Entities;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<SyncLocalRotationToTransform> TagData;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<DontSyncOneFrame> DontSyncTags;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<LocalRotation> RotationArray;

         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];
            
            if (DontSyncTags.HasComponent(entity)) return;
            if (!TagData.HasComponent(entity)) return;
            if (!RotationArray.TryGetComponent(entity, out LocalRotation data)) return;

            transform.localRotation = data.Value;
         }
      }
   }
}