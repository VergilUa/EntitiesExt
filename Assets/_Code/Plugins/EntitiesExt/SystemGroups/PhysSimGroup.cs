using Unity.Entities;

namespace EntitiesExt.SystemGroups {
   /// <summary>
   /// Group used for physical simulation / phys engine
   /// </summary>
   [UpdateInGroup(typeof(BeforeSimulationGroup))]
   public class PhysSimGroup : ComponentSystemGroup { }
}