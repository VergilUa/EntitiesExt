using System;
using System.Collections.Generic;
using Unity.Collections;

namespace EntitiesExt {
   public static class SystemExt {
      public static T AddTo<T>(this T disposable, List<IDisposable> subs) where T : IDisposable {
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
      public static void SafeDispose<T>(this NativeArray<T> collection) where T : struct {
         if (collection.IsCreated) collection.Dispose();
      }
      
      /// <summary>
      /// Safely disposes collection by checking if it has been created
      /// </summary>
      public static void SafeDispose<T>(this NativeList<T> collection) where T : unmanaged {
         if (collection.IsCreated) collection.Dispose();
      }

      /// <summary>
      /// Safely disposes collection by checking if it has been created
      /// </summary>
      public static void SafeDispose<TKey>(this NativeParallelHashSet<TKey> collection)
         where TKey : unmanaged, IEquatable<TKey> {
         if (collection.IsCreated) collection.Dispose();
      }
   }
}
