using System;
using System.Collections.Generic;
using Unity.Entities;

namespace EntitiesExt.Contracts {
   public interface IEntitySupplier {
#if UNITY_EDITOR
      /// <summary>
      /// Used for gathering all ComponentTypes in editor, to convert them into Archetype for runtime
      /// </summary>
      void GatherEntityTypes(HashSet<Type> types);
#endif
      void SetupEntity(Entity entity, EntityCommandBuffer ecb);
   }
}