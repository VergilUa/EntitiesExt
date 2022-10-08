using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace HybridTransformations {
   [System.Serializable]
   public struct LocalPosition : IComponentData {
      public float3 Value;

      public LocalPosition(float3 position) { Value = position; }
      public LocalPosition(Vector3 position) { Value = position; }
   }
}