using Unity.Entities;

namespace EntitiesExt {
   /// <summary>
   /// Performs structural changes at the beginning of the (next) frame before any work is performed.
   /// </summary>
   /// <remarks>
   /// This one executes before <see cref="BeginFrameEntityCommandBufferSystem"/>,
   /// so its safe to put next frame structural changes here
   /// </remarks>
   [UpdateInGroup(typeof(BeginFrameGroup))]
   [UpdateBefore(typeof(BeginFrameEntityCommandBufferSystem))]
   public class NextFrameEntityCommandBufferSystem : EntityCommandBufferSystem { }
}
