using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace EntitiesExt {
   public static class SystemExt {
      public static T AddTo<T>(this T disposable, List<IDisposable> subs)
         where T : IDisposable {
         subs.Add(disposable);
         return disposable;
      }

      /// <summary>
      /// Performs safe dispose on each subscription
      /// </summary>
      public static void SafeDispose(this List<IDisposable> subs) {
         foreach (IDisposable sub in subs) {
            sub?.Dispose();
         }
      }

      /// <summary>
      /// Safely disposes collection by checking if it has been created
      /// </summary>
      public static void SafeDispose<T>(this NativeArray<T> collection)
         where T : struct {
         if (collection.IsCreated) collection.Dispose();
      }

      /// <summary>
      /// Safely disposes collection by checking if it has been created
      /// </summary>
      public static void SafeDispose<T>(this NativeList<T> collection)
         where T : unmanaged {
         if (collection.IsCreated) collection.Dispose();
      }

      /// <summary>
      /// Safely disposes collection by checking if it has been created
      /// </summary>
      public static void SafeDispose<TKey>(this NativeParallelHashSet<TKey> collection)
         where TKey : unmanaged, IEquatable<TKey> {
         if (collection.IsCreated) collection.Dispose();
      }

      /// <summary>
      /// Safely disposes collection by checking if it has been created
      /// </summary>
      public static void SafeDispose<TKey, TValue>(this NativeParallelHashMap<TKey, TValue> collection)
         where TKey : unmanaged, IEquatable<TKey>
         where TValue : unmanaged {
         if (collection.IsCreated) collection.Dispose();
      }
      
      /// <summary>
      /// Safely disposes collection by checking if it has been created
      /// </summary>
      public static void SafeDispose<TKey, TValue>(this NativeParallelMultiHashMap<TKey, TValue> collection)
         where TKey : unmanaged, IEquatable<TKey>
         where TValue : unmanaged {
         if (collection.IsCreated) collection.Dispose();
      }
      
      /// <summary>
      /// Safely disposes collection by checking if it has been created
      /// </summary>
      public static void SafeDispose<TKey, TValue>(this UnsafeParallelHashMap<TKey, TValue> collection)
         where TKey : unmanaged, IEquatable<TKey>
         where TValue : unmanaged {
         if (collection.IsCreated) collection.Dispose();
      }
      
      /// <summary>
      /// Safely disposes collection by checking if it has been created
      /// </summary>
      public static void SafeDispose<TKey, TValue>(this UnsafeParallelMultiHashMap<TKey, TValue> collection)
         where TKey : unmanaged, IEquatable<TKey>
         where TValue : unmanaged {
         if (collection.IsCreated) collection.Dispose();
      }

      public static void SafeDispose<T>(this NativeQueue<T> collection)
         where T : unmanaged {
         if (collection.IsCreated) collection.Dispose();
      }

      public static JobHandle CombineWith(this JobHandle handle, JobHandle jobHandle) {
         return JobHandle.CombineDependencies(handle, jobHandle);
      }
   }
}