using Unity.Entities;

namespace EntitiesExt {
   /// <summary>
   /// Performs structural changes after simulation is done including MonoBehaviours
   /// </summary>
   [UpdateInGroup(typeof(AfterSimulationGroup), OrderLast = true)]
   public class AfterSimulationEntityCommandBufferSystem : EntityCommandBufferSystem { }
}