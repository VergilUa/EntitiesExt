using Unity.Entities;

namespace EntitiesExt.SystemGroups {
   /// <summary>
   /// Group used for physical simulation / phys engine
   /// </summary>
   [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
   public class PhysSimGroup : ComponentSystemGroup { }
}