using Unity.Entities;

namespace EntitiesExt {
   /// <summary>
   /// Update group that runs in the simulation group, but after all ECS system / jobs ran / scheduled.
   /// This is done in PreLateUpdate
   /// Use this one for the hybrid / MonoBehaviour bridged systems
   /// </summary>
   [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
   [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
   public partial class AfterSimulationGroup : ComponentSystemGroup { }
}