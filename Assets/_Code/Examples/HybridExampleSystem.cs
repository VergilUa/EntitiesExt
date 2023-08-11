using Unity.Entities;
using UnityEngine;

namespace Examples {
   /// <summary>
   /// Simple managed system to run basic examples
   /// </summary>
   [RequireMatchingQueriesForUpdate]
   public partial class HybridExampleSystem : SystemBase {
      protected override void OnUpdate() {
         Entities.ForEach((ref DynamicBuffer<RotationTest> rotationTestBuffer,
                           in Rigidbody rgb,
                           in ForceTest forceTestData) => {
                    float rnd = Random.value;
                    Vector3 dir = Vector3.up;

                    if (forceTestData.UseRng) {
                       dir = rnd <= 0.5f ? Vector3.up : Vector3.down;
                       rgb.useGravity = false;
                    }

                    rgb.AddForce(forceTestData.Value * dir);

                    for (int i = 0; i < rotationTestBuffer.Length; i++) {
                       var rotationTestData = rotationTestBuffer[i];
                       rgb.AddTorque(rotationTestData.Value * Vector3.up);
                    }
                 })
                 .WithAny<HybridExample, HybridManagedExample>()
                 .WithoutBurst()
                 .Run();
      }
   }
}