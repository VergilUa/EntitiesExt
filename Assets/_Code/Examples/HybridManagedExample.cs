using System;
using System.Collections.Generic;
using EntitiesExt;
using Unity.Entities;
using UnityEngine;

namespace Examples {
   /// <summary>
   /// Example of how to implement IEntityManagedSupplier.
   ///
   /// Difference with <see cref="IEntitySupplier"/> is that there's no referencing to the EntityBehaviour involved.
   /// So if you're not accessing EntityBehaviour in runtime, you can use <see cref="IEntityManagedSupplier"/>
   /// to author UnityEngine.Objects
   /// </summary>
   //
   // Ensure EntityBehaviour exists to pick up this behaviour
   // Note that without OnValidate testing there's no way to ensure only one behaviour exist
   // as well as hierarchy tests. So use this for root only, otherwise use EntityBehaviour similar to IEntitySupplier
   [RequireComponent(typeof(EntityBehaviour))] 
   public class HybridManagedExample : MonoBehaviour, IEntityManagedSupplier {
      [SerializeField]
      private float _force = 13.37f;

      [SerializeField]
      private float _rotation = 69.420f;

      [SerializeField]
      private bool _useRng = true;
      
      [Space]
      [SerializeField]
      private Rigidbody _rgb;

      public void SetupEntity(Entity entity, EntityManager entityManager, EntityCommandBuffer ecb) {
         entityManager.Add(entity, this);
         entityManager.Add(entity, _rgb);

         ecb.SetComponent(entity,
                          new ForceTest
                          {
                             Value = _force,
                             UseRng = _useRng
                          });

         var buffer = ecb.SetBuffer<RotationTest>(entity);
         buffer.Add(new RotationTest
                    {
                       Value = _rotation
                    });
      }

#if UNITY_EDITOR
      public void GatherEntityTypes(HashSet<Type> types) {
         // Used as an example, for querying over specific MonoBehaviour, not used in the example system
         types.Add<HybridManagedExample>();
         
         types.Add<Rigidbody, ForceTest, RotationTest>();
      }

      protected virtual void OnValidate() {
         if (_rgb == null) _rgb = GetComponentInChildren<Rigidbody>();
      }
#endif
   }
}