using Unity.Entities;

namespace EntitiesExt {
   /// <summary>
   /// Group used for physical simulation / phys engine
   /// </summary>
   [UpdateInGroup(typeof(BeforeSimulationGroup))]
   public class PhysSimGroup : ComponentSystemGroup { }
}