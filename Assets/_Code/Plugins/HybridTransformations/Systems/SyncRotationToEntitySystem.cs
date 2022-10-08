using EntitiesExt.Data;
using EntitiesExt.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace HybridTransformations {
   /// <summary>
   /// Synchronizes rotation data from the transform to the ECS world
   /// </summary>
   // Replaced by codegen
   // ReSharper disable HeapView.DelegateAllocation
   // ReSharper disable HeapView.ClosureAllocation
   [UpdateInGroup(typeof(BeforeSimulationGroup))]
   [UpdateAfter(typeof(PhysSimGroup))]
   public partial class SyncRotationToEntitySystem : SystemBase {
      #region [Fields]

      private TransformContainerSystem _transformContainerSystem;

      #endregion

      protected override void OnCreate() {
         EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<SyncRotationToEntity>(),
                                            ComponentType.ReadWrite<Rotation>());
         RequireForUpdate(query);

         _transformContainerSystem = World.GetOrCreateSystemManaged<TransformContainerSystem>();
      }

      protected override void OnUpdate() {
         Profiler.BeginSample("SyncPositionToEntitySystem::OnUpdate (Main Thread)");

         ComponentLookup<Rotation> rotations = GetComponentLookup<Rotation>();
         ComponentLookup<SyncRotationToEntity> tagData = GetComponentLookup<SyncRotationToEntity>();

         var alignedEntities = _transformContainerSystem.AlignedEntities;

         SyncRotationToEntityJob syncPosJob = new SyncRotationToEntityJob
                                              {
                                                 Entities = alignedEntities,
                                                 TagData = tagData,
                                                 RotationArray = rotations
                                              };

         Dependency = syncPosJob.Schedule(_transformContainerSystem.RefArray, Dependency);
         alignedEntities.Dispose(Dependency);

         Profiler.EndSample();
      }

      [BurstCompile]
      private struct SyncRotationToEntityJob : IJobParallelForTransform {
         [ReadOnly]
         public NativeArray<Entity> Entities;

         [ReadOnly]
         [NativeDisableParallelForRestriction]
         public ComponentLookup<SyncRotationToEntity> TagData;

         [NativeDisableParallelForRestriction]
         public ComponentLookup<Rotation> RotationArray;

         public void Execute(int index, [ReadOnly] TransformAccess transform) {
            Entity entity = Entities[index];

            if (!TagData.HasComponent(entity)) return;
            if (!RotationArray.HasComponent(entity)) return;

            Quaternion pos = transform.rotation;

            Rotation data = RotationArray[entity];
            data.Value = pos;
            RotationArray[entity] = data;
         }
      }
   }
}