using Unity.Entities;
using UnityEngine;

namespace Examples {
   public partial class HybridExampleSystem : SystemBase {
      protected override void OnUpdate() {
         Entities.ForEach((in Rigidbody rgb,
                           in ForceTest forceTestData,
                           in DynamicBuffer<RotationTest> rotationTestBuffer) => {
                    float rnd = Random.value;
                    Vector3 dir = rnd <= 0.5f ? Vector3.up : Vector3.down;
                    
                    rgb.AddForce(forceTestData.Value * dir);
                    rgb.useGravity = false;

                    for (int i = 0; i < rotationTestBuffer.Length; i++) {
                       var rotationTestData = rotationTestBuffer[i];
                       rgb.AddTorque(rotationTestData.Value * Vector3.up);
                    }
                 })
                 .WithAll<HybridExample>()
                 .WithoutBurst()
                 .Run();
      }
   }
}