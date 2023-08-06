﻿using EntitiesExt;
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

      #endregion

      protected override void OnCreate() {
         EntityQuery query = new EntityQueryBuilder(WorldUpdateAllocator).WithAll<SyncUpToTransform, UpDirection>()
                                                                         .Build(EntityManager);
         RequireForUpdate(query);

         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncUpDirectionToTransformSystem::OnUpdate (Main Thread)");

         var tagData = GetComponentLookup<SyncUpToTransform>();
         var dontSyncTags = GetComponentLookup<DontSyncOneFrame>(true);
         var upArray = GetComponentLookup<UpDirection>();
         
         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncUpToTransformJob syncPosJob = new SyncUpToTransformJob
                                           {
                                              Entities = alignedEntities,
                                              TagData = tagData,
                                              UpArray = upArray,
                                              DontSyncTags = dontSyncTags
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
            if (!UpArray.HasComponent(entity)) return;

            UpDirection data = UpArray[entity];

            float3 forward = new float3(0, 0, 1);
            transform.rotation = quaternion.LookRotation(forward, data.Value);
         }
      }
   }
}