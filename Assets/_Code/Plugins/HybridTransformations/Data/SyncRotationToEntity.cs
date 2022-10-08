using Unity.Entities;

namespace HybridTransformations {
   /// <summary>
   /// Tag component that determines whether Rotation component should be synced from Transform each frame or not
   /// </summary>
   [System.Serializable]
   public struct SyncRotationToEntity : IComponentData { }
}