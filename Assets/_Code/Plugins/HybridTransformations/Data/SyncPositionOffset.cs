using System;
using Unity.Entities;
using Unity.Mathematics;

namespace HybridTransformations {
   /// <summary>
   /// Offset data used for transform / entity <see cref="Position"/> synchronization
   /// </summary>
   [Serializable]
   public struct SyncPositionOffset : IComponentData {
      public float3 Value;

      public SyncPositionOffset(float3 value) {
         Value = value;
      }
   }
}