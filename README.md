# EntitiesExt
### Runtime authoring solution for Entities & misc tooling

### Includes:
* EntityBehaviour - Handles Entity creation and lifecycle that is connected to the MonoBehaviour;
* IEntitySupplier - Contract which ensures generic workflow when authoring entities from MonoBehaviours;
* SerializedArchetype - Stores EntityArchetype in uint[] format to be able to serialize / deserialize it as part of MonoBehaviour;
* ArchetypeLookup - Allows caching deserialized SerializedArchetypes in runtime;
* EntityReference / ReferencingExt - Allows accessing "main" Entity if there's a need to reference other entity;
* EntitiesBridge / SystemExt - Contains useful extension methods for MonoBehaviour <-> Entities communication & system utilities;
* Extra buffers system - Allow to insert MonoBehaviour "Update"s into Entities loop without stalling jobs;
* EntityTransform - Inserts specified Transform (UnityEngine.Transform) into TransformAccessArray for sync jobs to process;

### How to:

### Generate an entity in runtime:
1. Implement IEntitySupplier inside required MonoBehaviour, ScriptableObject, or any other class you want;
2. Attach EntityBehaviour to the specified gameObject:
```
[SerializedField]
private EntityBehaviour _entityBehaviour;
```
Declare in OnValidate:
```
gameObject.SetupEntityBehaviour(ref _entityBehaviour);
```
Alternatively, you can attach EntityBehaviour to the gameObject manually via Inspector.

3. Implement GatherEntityTypes to insert types of components that should be added to the generated Entity / EntityArchetype:
```
#if UNITY_EDITOR
      public void GatherEntityTypes(HashSet<Type> types) {
         // Used as an example, for querying over specific MonoBehaviour, not actually required
         types.Add<HybridExample>();
         types.Add<Rigidbody, ForceTest, RotationTest>();
      }
#endif
```

4. Implement SetupEntity to generate / add data to the World via EntityCommandBuffer. 
```
      public void SetupEntity(Entity entity, EntityCommandBuffer ecb) {
         // Managed objects can be added by accessing EntityBehaviour:
         _entityBehaviour.Add(this);
         _entityBehaviour.Add(_rgb);

         // Entity related IComponentData or IBufferElementData can be added via EntityCommandBuffer:
         ecb.SetComponent(entity,
                          new ForceTest
                          {
                             Value = _force,
                          });

         var buffer = ecb.SetBuffer<RotationTest>(entity);
         buffer.Add(new RotationTest
                    {
                       Value = _rotation
                    });
      }
```

5. (Optional) If you're using custom classes, make sure to call respective GatherEntityTypes / SetupEntity in corresponding manner. E.g. from MonoBehaviour.
6. Use systems as usual to access / process data. If you need access to the Entity and its data from MonoBehaviours - use according EntityBehaviour methods. 
EntityBehaviour can also be used to queue up via EntityCommandBuffer structural changes, such as adding / seting / removing components. (See Lifecycle)


### How it works:
In editor:
1. EntityBehaviour processes attached components to the gameObject hierarchy, gathers types required for EntityArchetype to be generated in Editor via GatherEntityTypes (by IEntitySupplier contract). 
These are serialized from Stable Hash (see TypeManager.GetTypeIndexFromStableTypeHash) in EntityBehaviour as uint[].
2. Unique hash is generated from each attached component as uint. This hash is used in ArchetypeLookup to ensure O(1) speed for fetching previously generated EntityArchetype(s).

In runtime:
1. EntityBehaviour creates an entity in OnEnable based on deserialized hashes. 
(Alternatively, EntityBehaviour.Initialize() method can be called to ensure Entity is initialized in case if OnEnable order is undefined between MonoBehaviours)
2. At this step all required references to the World, EntityManger and systems & buffer are set;
3. Stable type hashes & unique hash are passed to the ArchetypeLookup system which either:
 - Generates new EntityArchetype if its not present in the lookup;
 - Or returns previouly generated EntityArchetype based on unique hash;
4. EntityBehaviour generates an Entity via EntityManager.CreateEntity(EntityArchetype);
5. EntityBehaviour calls SetupEntity and passes Entity + EntityCommandBuffer to the IEntitySupplier implementation.

## Lifecycle (important)
Entities generated by EntityBehaviour exist only if GameObject is enabled. 
If GameObject which has EntityBehaviour is disabled - OnDisable will be called and according Entity will be Destroy'ed via EntityCommandBuffer.

This may seem inconvinient at first, but at the same time:
- Its really handy to have completely clean state upon pooling / commissioning MonoBehaviours.
- Entity is automatically cleaned up for you without triggering structural changes instantly (uses ECB).
- No way to mess up the state / link between MonoBehaviour / Entities;
- Extra checks are in place if EntityBehaviour initialization is called on disabled GO to ensure user domain code does not desync state 
(Error is generated in the Console window)

If you need to stop "Update" use either Entities enableable components feature, or structural changes + system queries to filter out logic that should not run.

