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
   // ReSharper disable once UnusedType.Global -> Handled by Auto-creation
   public partial class SyncLocalRotationToTransformSystem : SystemBase {
      #region [Fields]

      private TransformContainerSystem _transformContainerSystem;

      #endregion

      protected override void OnCreate() {
         EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<SyncLocalRotationToTransform>(),
                                            ComponentType.ReadOnly<LocalRotation>());
         RequireForUpdate(query);
         
         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
      }
      
      protected override void OnUpdate() {
         Profiler.BeginSample("SyncRotationToTransformSystem::OnUpdate (Main Thread)");

         var tagData = GetComponentLookup<SyncLocalRotationToTransform>(true);
         var dontSyncTags = GetComponentLookup<DontSyncOneFrame>(true);
         var rotationArray = GetComponentLookup<LocalRotation>(true);

         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncLocalRotationToTransformJob syncPosJob = new SyncLocalRotationToTransformJob
                                                      {
                                                         Entities = alignedEntities,
                                                         TagData = tagData,
                                                         DontSyncTags = dontSyncTags,
                                                         RotationArray = rotationArray
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
            if (!RotationArray.HasComponent(entity)) return;

            LocalRotation data = RotationArray[entity];
            transform.localRotation = data.Value;
         }
      }
   }
}