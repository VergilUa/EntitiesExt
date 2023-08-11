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
      
      private ComponentLookup<SyncRotationToTransform> _tagData;
      private ComponentLookup<DontSyncOneFrame> _dontSyncTags;
      private ComponentLookup<Rotation> _rotations;

      #endregion

      protected override void OnCreate() {
         _tagData = GetComponentLookup<SyncRotationToTransform>(true);
         _dontSyncTags = GetComponentLookup<DontSyncOneFrame>(true);
         _rotations = GetComponentLookup<Rotation>(true);
          
         EntityQuery query = new EntityQueryBuilder(WorldUpdateAllocator).WithAll<Rotation, SyncRotationToTransform>()
                                                                         .Build(EntityManager);
         RequireForUpdate(query);
         
         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncRotationToTransformSystem::OnUpdate (Main Thread)");

         _tagData.Update(this);
         _dontSyncTags.Update(this);
         _rotations.Update(this);
         
         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncRotationToTransformJob syncPosJob = new SyncRotationToTransformJob
                                                 {
                                                    Entities = alignedEntities, 
                                                    TagData = _tagData,
                                                    DontSyncTags = _dontSyncTags,
                                                    Rotations = _rotations
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
         public ComponentLookup<Rotation> Rotations;

         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];
            
            if (DontSyncTags.HasComponent(entity)) return;
            if (!TagData.HasComponent(entity)) return;
            if (!Rotations.TryGetComponent(entity, out Rotation data)) return;
               
            transform.rotation = data.Value;
         }
      }
   }
}