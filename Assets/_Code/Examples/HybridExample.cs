using System;
using System.Collections.Generic;
using EntitiesExt;
using Unity.Entities;
using UnityEngine;

namespace Examples {
   /// <summary>
   /// Example of how to implement IEntitySupplier
   /// </summary>
   public class HybridExample : MonoBehaviour, IEntitySupplier {
      [SerializeField]
      private float _force = 13.37f;

      [SerializeField]
      private float _rotation = 69.420f;

      [SerializeField]
      private bool _useRng = true;
      
      [Space]
      [SerializeField]
      private Rigidbody _rgb;

      [SerializeField]
      private EntityBehaviour _entityBehaviour = default;

      public void SetupEntity(Entity entity, EntityCommandBuffer ecb) {
         _entityBehaviour.Add(this);
         _entityBehaviour.Add(_rgb);

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
         types.Add<HybridExample>();
         
         types.Add<Rigidbody, ForceTest, RotationTest>();
      }

      protected virtual void OnValidate() {
         gameObject.SetupEntityBehaviour(ref _entityBehaviour);
         if (_rgb == null) _rgb = GetComponentInChildren<Rigidbody>();
      }
#endif
   }
}