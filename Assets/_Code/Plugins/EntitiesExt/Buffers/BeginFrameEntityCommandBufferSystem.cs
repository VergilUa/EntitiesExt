using Unity.Entities;

namespace EntitiesExt {
   /// <summary>
   /// Performs structural changes at the beginning of the frame before any work is performed.
   /// Also Generates a single EntityCommandBuffer for AfterSimulationGroup writing. E.g. from entity container.
   /// Note that generated buffer is not intended to be used in actual SimulationSystemGroup Begin -- End time.
   /// </summary>
   [UpdateInGroup(typeof(BeginFrameGroup))]
   public partial class BeginFrameEntityCommandBufferSystem : EntityCommandBufferSystem {
      #region [Properties]

      public EntityCommandBuffer ECB => _ecb;
      private EntityCommandBuffer _ecb;

      #endregion
      
      protected override void OnCreate() {
         base.OnCreate();
         
         _ecb = CreateCommandBuffer();
      }

      protected override void OnUpdate() {
         base.OnUpdate(); // Apply current frame state changes
         _ecb = CreateCommandBuffer(); // Generate a new buffer for the next frame 
      }
   }
}