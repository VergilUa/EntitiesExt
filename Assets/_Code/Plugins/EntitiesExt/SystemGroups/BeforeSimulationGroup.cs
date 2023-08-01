using Unity.Entities;

namespace EntitiesExt {
   /// <summary>
   /// Update group that runs in the simulation group, but before any pure ECS system / jobs are scheduled.
   /// Use this one for the hybrid / MonoBehaviour bridged systems
   /// </summary>
   [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
   public partial class BeforeSimulationGroup : ComponentSystemGroup { }
}