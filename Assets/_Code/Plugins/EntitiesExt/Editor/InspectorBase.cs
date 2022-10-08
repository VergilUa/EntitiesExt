using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace EditorExt {
   /// <summary>
   /// Editor extension class that provides useful field drawers and more
   /// Inherits directly from Editor
   /// </summary>
   /// <remarks>
   /// Actually included in a separate package, but included here to simplify things a little.
   /// In case of EditorExt ever gets published, use that one instead.
   /// </remarks>
   public class InspectorBase : Editor {
      #region [Properties]

      public Type TargetType => _targetType ?? (_targetType = target.GetType());
      protected Type _targetType;

      protected static GUIStyle BoxDnd {
         get {
            if (_boxDnd == null) {
               _boxDnd = new GUIStyle(GUI.skin.box) {
                                                       alignment = TextAnchor.MiddleCenter,
                                                       fontStyle = FontStyle.Italic,
                                                       fontSize = 12
                                                    };
            }

            return _boxDnd;
         }
      }

      protected static GUIStyle _boxDnd;
      protected static GUIStyle _boldFoldoutStyle;

      #endregion

      protected virtual void OnEnable() { }
      
      public override void OnInspectorGUI() {
         serializedObject.Update();

         DrawAttributedInspector();

         serializedObject.ApplyModifiedProperties();
      }

      protected virtual bool ShouldDrawOnValidateBtn() {
         return TargetType.GetMethod("OnValidate",
                                     BindingFlags.NonPublic
                                     | BindingFlags.Public
                                     | BindingFlags.Instance
                                     | BindingFlags.Static)
                != null;
      }

      #region [Different inspectors]

      /// <summary>
      /// Draws default inspector that supports custom defined attributes, such as [FoldOutStart] and [FoldOutEnd]
      /// and more.
      /// </summary>
      protected void DrawAttributedInspector() {
         SerializedProperty prop = serializedObject.GetIterator();
         if (!prop.NextVisible(true)) return;

         // Draw script field disabled
         using (Horizontal) {
            using (Disabled(true)) {
               PropField(prop);
            }

            // Add a validate button even if its script field only
            if (ShouldDrawOnValidateBtn()) {
               DrawInvokeButton("Validate", "OnValidate", "Performs an OnValidate method call", false, true,
                                GUILayout.MaxWidth(60));
            }
            
            // First and last field
            if (!prop.NextVisible(true)) return;
         }

         do {
            DrawPropertyFinal(prop, true);
         } while (prop.NextVisible(false));
      }

      private void DrawPropertyFinal(SerializedProperty prop, bool drawLabel) {
         if (!drawLabel) {
            EditorGUILayout.ObjectField(prop, GUIContent.none);
         } else {
            EditorGUILayout.PropertyField(prop, true);
         }
      }

      #endregion

      #region [Layout management]

      /// <summary>
      /// Helper/Factory Method that provides instant access to construct
      /// using (HorizontalScope())
      /// {
      /// 
      /// }
      /// </summary>
      protected EditorGUILayout.HorizontalScope
         HorizontalLayout(GUIStyle style = null, params GUILayoutOption[] options) {
         return style != null
                   ? new EditorGUILayout.HorizontalScope(style, options)
                   : new EditorGUILayout.HorizontalScope(options);
      }

      /// <summary>
      /// Helper/Factory Method that provides instant access to construct
      /// using (VerticalScope())
      /// {
      /// 
      /// }
      /// </summary>
      protected EditorGUILayout.VerticalScope
         VerticalLayout(GUIStyle style = null, params GUILayoutOption[] options) {
         return style != null
                   ? new EditorGUILayout.VerticalScope(style, options)
                   : new EditorGUILayout.VerticalScope(options);
      }

      /// <summary>
      /// Helper/Factory Method that provides instant access to construct
      /// using (Horizontal)
      /// {
      /// 
      /// }
      /// </summary>
      protected EditorGUILayout.HorizontalScope Horizontal => HorizontalLayout();

      /// <summary>
      /// Helper/Factory Method that provides instant access to construct
      /// using (Vertical)
      /// {
      /// 
      /// }
      /// </summary>
      protected EditorGUILayout.VerticalScope Vertical => VerticalLayout();

      /// <summary>
      /// Helper/Factory Method that provides instant access to construct
      /// using (VerticalS(string style))
      /// {
      /// 
      /// }
      /// </summary>
      protected EditorGUILayout.VerticalScope VerticalS(string style) {
         return new EditorGUILayout.VerticalScope(style);
      }

      /// <summary>
      /// Helper/Factory Method that provides instant access to construct
      /// using (VerticalS(params GUILayoutOption[] options))
      /// {
      /// 
      /// }
      /// </summary>
      protected EditorGUILayout.VerticalScope VerticalS(params GUILayoutOption[] options) {
         return new EditorGUILayout.VerticalScope(options);
      }

      /// <summary>
      /// Helper/Factory Method that provides instant access to construct
      /// using (HorizontalS(string style))
      /// {
      /// 
      /// }
      /// </summary>
      protected EditorGUILayout.HorizontalScope HorizontalS(string style) {
         return new EditorGUILayout.HorizontalScope(style);
      }

      /// <summary>
      /// Helper/Factory Method that provides instant access to construct
      /// using (HorizontalS(string style))
      /// {
      /// 
      /// }
      /// </summary>
      protected EditorGUILayout.HorizontalScope HorizontalS(GUIStyle style) {
         return new EditorGUILayout.HorizontalScope(style);
      }

      /// <summary>
      /// Helper / Factory Method that provides instant access to construct
      /// using (ScrollView(GUIStyle))
      /// {
      ///
      /// }
      /// </summary>
      /// <returns></returns>
      protected EditorGUILayout.ScrollViewScope ScrollView(Vector2 scrollPos, params GUILayoutOption[] options) {
         return new EditorGUILayout.ScrollViewScope(scrollPos, options);
      }

      /// <summary>
      /// Helper/Factory Method that provides instant access to construct
      /// Equal to:
      /// 
      /// using (HorizontalS("Box"))
      /// {
      /// 
      /// }
      /// </summary>
      protected EditorGUILayout.HorizontalScope HorizontalBox => HorizontalS("Box");

      /// <summary>
      /// Helper/Factory Method that provides instant access to construct
      /// Equal to:
      /// 
      /// using (VerticalS("Box"))
      /// {
      /// 
      /// }
      /// </summary>
      protected EditorGUILayout.VerticalScope VerticalBox => VerticalS("Box");

      /// <summary>
      /// Helper/Factory Method that provides instant access to construct
      /// using (Disabled(bool disabled))
      /// {
      /// 
      /// }
      /// </summary>
      public static EditorGUI.DisabledScope Disabled(bool disabled) {
         return new EditorGUI.DisabledScope(disabled);
      }

      /// <summary>
      /// Performs "EditorGUI.IndentLevel = value" call via using shortcut
      /// using(Indent(int x)
      /// {
      /// }
      /// 
      /// Useful for resetting indent back to it's initial values without introducing any additional variables
      /// </summary>
      public static IndentScope Indent(int indent) {
         return new IndentScope(indent);
      }

      #endregion

      #region [Field drawers]

      /// <summary>
      /// Shortcut for EditorGUILayout.PropertyField
      /// </summary>
      /// <param name="prop"></param>
      /// <param name="guiContent"></param>
      /// <param name="includeChildren"></param>
      protected static void PropField(SerializedProperty prop, GUIContent guiContent, bool includeChildren = true) {
         EditorGUILayout.PropertyField(prop, guiContent, includeChildren);
      }

      /// <summary>
      /// Shortcut for EditorGUILayout.PropertyField.
      /// Finds property via serializedObject.FindProperty(string propertyName)
      /// 
      /// Doesn't attempts to draw it if there isn't one
      /// </summary>
      public void PropField(string propName, bool includeChildren = true) {
         SerializedProperty prop = FindProp(propName);

         if (prop == null) {
            Debug.LogWarning("Unable to find property of " + propName + " on object - " + serializedObject);
         }

         PropField(prop, includeChildren);
      }

      /// <summary>
      /// Shortcut for EditorGUILayout.PropertyField
      /// </summary>
      /// <param name="prop"></param>
      /// <param name="includeChildren"></param>
      protected static void PropField(SerializedProperty prop, bool includeChildren = true) {
         EditorGUILayout.PropertyField(prop, includeChildren);
      }

      /// <summary>
      /// Shortcut for EditorGUILayout.ObjectField
      /// </summary>
      /// <param name="content"></param>
      /// <param name="objectField"></param>
      /// <param name="allowSceneObjects"></param>
      /// <typeparam name="T"></typeparam>
      protected static void ObjectField<T>(GUIContent content, ref T objectField, bool allowSceneObjects = true)
         where T : Object {
         objectField = (T) EditorGUILayout.ObjectField(content, objectField, typeof(T), allowSceneObjects);
      }

      /// <summary>
      /// Shortcut for EditorGUILayout.ObjectField
      /// </summary>
      /// <param name="content"></param>
      /// <param name="objectField"></param>
      /// <param name="allowSceneObjects"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns></returns>
      protected static T ObjectField<T>(GUIContent content, T objectField, bool allowSceneObjects = true)
         where T : Object {
         return (T) EditorGUILayout.ObjectField(content, objectField, typeof(T), allowSceneObjects);
      }

      /// <summary>
      /// Shortcut for EditorGUILayout.ObjectField
      /// </summary>
      /// <param name="objectField"></param>
      /// <param name="allowSceneObjects"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns></returns>
      protected static T ObjectField<T>(T objectField, bool allowSceneObjects = true)
         where T : Object {
         return (T) EditorGUILayout.ObjectField(objectField, typeof(T), allowSceneObjects);
      }

      /// <summary>
      /// Draws float field for passed ref.
      /// Also record any changes done to target (default) or pased changedObject
      /// </summary>
      /// <param name="value"></param>
      /// <param name="changedObject"></param>
      protected void FloatField(ref float value, Object changedObject = null) {
         float newValue = EditorGUILayout.FloatField(value);

         // ReSharper disable once CompareOfFloatsByEqualityOperator
         if (newValue != value) {
            RecordObject(changedObject);
         }

         value = newValue;
      }

      /// <summary>
      /// Shortcut for FloatField
      /// </summary>
      protected float FloatField(float value, string label, params GUILayoutOption[] options) {
         return EditorGUILayout.FloatField(label, value, options);
      }

      /// <summary>
      /// Shortcut for FloatField
      /// </summary>
      /// <param name="value"></param>
      /// <param name="options"></param>
      public static float FloatField(float value, params GUILayoutOption[] options) {
         return EditorGUILayout.FloatField(value, options);
      }

      /// <summary>
      /// Shortcut for FloatField
      /// </summary>
      /// <param name="content"></param>
      /// <param name="value"></param>
      /// <param name="options"></param>
      protected float FloatField(GUIContent content, float value, params GUILayoutOption[] options) {
         return EditorGUILayout.FloatField(content, value, options);
      }

      /// <summary>
      /// Draws float field for passed ref.
      /// Also record any changes done to target (default) or pased changedObject
      /// </summary>
      /// <param name="content"></param>
      /// <param name="value"></param>
      /// <param name="changedObject"></param>
      protected void FloatField(GUIContent content, ref float value, Object changedObject = null) {
         float newValue = EditorGUILayout.FloatField(content, value);

         // ReSharper disable once CompareOfFloatsByEqualityOperator
         if (newValue != value) {
            RecordObject(changedObject);
         }

         value = newValue;
      }

      /// <summary>
      /// Draws float field for passed ref, clamping values
      /// Also records any changes done to target (default) or passed changedObject
      /// </summary>
      /// <param name="content"></param>
      /// <param name="value"></param>
      /// <param name="min"></param>
      /// <param name="max"></param>
      /// <param name="changedObject"></param>
      protected void FloatField(GUIContent content,
                                ref float value,
                                float min,
                                float max,
                                Object changedObject = null) {
         float newVal = Mathf.Clamp(EditorGUILayout.FloatField(content, value), min, max);

         // ReSharper disable once CompareOfFloatsByEqualityOperator
         if (newVal != value) {
            RecordObject(changedObject);
         }

         value = newVal;
      }

      /// <summary>
      /// Draws int field for passed ref
      /// Also records any changes done to target (default) or passed changedObject 
      /// </summary>
      /// <param name="content"></param>
      /// <param name="value"></param>
      /// <param name="changedObject"></param>
      protected void IntField(GUIContent content, ref int value, Object changedObject = null) {
         int newVal = EditorGUILayout.IntField(content, value);

         if (newVal != value) {
            RecordObject(changedObject);
         }

         value = newVal;
      }

      /// <summary>
      /// Draws int field for passed value
      /// Also records any changes done to target (default) or passed changedObject 
      /// </summary>
      protected int IntField(GUIContent content, int value, Object changedObject = null) {
         int newVal = EditorGUILayout.IntField(content, value);

         if (newVal != value) {
            RecordObject(changedObject);
         }

         return newVal;
      }

      /// <summary>
      /// Draws bool field for passed ref
      /// Also records any changes done to target (default) or passed changedObject 
      /// </summary>
      /// <param name="content"></param>
      /// <param name="value"></param>
      /// <param name="changedObject"></param>
      protected void BoolField(GUIContent content, ref bool value, Object changedObject = null) {
         bool newVal = EditorGUILayout.Toggle(content, value);

         if (newVal != value) {
            RecordObject(changedObject);
         }

         value = newVal;
      }

      /// <summary>
      /// Shortcut for Toggle
      /// </summary>
      protected bool BoolField(bool value, string label, params GUILayoutOption[] options) {
         return EditorGUILayout.Toggle(label, value, options);
      }
      
      /// <summary>
      /// Shortcut for Toggle
      /// </summary>
      protected bool BoolField(GUIContent content, bool value, params GUILayoutOption[] options) {
         return EditorGUILayout.Toggle(content, value, options);
      }

      /// <summary>
      /// Shortcut for Toggle
      /// </summary>
      /// <param name="value"></param>
      /// <param name="options"></param>
      protected bool BoolField(bool value, params GUILayoutOption[] options) {
         return EditorGUILayout.Toggle(value, options);
      }

      /// <summary>
      /// Draws toggle/bool field
      /// </summary>
      /// <param name="rect"></param>
      /// <param name="label"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      protected bool BoolField(Rect rect, string label, bool value) {
         return EditorGUI.Toggle(rect, label, value);
      }

      /// <summary>
      /// Draws toggle/bool field
      /// </summary>
      /// <param name="rect"></param>
      /// <param name="content"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      protected bool BoolField(Rect rect, GUIContent content, bool value) {
         return EditorGUI.Toggle(rect, content, value);
      }

      /// <summary>
      /// Draws EditorGUILayout.Slider with default min == 0 && max == 1f values
      /// Also records any changes done to target (default) or passed changedObject 
      /// </summary>
      protected float FloatSlider01(GUIContent content, float value, Object changedObject = null) {
         return FloatSlider(content, value, 0, 1f, changedObject);
      }

      /// <summary>
      /// Draws EditorGUILayout.Slider via shortcut
      /// Also records any changes done to target (default) or passed changedObject 
      /// </summary>
      protected float FloatSlider(GUIContent content,
                                  float value,
                                  float min,
                                  float max,
                                  Object changedObject = null) {
         float newValue = EditorGUILayout.Slider(content, value, min, max);
         // ReSharper disable once CompareOfFloatsByEqualityOperator
         if (value != newValue) {
            RecordObject(changedObject);
         }

         return newValue;
      }

      /// <summary>
      /// Draws EditorGUILayout.Slider and auto-assigns value to the same passed ref
      /// Also records any changes done to target (default) or passed changedObject 
      /// </summary>
      protected void FloatSlider(GUIContent content,
                                 ref float value,
                                 float min,
                                 float max,
                                 Object changedObject = null) {
         float newValue = EditorGUILayout.Slider(content, value, min, max);

         // ReSharper disable once CompareOfFloatsByEqualityOperator
         if (newValue != value) {
            RecordObject(changedObject);
         }

         value = newValue;
      }

      /// <summary>
      /// Draws EditorGUILayout.Slider and auto-assigns value to the same passed ref
      /// Also auto-sets min to 0 and max to 1f
      /// Also records any changes done to target (default) or passed changedObject 
      /// </summary>
      protected void FloatSlider01(GUIContent content, ref float value, Object changedObject = null) {
         FloatSlider(content, ref value, 0, 1f, changedObject);
      }

      /// <summary>
      /// Field drawer for default enums
      /// This one also records changes done, so make sure to apply retrieved value
      /// </summary>
      protected void EnumField<T>(GUIContent content, ref T e, Object changedObject = null)
         where T : struct, IConvertible {
         T newVal = (T) (object) EditorGUILayout.EnumPopup(content, e as Enum); // WTF??? 

         if (!e.Equals(newVal)) {
            RecordObject(changedObject);
         }

         e = newVal;
      }

      /// <summary>
      /// Field drawer for AnimationCurve
      /// </summary>
      protected void CurveField(GUIContent content, ref AnimationCurve curve) {
         curve = EditorGUILayout.CurveField(content, curve);
      }

      /// <summary>
      /// Field drawer for AnimationCurve
      /// </summary>
      protected AnimationCurve CurveField(GUIContent label,
                                          AnimationCurve curve,
                                          Color color,
                                          Rect ranges,
                                          params GUILayoutOption[] options) {
         return EditorGUILayout.CurveField(label, curve, color, ranges, options);
      }

      /// <summary>
      /// Records changes done to passed object, or target is it's null
      /// </summary>
      /// <param name="recordObject"></param>
      private void RecordObject(Object recordObject) {
         Object recObj = recordObject;
         if (recObj == null) {
            recObj = target;
         }

         Undo.RecordObject(recObj, "Changed " + recObj);
      }

      /// <summary>
      /// Label drawer that supports textures
      /// </summary>
      /// <param name="label"></param>
      /// <param name="texture"></param>
      private void Label(string label, Texture2D texture = null) {
         GUILayout.Space(3);

         using (Horizontal) {
            if (texture != null) {
               Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(16), GUILayout.Height(16));
               rect = EditorGUI.IndentedRect(rect);
               rect.width = 16;

               GUI.DrawTexture(rect, texture);
            }

            EditorGUILayout.LabelField(Content(label));
         }

         GUILayout.Space(3);
      }

      /// <summary>
      /// Simple double LabelField drawer
      /// </summary>
      /// <param name="label"></param>
      /// <param name="label2"></param>
      protected void Label(string label, string label2) {
         GUILayout.Space(3);

         EditorGUILayout.LabelField(Content(label), Content(label2));

         GUILayout.Space(3);
      }

      /// <summary>
      /// Label drawer that support styles
      /// </summary>
      protected void Label(string label,
                           FontStyle fontStyle = FontStyle.Normal,
                           Texture2D texture = null) {
         // Apply style
         FontStyle initialStyle = EditorStyles.label.fontStyle;
         EditorStyles.label.fontStyle = fontStyle;

         Label(label, texture);

         EditorStyles.label.fontStyle = initialStyle;
      }

      /// <summary>
      /// Label drawer shortcut
      /// </summary>
      protected void Label(string label, FontStyle style, params GUILayoutOption[] options) {
         FontStyle initialStyle = EditorStyles.label.fontStyle;
         EditorStyles.label.fontStyle = style;

         GUILayout.Label(label, options);

         EditorStyles.label.fontStyle = initialStyle;
      }

      /// <summary>
      /// Label drawer shortcut
      /// </summary>
      /// <param name="content"></param>
      protected void Label(GUIContent content) {
         EditorGUILayout.LabelField(content);
      }

      /// <summary>
      /// Label drawer shortcut
      /// </summary>
      protected void Label(string label, params GUILayoutOption[] options) {
         GUILayout.Label(label, options);
      }

      /// <summary>
      /// Label drawer shortcut
      /// </summary>
      protected void Label(string label, GUIStyle style, params GUILayoutOption[] options) {
         GUILayout.Label(label, style, options);
      }

      /// <summary>
      /// Label drawer shortcut
      /// </summary>
      protected void Label(GUIContent content, GUIStyle style, params GUILayoutOption[] options) {
         GUILayout.Label(content, style, options);
      }

      /// <summary>
      /// Draws (I)ndented Label at indentLevel
      /// </summary>
      // ReSharper disable once InconsistentNaming
      protected void ILabel(string label, int indentLevel, Texture2D texture = null) {
         using (Indent(indentLevel)) {
            Label(label, texture);
         }
      }

      /// <summary>
      /// Draws (I)ndented Label at indentLevel with specific style
      /// </summary>
      // ReSharper disable once InconsistentNaming
      protected void ILabel(string label, FontStyle fontStyle, int indentLevel,
                            Texture2D texture = null) {
         // Apply style
         FontStyle initialStyle = EditorStyles.label.fontStyle;
         EditorStyles.label.fontStyle = fontStyle;

         ILabel(label, indentLevel, texture);

         EditorStyles.label.fontStyle = initialStyle;
      }

      #endregion

      #region [Shortcuts]

      protected void HelpBox(string text) {
         EditorGUILayout.HelpBox(text, MessageType.Info);
      }

      protected void ErrorBox(string text) {
         EditorGUILayout.HelpBox(text, MessageType.Error);
      }

      /// <summary>
      /// Shortcut for V3Field
      /// </summary>
      protected Vector3 V3Field(Vector3 value, string label = "", params GUILayoutOption[] options) {
         return EditorGUILayout.Vector3Field(label, value, options);
      }

      /// <summary>
      /// Performs EditorGUI.BeginChangeCheck()
      /// </summary>
      public static void ChangeCheck() {
         EditorGUI.BeginChangeCheck();
      }

      /// <summary>
      /// Shortcut for EditorGUILayout.TextField
      /// </summary>
      protected string TextField(string label, string text) {
         return EditorGUILayout.TextField(label, text);
      }

      /// <summary>
      /// Shortcut for EditorGUILayout.Foldout
      /// </summary>
      /// <param name="value"></param>
      /// <param name="content"></param>
      /// <returns></returns>
      protected bool Foldout(bool value, GUIContent content) {
         return EditorGUILayout.Foldout(value, content, true);
      }

      /// <summary>
      /// Shortcut for EditorGUILayout.Foldout
      /// </summary>
      protected bool Foldout(bool value, string content, GUIStyle style = null) {
         if (style == null) {
            if (_boldFoldoutStyle == null) _boldFoldoutStyle = new GUIStyle(EditorStyles.foldout);
            _boldFoldoutStyle.fontStyle = FontStyle.Bold;
         }

         return EditorGUILayout.Foldout(value, content, true, style ?? _boldFoldoutStyle);
      }

      /// <summary>
      /// Shortcut for EditorGUILayout.GetControlRect()
      /// </summary>
      /// <returns></returns>
      protected Rect GetControlRect() {
         return EditorGUILayout.GetControlRect();
      }

      /// <summary>
      /// Shortcut to GUILayout.Button
      /// </summary>
      /// <param name="label"></param>
      /// <returns></returns>
      protected bool Button(string label) {
         return GUILayout.Button(label);
      }

      /// <summary>
      /// Shortcut to GUILayout.Button
      /// </summary>
      /// <returns></returns>
      protected bool Button(GUIContent content, string style, GUILayoutOption layoutOptions) {
         return GUILayout.Button(content, style, layoutOptions);
      }

      /// <summary>
      /// Shortcut to GUILayout.Button
      /// </summary>
      /// <returns></returns>
      protected bool Button(GUIContent content, params GUILayoutOption[] layoutOptions) {
         return GUILayout.Button(content, layoutOptions);
      }

      /// <summary>
      /// Shortcut to GUILayout.Button
      /// </summary>
      /// <param name="style"></param>
      /// <returns></returns>
      protected bool Button(GUIContent style) {
         return GUILayout.Button(style);
      }

      /// <summary>
      /// Draws a validation button
      /// </summary>
      protected void DrawValidationButton(string buttonLabel = "Validate",
                                          string tooltip = "",
                                          bool halfButton = true) {
         if (halfButton) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width / 2f - 15f);
         }

         if (GUILayout.Button(
                              new GUIContent(
                                             buttonLabel,
                                             tooltip))) {
            // Access validation method
            Object obj = serializedObject.targetObject;
            MethodInfo dynMethod = obj.GetType()
                                      .GetMethod(
                                                 "OnValidate",
                                                 BindingFlags.NonPublic | BindingFlags.Instance);

            if (dynMethod == null) {
               Debug.LogError("Script " + obj.name + " doesn't contain OnValidate method", obj);
            } else {
               dynMethod.Invoke(obj, new object[0]);
            }
         }

         if (halfButton) {
            EditorGUILayout.EndHorizontal();
         }
      }

      /// <summary>
      /// Draws button of half size of Screen.width that Invokes method with passed methodName
      /// </summary>
      protected bool DrawInvokeButton(string buttonLabel,
                                      string methodName,
                                      string tooltip = "",
                                      bool halfButton = true,
                                      bool recordChanges = false,
                                      params GUILayoutOption[] options) {
         if (halfButton) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width / 2f - 15f);
         }

         bool value = false;
         if (Button(Content(buttonLabel, tooltip), options)) {
            InvokeMethod(methodName, recordChanges);

            value = true;
         }

         if (halfButton) {
            EditorGUILayout.EndHorizontal();
         }

         return value;
      }

      private void InvokeMethod(string methodName, bool recordChanges) {
         Object[] gos = serializedObject.targetObjects;

         foreach (Object obj in gos) {
            // Access validation method
            MethodInfo dynMethod = obj.GetType().GetMethod(methodName,
                                                           BindingFlags.Public | BindingFlags.NonPublic |
                                                           BindingFlags.Instance);

            if (dynMethod == null) {
               Debug.LogError($"Script {obj.name} doesn't contain {methodName} method", obj);
            } else {
               if (recordChanges) Undo.RecordObject(obj, $"Invoking {methodName}");

               dynMethod.Invoke(obj, new object[0]);
            }
         }
      }

      /// <summary>
      /// Creates an instance of GUIContent with passed data
      /// </summary>
      /// <param name="label"></param>
      /// <param name="tooltip"></param>
      public static GUIContent Content(string label = "", string tooltip = "") {
         return new GUIContent(label, tooltip);
      }

      /// <summary>
      /// Shortcut to EditorGUILayout.Space()
      /// </summary>
      public static void Space() {
         EditorGUILayout.Space();
      }

      /// <summary>
      /// Shortcut for GUILayout.Width
      /// </summary>
      public static GUILayoutOption Width(float width) {
         return GUILayout.Width(width);
      }

      /// <summary>
      /// Shortcut for GUILayout.FlexibleSpace()
      /// </summary>
      public static void FlexibleSpace() {
         GUILayout.FlexibleSpace();
      }

      /// <summary>
      /// Shortcut for GUILayout.Space()
      /// </summary>
      /// <param name="spacer"></param>
      public static void Space(int spacer) {
         GUILayout.Space(spacer);
      }

      /// <summary>
      /// Shortcut for serializedObject.FindProperty(string propertyName)
      /// </summary>
      public SerializedProperty FindProp(string propertyName, bool testNulls = true) {
         SerializedProperty prop = serializedObject.FindProperty(propertyName);
         if (testNulls && prop == null) {
            Debug.LogError("Editor:: Cannot find property " + propertyName + " please adjust the property name");
         }

         return prop;
      }

      #endregion

      #region [Utilities]

      protected void DragNDrop(List<Object> buffer, string label) {
         GUI.skin.box = BoxDnd;

         Rect parentRect = new Rect();
         parentRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));

         if (!parentRect.Contains(Event.current.mousePosition)) {
            GUI.Box(parentRect, label, BoxDnd);
            return;
         }

         GUI.Box(parentRect, $"Total:: {DragAndDrop.objectReferences.Length}", BoxDnd);

         if (Event.current.type == EventType.DragUpdated) {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            Event.current.Use();
         } else if (Event.current.type == EventType.DragPerform) {
            var objRefs = DragAndDrop.objectReferences;
            buffer.AddRange(objRefs);

            Event.current.Use();
         }
      }

      public static float HandleSize(Vector3 position) {
         float s = HandleUtility.GetHandleSize(position) * 0.1f;
         return Mathf.Lerp(s, 0.025f, 0.2f);
      }

      /// <summary>
      /// Returns width for the string
      /// </summary>
      public static int PxSize(string str, int extraPx) {
         return str.Length * extraPx;
      }

      /// <summary>
      /// Sets the width to the length of the string
      /// </summary>
      public static GUILayoutOption PxWidth(string str, int extraPx = 10) {
         return Width(PxSize(str, extraPx));
      }

      /// <summary>
      /// Does the same thing as Header attribute
      /// </summary>
      /// <param name="text"></param>
      public static void Header(string text) {
         Space(5);
         EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
      }

      #endregion

      #region [Editor Prefs]

      /// <summary>
      /// Gets or sets default value for the Editor pref by key
      /// </summary>
      public static float GetSetPref(string key, float defaultValue) {
         if (EditorPrefs.HasKey(key)) {
            return EditorPrefs.GetFloat(key);
         }

         EditorPrefs.SetFloat(key, defaultValue);
         return defaultValue;
      }

      public static bool GetSetPref(string key, bool defaultValue) {
         if (EditorPrefs.HasKey(key)) {
            return EditorPrefs.GetBool(key);
         }

         EditorPrefs.SetBool(key, defaultValue);
         return defaultValue;
      }

      /// <summary>
      /// Sets value for the EditorPref float
      /// </summary>
      /// <param name="key"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      public static void SetPref(string key, float value) {
         EditorPrefs.SetFloat(key, value);
      }

      /// <summary>
      /// Sets value for the EditorPref bool
      /// </summary>
      /// <param name="key"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      public static void SetPref(string key, bool value) {
         EditorPrefs.SetBool(key, value);
      }

      #endregion
   }

   /// <summary>
   /// Editor extension class that provides useful field drawers and more
   /// Inherits directly from Editor
   ///
   /// This one is used to override MonoBehaviour's default inspector without messing with Object inheritance
   /// </summary>
   [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
   [CanEditMultipleObjects]
   public class InspectorMono : InspectorBase {
   }

   /// <summary>
   /// Editor extension class that provides useful field drawers and more
   /// Inherits directly from Editor
   ///
   /// This one is used to override ScriptableObject's default inspector without messing with Object inheritance
   /// </summary>
   [CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
   [CanEditMultipleObjects]
   public class InspectorScriptable : InspectorBase {
   }

   /// <summary>
   /// Indent scope class for
   /// using(Indent(int x)
   /// {
   /// }
   /// 
   /// Useful for resetting indent back to it's initial values without introducing any additional variables
   /// </summary>
   public class IndentScope : IDisposable {
      private bool _isDisposed;
      private readonly int _initialIndent;

      public IndentScope(int newIndent) {
         _initialIndent = EditorGUI.indentLevel;
         EditorGUI.indentLevel = newIndent;
      }

      public void Dispose() {
         if (_isDisposed) return;

         _isDisposed = true;

         // Had to hack this, since guiIsExiting is internal
         Type type = typeof(GUIUtility);
         PropertyInfo info = type.GetProperty("guiIsExiting", BindingFlags.NonPublic | BindingFlags.Static);

         System.Diagnostics.Debug.Assert(info != null, "info != null");
         bool isExiting = (bool) info.GetValue(null, null);

         if (isExiting) {
            return;
         }

         CloseScope();
      }

      private void CloseScope() {
         EditorGUI.indentLevel = _initialIndent;
      }
   }

   public static class InspectorBaseUtils {
      public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property) {
         property = property.Copy();
         var nextElement = property.Copy();
         bool hasNextElement = nextElement.NextVisible(false);
         if (!hasNextElement) {
            nextElement = null;
         }

         property.NextVisible(true);
         while (true) {
            if ((SerializedProperty.EqualContents(property, nextElement))) {
               yield break;
            }

            yield return property;

            bool hasNext = property.NextVisible(false);
            if (!hasNext) {
               break;
            }
         }
      }
   }
}