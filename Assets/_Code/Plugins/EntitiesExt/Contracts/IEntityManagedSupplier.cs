using System;
using System.Collections.Generic;
using Unity.Entities;

namespace EntitiesExt {
   /// <summary>
   /// Allows to setup entity with UnityEngine.Object components  via EntityManager.AddComponentObject / Add extension
   /// </summary>
   public interface IEntityManagedSupplier {
#if UNITY_EDITOR
      /// <summary>
      /// Used for gathering all ComponentTypes in editor, to convert them into Archetype for runtime
      /// </summary>
      void GatherEntityTypes(HashSet<Type> types);
#endif
      
      void SetupEntity(Entity entity, EntityManager entityManager, EntityCommandBuffer ecb);
   }
}