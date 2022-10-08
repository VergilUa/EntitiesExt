using Unity.Entities;

namespace HybridTransformations {
   /// <summary>
   /// Marks entity to synchronize <see cref="LocalScale"/> data from ECS to Transform local scale
   /// </summary>
   [System.Serializable]
   public struct SyncLocalScaleToTransform : IComponentData { }
}