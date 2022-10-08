using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace HybridTransformations {
   [System.Serializable]
   public struct Position : IComponentData {
      public float3 Value;

      public Position(float3 position) { Value = position; }
      public Position(Vector3 position) { Value = position; }
      public Position(Position position) { Value = position.Value; }
   }
}