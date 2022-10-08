using System;
using Unity.Entities;

namespace EntitiesExt {
   [Serializable]
   public struct EntityReference : IComponentData {
      public Entity Value;

      public EntityReference(Entity value) => Value = value;
   }
}