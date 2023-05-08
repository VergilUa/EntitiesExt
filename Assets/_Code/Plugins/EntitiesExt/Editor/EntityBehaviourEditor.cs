using System;
using System.Collections.Generic;
using System.Text;
using EditorExt;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EntitiesExt {
   [CustomEditor(typeof(EntityBehaviour))]
   public sealed class EntityBehaviourEditor : InspectorBase {
      #region [Properties]

      private EntityBehaviour Target => target as EntityBehaviour;
      
      private string WorldNamesKey {
         get
         {
            string uniqueProjectName = Application.productName;
            return EditorPrefsWorldNames + uniqueProjectName;
         }
      }
      private const string EditorPrefsWorldNames = "EditorPrefs_WorldNames_";

      private bool HasCachedWorldNames => _worldNamesArr != null && _worldNamesArr.Length > 0;

      #endregion

      #region [Fields]

      private SerializedProperty _insertToWorld;
      private SerializedProperty _componentHashes;
      
      private readonly List<ComponentDisplayData> _addedComponents = new List<ComponentDisplayData>(4);
      private static readonly HashSet<Type> TypeBuffer = new HashSet<Type>(16);
      private static readonly List<MonoBehaviour> UsageBuffer = new List<MonoBehaviour>();

      private const string OpenScript = "Open Script";
      private const string PathSeparator = "/";
      private const string SelectObject = "Select GameObject";
      private const string LaunchToCache = "Enter playmode and select EntityBehaviour to cache World names. \n"
                                           + "Names are stored in EditorPrefs and are unique to each productName \\ project.";
      
      private string[] _worldNamesArr = Array.Empty<string>();
      private WorldNames _worldNames;

      #endregion

      protected override void OnEnable() {
         base.OnEnable();

         SetupWorldNames();
         
         _insertToWorld = FindProp(nameof(_insertToWorld));
         _componentHashes = FindProp(nameof(_componentHashes));
      }

      public override void OnInspectorGUI() {
         base.OnInspectorGUI();

         serializedObject.Update();

         DrawHelpBox();
         DrawWorldInsertProp();
         DrawAddedComponents();
         
         serializedObject.ApplyModifiedProperties();
      }

      /// <summary>
      /// Cache existing world names when inspector is selected in playmode.
      /// </summary>
      private void SetupWorldNames() {
         if (!Application.isPlaying) {
            LoadExistingWorldNames();
            return;
         }

         LoadExistingWorldNames();

         List<string> names = _worldNames.Names ?? new List<string>();
         names.Clear();

         var allWorlds = World.All;

         foreach (World world in allWorlds) {
            names.Add(world.Name);
         }

         _worldNames.Names = names;
         
         string json = JsonUtility.ToJson(_worldNames);
         EditorPrefs.SetString(WorldNamesKey, json);
      }

      private void LoadExistingWorldNames() {
         string worldNamesJSON = EditorPrefs.GetString(WorldNamesKey, string.Empty);
         if (string.IsNullOrEmpty(worldNamesJSON)) return;
         
         _worldNames = JsonUtility.FromJson<WorldNames>(worldNamesJSON);
         _worldNamesArr = _worldNames.Names.ToArray();
      }

      private void DrawHelpBox() {
         if (_worldNamesArr != null && _worldNamesArr.Length != 0) return;
         
         // Notify how World name caching works
         HelpBox(LaunchToCache);
      }

      private void DrawWorldInsertProp() {
         using var disabled = Disabled(Application.isPlaying);
         using var horizontal = Horizontal;

         ChangeCheck();

         PropField(_insertToWorld);
         if (!HasCachedWorldNames) return;
         
         int value = _insertToWorld.intValue;

         int index = (byte) EditorGUILayout.Popup(value, _worldNamesArr);

         if (EndCheck)
            _insertToWorld.intValue = index;
      }

      private void DrawAddedComponents() {
         ulong[] hashes = Target.CompHashesEditorOnly;
         TypeManager.Initialize();

         bool isExpanded = Foldout(_componentHashes.isExpanded, Content("Added Components"));
         _componentHashes.isExpanded = isExpanded;

         if (!isExpanded) return;

         _addedComponents.Clear();

         for (int i = 0; i < hashes.Length; i++) {
            ulong hash = hashes[i];

            int typeIndex = TypeManager.GetTypeIndexFromStableTypeHash(hash);
            Type t = TypeManager.GetType(typeIndex);

            _addedComponents.Add(new ComponentDisplayData
                                 {
                                    Name = t.Name,
                                    Type = t
                                 });
         }

         _addedComponents.Sort((x1, x2) => string.Compare(x1.Name, x2.Name, StringComparison.Ordinal));
         using (Indent(1)) {
            int totalLength = _addedComponents.Count;

            for (int i = 0; i < totalLength; i += 2) {
               using (Horizontal) {
                  for (int j = i; j < i + 2 && j < _addedComponents.Count; j++) {
                     ComponentDisplayData comp = _addedComponents[j];

                     GUIContent content = Content(comp.Name);
                     if (EditorGUILayout.DropdownButton(content, FocusType.Passive)) {
                        ShowUsagesFor(comp);
                     }
                  }
               }
            }
         }
      }

      private void ShowUsagesFor(ComponentDisplayData data) {
         UsageBuffer.Clear();
         
         InsertToUsageBuffer(Target.Suppliers_EditorOnly, data);
         InsertToUsageBuffer(Target.ManagedSuppliers_EditorOnly, data);

         GenericMenu menu = new GenericMenu();
         menu.AddDisabledItem(Content("Used by:"), false);
         menu.AddSeparator(string.Empty);

         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < UsageBuffer.Count; i++) {
            MonoBehaviour behaviour = UsageBuffer[i];

            sb.Clear();
            sb.Append(behaviour.GetType().Name);
            sb.Append(" (");
            sb.Append(behaviour.gameObject.name);
            sb.Append(")");

            string root = sb.ToString();
            sb.Clear();
            sb.Append(root);
            sb.Append(PathSeparator);
            sb.Append(OpenScript);
            
            GUIContent content = Content(sb.ToString());
            menu.AddItem(content, false, () => FindScriptAndOpen(behaviour));

            sb.Clear();
            sb.Append(root);
            sb.Append(PathSeparator);
            sb.Append(SelectObject);
            
            content = Content(sb.ToString());
            menu.AddItem(content, false, () => SelectObj(behaviour));
         }

         menu.ShowAsContext();
      }
      
      private void InsertToUsageBuffer(List<MonoBehaviour> suppliers, ComponentDisplayData data) {
         foreach (MonoBehaviour monoSup in suppliers) {
            TypeBuffer.Clear();

            IEntitySupplier sup = monoSup as IEntitySupplier;
            Debug.Assert(sup != null);

            sup.GatherEntityTypes(TypeBuffer);

            if (TypeBuffer.Contains(data.Type)) {
               UsageBuffer.Add(monoSup);
            }
         }
      }

      private void FindScriptAndOpen(MonoBehaviour behaviour) {
         MonoScript script = MonoScript.FromMonoBehaviour(behaviour);
         AssetDatabase.OpenAsset(script, 0);
      }

      private void SelectObj(Object behaviour) {
         Selection.activeObject = behaviour;
      }
   }

   public struct ComponentDisplayData {
      public string Name;
      public Type Type;
   }

   [Serializable]
   public struct WorldNames {
      public List<string> Names;
   }
}