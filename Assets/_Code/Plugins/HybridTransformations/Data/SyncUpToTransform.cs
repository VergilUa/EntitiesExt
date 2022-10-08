using Unity.Entities;

namespace HybridTransformations {
   /// <summary>
   /// Marks entity to sync <see cref="UpDirection"/> from entity to transform each frame
   /// </summary>
   [System.Serializable]
   public struct SyncUpToTransform : IComponentData { }
}