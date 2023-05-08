using System;
using Unity.Entities;

namespace Examples {
   /// <summary>
   /// Data for the examples involving rotation (to cover buffer example as well)
   /// </summary>
   [Serializable]
   public struct RotationTest : IBufferElementData {
      public float Value;
   }
}