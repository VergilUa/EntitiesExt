using System;
using Unity.Entities;

namespace Examples {
   /// <summary>
   /// Data for the examples involving forces
   /// </summary>
   [Serializable]
   public struct ForceTest : IComponentData {
      public float Value;
      public bool UseRng;
   }
}
