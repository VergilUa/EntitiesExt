using EntitiesExt;
using Unity.Entities;

namespace HybridTransformations {
   /// <summary>
   /// Removes <see cref="DontSyncOneFrame"/> and alike tags next frame
   /// </summary>
   [UpdateInGroup(typeof(AfterSimulationGroup))]
   public partial class ResetSyncStateSystem : SystemBase {
      #region [Fields]

      private EntityCommandBufferSystem _ecbSystem;
      private EntityQuery _dontSyncOneFrame;

      #endregion

      protected override void OnCreate() {
         base.OnCreate();
         
         _ecbSystem = World.GetOrCreateSystemManaged<NextFrameEntityCommandBufferSystem>();
         _dontSyncOneFrame = GetEntityQuery(ComponentType.ReadOnly<DontSyncOneFrame>());
         
         RequireForUpdate(_dontSyncOneFrame);
      }

      protected override void OnUpdate() {
         var ecb = _ecbSystem.CreateCommandBuffer();
         ecb.RemoveComponent<DontSyncOneFrame>(_dontSyncOneFrame, EntityQueryCaptureMode.AtPlayback);
      }
   }
}