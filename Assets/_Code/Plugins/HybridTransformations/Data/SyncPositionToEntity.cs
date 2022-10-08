using Unity.Entities;

namespace HybridTransformations {
   /// <summary>
   /// Tag component that determines whether Position component should be synced from Transform each frame or not
   /// </summary>
   [System.Serializable]
   public struct SyncPositionToEntity : IComponentData { }
}