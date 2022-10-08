using System.Runtime.CompilerServices;
using Unity.Entities;

namespace EntitiesExt {
   /// <summary>
   /// Extensions for use in system that require <see cref="EntityReference"/>
   /// </summary>
   public static class ReferencingExt {
      /// <summary>
      /// Obtains a main entity if there's an entity reference assigned to this entity,
      /// and patches up 'entity' parameter
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void AsMainEntity(this ref Entity entity,
                                      ref ComponentLookup<EntityReference> entityReferences) {
         if (!entityReferences.HasComponent(entity)) return;
         entity = entityReferences[entity].Value;
      }
   }
}