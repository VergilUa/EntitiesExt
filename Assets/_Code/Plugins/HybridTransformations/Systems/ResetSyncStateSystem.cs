using EntitiesExt;
using Unity.Burst;
using Unity.Entities;

namespace HybridTransformations {
   /// <summary>
   /// Removes <see cref="DontSyncOneFrame"/> and alike tags next frame
   /// </summary>
   // Cases replaced by Entities Codegen:
   // ReSharper disable HeapView.ClosureAllocation
   // ReSharper disable HeapView.DelegateAllocation
   // ReSharper disable HeapView.PossibleBoxingAllocation
   [UpdateInGroup(typeof(AfterSimulationGroup))]
   public partial struct ResetSyncStateSystem : ISystem {
      #region [Fields]

      private EntityQuery _dontSyncOneFrame;

      #endregion

      [BurstCompile]
      public void OnCreate(ref SystemState state) {
         _dontSyncOneFrame = new EntityQueryBuilder(state.WorldUpdateAllocator).WithAll<DontSyncOneFrame>()
                                                                               .Build(ref state);
         
         state.RequireForUpdate<NextFrameEntityCommandBufferSystem.Singleton>();
         state.RequireForUpdate(_dontSyncOneFrame);
      }

      [BurstCompile]
      public void OnUpdate(ref SystemState state) {
         var ecb = SystemAPI.GetSingleton<NextFrameEntityCommandBufferSystem.Singleton>()
                            .CreateCommandBuffer(state.WorldUnmanaged);
         
         ecb.RemoveComponent<DontSyncOneFrame>(_dontSyncOneFrame, EntityQueryCaptureMode.AtRecord);
      }
   }
}