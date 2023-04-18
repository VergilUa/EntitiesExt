using EntitiesExt;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace HybridTransformations {
   /// <summary>
   /// Synchronizes rotation data from ECS to transform
   /// </summary>
   [UpdateInGroup(typeof(AfterSimulationGroup))]
   // ReSharper disable once UnusedType.Global -> Handled by Auto-creation
   public partial class SyncRotationToTransformSystem : SystemBase {
      #region [Fields]

      private TransformContainerSystem _transformContainerSystem;

      #endregion

      protected override void OnCreate() {
         EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<SyncRotationToTransform>(),
                                            ComponentType.ReadOnly<Rotation>());
         RequireForUpdate(query);
         
         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncRotationToTransformSystem::OnUpdate (Main Thread)");

         var tagData = GetComponentLookup<SyncRotationToTransform>(true);
         var dontSyncTags = GetComponentLookup<DontSyncOneFrame>(true);
         var rotationArray = GetComponentLookup<Rotation>(true);
         
         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncRotationToTransformJob syncPosJob = new SyncRotationToTransformJob
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
      private struct SyncRotationToTransformJob : IJobParallelForTransform {
         [ReadOnly]
         public NativeArray<Entity> Entities;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<SyncRotationToTransform> TagData;
         
         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<DontSyncOneFrame> DontSyncTags;
         
         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<Rotation> RotationArray;

         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];
            if (DontSyncTags.HasComponent(entity)) return;
            if (!TagData.HasComponent(entity)) return;
            if (!RotationArray.HasComponent(entity)) return;
               
            Rotation data = RotationArray[entity];
            transform.rotation = data.Value;
         }
      }
   }
}