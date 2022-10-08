using Unity.Entities;

namespace EntitiesExt.SystemGroups {
   /// <summary>
   /// Update group that runs in the simulation group, but before any pure ECS system / jobs are scheduled.
   /// Use this one for the hybrid / Monobehaviour bridged systems
   /// </summary>
   [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
   public class BeforeSimulationGroup : ComponentSystemGroup { }
}