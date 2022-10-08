using System;
using System.Collections.Generic;
using EntitiesExt;
using EntitiesExt.Contracts;
using HybridTransformations;
using Unity.Entities;
using UnityEngine;

namespace Examples {
   /// <summary>
   /// Synchronizes position, rotation to entity side
   /// </summary>
   [RequireComponent(typeof(EntityTransform))]
   public class SyncToEntityExample : MonoBehaviour, IEntitySupplier {
      public void SetupEntity(Entity entity, EntityCommandBuffer ecb) { }

#if UNITY_EDITOR
      public void GatherEntityTypes(HashSet<Type> types) {
         types.Add<Position, Rotation>();
         types.Add<SyncPositionToEntity, SyncRotationToEntity>();
      }
#endif
   }
}