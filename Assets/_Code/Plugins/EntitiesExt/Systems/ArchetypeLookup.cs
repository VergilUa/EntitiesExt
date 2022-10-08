using EntitiesExt.SystemGroups;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace EntitiesExt {
   [UpdateInGroup(typeof(BeginFrameGroup))]
   public partial class ArchetypeLookup : SystemBase {
      private static NativeParallelHashMap<ulong, EntityArchetype> _lookup;
      
      [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
      private static void Initialize() {
         _lookup = new NativeParallelHashMap<ulong, EntityArchetype>(1024, Allocator.Persistent);
         TypeManager.Initialize();
      }

      public static EntityArchetype GetCreateArchetype(ulong uniqueHash,
                                                       ulong[] typeHashes,
                                                       EntityManager entityManager) {
         if (_lookup.TryGetValue(uniqueHash, out EntityArchetype arch)) 
            return arch;

         int typeCount = typeHashes.Length;

         NativeArray<ComponentType> types = new NativeArray<ComponentType>(typeCount, Allocator.Temp);
         for (int i = 0; i < typeCount; i++) {
            ulong stableHash = typeHashes[i];
            int typeIndex = TypeManager.GetTypeIndexFromStableTypeHash(stableHash);

            types[i] = ComponentType.FromTypeIndex(typeIndex);
         }

         arch = entityManager.CreateArchetype(types);

         _lookup[uniqueHash] = arch;
         return arch;
      }

      protected override void OnCreate() {
         base.OnCreate();

         // Don't run, only cleanup
         Enabled = false;
      }

      protected override void OnUpdate() {  }
      
      protected override void OnDestroy() {
         base.OnDestroy();
         
         if (_lookup.IsCreated) _lookup.Dispose();
      }
   }
}