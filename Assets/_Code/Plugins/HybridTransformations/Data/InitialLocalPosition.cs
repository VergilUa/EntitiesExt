using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace HybridTransformations {
   [System.Serializable]
   public struct InitialLocalPosition : IComponentData {
      public float3 Value;

      public InitialLocalPosition(float3 position) { Value = position; }
      public InitialLocalPosition(Vector3 position) { Value = position; }
   }
}