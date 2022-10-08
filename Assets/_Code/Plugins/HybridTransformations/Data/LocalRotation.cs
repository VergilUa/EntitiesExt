using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace HybridTransformations {
   [System.Serializable]
   public struct LocalRotation : IComponentData {
      public quaternion Value;

      public LocalRotation(quaternion rotation) { Value = rotation; }
   }
}