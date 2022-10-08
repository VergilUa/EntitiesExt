using Unity.Entities;

namespace HybridTransformations {
   /// <summary>
   /// Marks entity to sync rotation from entity to transform each frame
   /// </summary>
   [System.Serializable]
   public struct SyncRotationToTransform : IComponentData { }
}
