using Unity.Entities;

namespace EntitiesExt {
   /// <summary>
   /// Group that is executed first in <see cref="BeforeSimulationGroup"/>.
   /// </summary>
   /// <remarks>Can be used to sort systems correctly via order attributes</remarks>
   [UpdateInGroup(typeof(BeforeSimulationGroup), OrderFirst = true)]
   public partial class BeginFrameGroup : ComponentSystemGroup { }
}
