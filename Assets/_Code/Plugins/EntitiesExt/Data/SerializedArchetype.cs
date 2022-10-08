using System;
using Unity.Entities;

namespace EntitiesExt {
   /// <summary>
   /// Determines entity archetype that can be saved in MonoBehaviour via single field
   /// </summary>
   [Serializable]
   public struct SerializedArchetype {
      public ulong ArchetypeUniqueHash;
      public ulong[] ComponentHashes;

      public EntityArchetype AsArchetype(EntityManager em) =>
         ArchetypeLookup.GetCreateArchetype(ArchetypeUniqueHash, ComponentHashes, em);
   }
}
