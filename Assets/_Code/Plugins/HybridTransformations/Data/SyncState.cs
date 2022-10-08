using System;
using System.Runtime.CompilerServices;
using EntitiesExt;
using Unity.Entities;
using UnityEngine;

namespace HybridTransformations {
   /// <summary>
   /// Describes which state should be synced to entity or transform
   /// </summary>
   [Flags]
   [Serializable]
   public enum SyncState {
      None = 0,
      SyncPositionToEntity = 1,
      SyncRotationToEntity = 1 << 1,
      SyncPositionToTransform = 1 << 2,
      SyncRotationToTransform = 1 << 3,
   }

   public static class SyncStateExt {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void AddIfTrue(ref this SyncState status, bool value, SyncState flag) {
         if (value) status |= flag;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Add(ref this SyncState status, SyncState flag)
         => status |= flag;
      
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void Remove(ref this SyncState status, SyncState flag)
         => status &= ~flag;

      /// <summary>
      /// Packs multi-component state into single int, that can be used later on to restore same Entity state
      /// </summary>
      /// <returns></returns>
      public static SyncState PackSyncState(this EntityBehaviour entityBehaviour) {
         if (!entityBehaviour.IsInitialized) return SyncState.None;

         SyncState state = SyncState.None;
         
         Entity entity = entityBehaviour.Entity;
         EntityManager em = entityBehaviour.EntityManager;
         
         state.AddIfTrue(em.HasComponent<SyncPositionToEntity>(entity), SyncState.SyncPositionToEntity);
         state.AddIfTrue(em.HasComponent<SyncRotationToEntity>(entity), SyncState.SyncRotationToEntity);
         state.AddIfTrue(em.HasComponent<SyncPositionToTransform>(entity), SyncState.SyncPositionToTransform);
         state.AddIfTrue(em.HasComponent<SyncRotationToTransform>(entity), SyncState.SyncRotationToTransform);

         return state;
      }

      /// <summary>
      /// Unpacks component state as changes to the buffer
      /// </summary>
      public static void UnPackTo(this SyncState state, EntityCommandBuffer ecb, Entity entity) {
         if ((state & SyncState.SyncPositionToEntity) == SyncState.SyncPositionToEntity)
            ecb.AddComponent<SyncPositionToEntity>(entity);
         else 
            ecb.RemoveComponent<SyncPositionToEntity>(entity);
         
         if ((state & SyncState.SyncPositionToTransform) == SyncState.SyncPositionToTransform)
            ecb.AddComponent<SyncPositionToTransform>(entity);
         else 
            ecb.RemoveComponent<SyncPositionToTransform>(entity);
       
         if ((state & SyncState.SyncRotationToEntity) == SyncState.SyncRotationToEntity)
            ecb.AddComponent<SyncRotationToEntity>(entity);
         else
            ecb.RemoveComponent<SyncRotationToEntity>(entity);
         
         if ((state & SyncState.SyncRotationToTransform) == SyncState.SyncRotationToTransform)
            ecb.AddComponent<SyncRotationToTransform>(entity);
         else
            ecb.RemoveComponent<SyncRotationToTransform>(entity);
      }
      
      /// <summary>
      /// Unpacks component state as changes to the buffer fetched from EntityContainer
      /// </summary>
      public static void UnPackTo(this SyncState state, EntityBehaviour entityBehaviour) {
         EntityCommandBuffer ecb = entityBehaviour.Buffer;
         Entity entity = entityBehaviour.Entity;
         
         Debug.Assert(entity != Entity.Null);
         
         if ((state & SyncState.SyncPositionToEntity) == SyncState.SyncPositionToEntity)
            ecb.AddComponent<SyncPositionToEntity>(entity);
         else 
            ecb.RemoveComponent<SyncPositionToEntity>(entity);
         
         if ((state & SyncState.SyncPositionToTransform) == SyncState.SyncPositionToTransform)
            ecb.AddComponent<SyncPositionToTransform>(entity);
         else 
            ecb.RemoveComponent<SyncPositionToTransform>(entity);
       
         if ((state & SyncState.SyncRotationToEntity) == SyncState.SyncRotationToEntity)
            ecb.AddComponent<SyncRotationToEntity>(entity);
         else
            ecb.RemoveComponent<SyncRotationToEntity>(entity);
         
         if ((state & SyncState.SyncRotationToTransform) == SyncState.SyncRotationToTransform)
            ecb.AddComponent<SyncRotationToTransform>(entity);
         else
            ecb.RemoveComponent<SyncRotationToTransform>(entity);
      }
   }
}
