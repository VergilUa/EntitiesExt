using Unity.Collections;
using Unity.Entities;

namespace EntitiesExt {
   [UpdateInGroup(typeof(BeginFrameGroup))]
   public partial class ArchetypeLookup : SystemBase {
      #region [Fields]

      private NativeParallelHashMap<ulong, EntityArchetype> _lookup;      

      #endregion
      
      protected override void OnCreate() {
         base.OnCreate();

         _lookup = new NativeParallelHashMap<ulong, EntityArchetype>(1024, Allocator.Persistent);
         
         // Don't run, only cleanup
         Enabled = false;
      }

      protected override void OnUpdate() {  }
      
      protected override void OnDestroy() {
         base.OnDestroy();
         
         if (_lookup.IsCreated) _lookup.Dispose();
      }

      public EntityArchetype GetCreateArchetype(SerializedArchetype archetype) =>
         GetCreateArchetype(archetype.ArchetypeUniqueHash, archetype.ComponentHashes);

      public EntityArchetype GetCreateArchetype(ulong uniqueHash, ulong[] typeHashes) {
         if (_lookup.TryGetValue(uniqueHash, out EntityArchetype arch)) 
            return arch;

         int typeCount = typeHashes.Length;

         NativeArray<ComponentType> types = new NativeArray<ComponentType>(typeCount, Allocator.Temp);
         for (int i = 0; i < typeCount; i++) {
            ulong stableHash = typeHashes[i];
            int typeIndex = TypeManager.GetTypeIndexFromStableTypeHash(stableHash);

            types[i] = ComponentType.FromTypeIndex(typeIndex);
         }

         EntityManager em = EntityManager;
         arch = em.CreateArchetype(types);

         _lookup[uniqueHash] = arch;
         return arch;
      }
   }
}