﻿using Unity.Entities;

namespace HybridTransformations {
   /// <summary>
   /// Marks entity to sync position from entity to transform each frame
   /// </summary>
   [System.Serializable]
   public struct SyncPositionToTransform : IComponentData { }
}