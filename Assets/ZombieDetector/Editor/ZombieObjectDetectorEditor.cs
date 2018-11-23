using UnityEditor;
using UnityEngine;

namespace CSharpZombieDetector
{

    [CustomEditor(typeof(ZombieObjectDetector))]
    public class ZombieObjectDetectorEditor : Editor
    {

        private SerializedObject m_SerializedZombieDetector;

        private SerializedProperty m_LoggingOptions;
        
        private SerializedProperty m_IgnoredTypeStrings;

        private SerializedProperty m_TypesToScanStrings;

        private SerializedProperty m_LogZombieKeyCode;

        private SerializedProperty m_LogTag;

        private ZombieObjectDetector m_ZombieDetector;

        private string m_NameOfTypeToScanToAdd = "";

        private bool m_HasFailedToAddTypeToScan = false;

        private string m_NameOfIgnoredTypeToAdd = "";

        private bool m_HasFailedToAddIgnoredType = false;

        private bool m_ShowLoggingOptionDescriptions = false;

        private void OnEnable()
        {

            m_SerializedZombieDetector = new SerializedObject(target);

            m_ZombieDetector = (ZombieObjectDetector)(target);

            m_LoggingOptions = m_SerializedZombieDetector.FindProperty("m_LoggingOptions");
            
            m_LogTag = m_SerializedZombieDetector.FindProperty("m_LogTag");

            m_LogZombieKeyCode = m_SerializedZombieDetector.FindProperty("m_LogZombieKeyCode");

            m_IgnoredTypeStrings = m_SerializedZombieDetector.FindProperty("m_IgnoredTypeStrings");

            m_TypesToScanStrings = m_SerializedZombieDetector.FindProperty("m_TypesToScanStrings");
        }

        public override void OnInspectorGUI()
        {
            m_SerializedZombieDetector.Update();

            m_LoggingOptions.intValue = (int)(ZombieObjectDetector.LoggingOptions)EditorGUILayout.EnumMaskField(
                new GUIContent("Logging Options", "Allows different depths of logging."),
                (ZombieObjectDetector.LoggingOptions)m_LoggingOptions.intValue);

            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUI.indentLevel++;
            m_ShowLoggingOptionDescriptions = EditorGUILayout.Foldout(m_ShowLoggingOptionDescriptions, "Logging Option Descriptions");
            EditorGUI.indentLevel--;

            if (m_ShowLoggingOptionDescriptions)
            {
                DrawOptionDescription("InvalidType", "Logs information about types that are ignored because they can't cause, or be used to detect, zombie objects (int,float etc)");

                DrawOptionDescription("ListBadEqualsImplementations", "Lists all Bad .Equals Implementations that were found causing the object and all its members to be ignored.");

                DrawOptionDescription("ListScannedObjects", "Lists all objects that have had there members checked for zombie objects.");

                DrawOptionDescription("ListScannedStaticMembers", "List of all Static Members that have been checked from there roots.");

                DrawOptionDescription("ZombieCountForEachStaticField", "Individual Zombie Object count for Static field roots.");

                DrawOptionDescription("ZombieStackTrace", "Zombie object stack trace, used to find root to object causeing leaks.");

                DrawOptionDescription("Default Options", "ZombieStackTrace\nZombieCountForEachStaticField.");
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(m_LogTag);

            DisplayTypeList("Types to scan. Empty to scan all types.", m_TypesToScanStrings, ref m_NameOfTypeToScanToAdd, ref m_HasFailedToAddTypeToScan);

            DisplayTypeList("Types to ignore.", m_IgnoredTypeStrings, ref m_NameOfIgnoredTypeToAdd, ref m_HasFailedToAddIgnoredType);

            EditorGUILayout.PropertyField(m_LogZombieKeyCode, new GUIContent("Zombie Logging Key Code", "Used for logging in builds"));

            EditorGUI.BeginDisabledGroup(!Application.isPlaying || m_ZombieDetector.IsLogging());
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Log Zombies"))
                {
                    m_ZombieDetector.RunZombieObjectDetection();
                }
            }
            else
            {
                GUILayout.Button("Log Zombies (Available during Play)");
            }

            EditorGUI.EndDisabledGroup();
            
            m_SerializedZombieDetector.ApplyModifiedProperties();
        }

        private void DisplayTypeList(string label, SerializedProperty property, ref string inputText, ref bool hasFailedToAdd)
        {
            if (hasFailedToAdd && inputText == "")
            {
                hasFailedToAdd = false;
            }
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);

            if (hasFailedToAdd)
            {
                Color oldColor = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.LabelField("Type Not Found: " + inputText);
                GUI.color = oldColor;
            }
            EditorGUILayout.EndHorizontal();

            // Box for adding Ignored Types.
            EditorGUILayout.BeginHorizontal("Box");
            inputText = EditorGUILayout.TextField(inputText);
            if (GUILayout.Button("+", GUILayout.Width(40.0f)))
            {
                // attempt add type.
                GUI.FocusControl(null);
                if (TypeHelper.IsType(inputText))
                {
                    AddType(inputText, property);
                    inputText = "";
                    hasFailedToAdd = false;
                }
                else
                {
                    hasFailedToAdd = true;
                }

            }

            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < property.arraySize; i++)
            {

                EditorGUILayout.BeginHorizontal("Box");

                EditorGUILayout.LabelField(GetType(i, property));
                if (GUILayout.Button("-", GUILayout.Width(40.0f)))
                {
                    RemoveType(i, property);
                    GUI.FocusControl(null);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
        private void AddType(string typeName, SerializedProperty property)
        {
            property.InsertArrayElementAtIndex(property.arraySize);
            property.GetArrayElementAtIndex(property.arraySize - 1).stringValue = typeName;
        }

        private void RemoveType(int index, SerializedProperty property)
        {
            for (int i = index; i < (property.arraySize - 1); i++)
            {
                SetType(i, GetType(i + 1, property), property);
            }
            property.arraySize--;
        }

        private void SetType(int index, string typeName, SerializedProperty property)
        {
            property.GetArrayElementAtIndex(index).stringValue = typeName;
        }

        private string GetType(int index, SerializedProperty property)
        {
            return property.GetArrayElementAtIndex(index).stringValue;
        }

        [MenuItem("GameObject/Create Zombie Object Detector", priority = 0)]
        public static void CreateZombieDetector()
        {
            ZombieObjectDetector m_ZombieDetector = FindObjectOfType<ZombieObjectDetector>();
            if (m_ZombieDetector == null)
            {
                Debug.Log("Creating A Default Zombie Object Detector");
                new GameObject("ZombieObjectDetector", typeof(ZombieObjectDetector));
            }
            else
            {
                Debug.LogWarning("Zombie Object Detector Already Exists.");
            }
        }

        private void DrawOptionDescription(string name, string description)
        {

            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField(name, GUILayout.MinWidth(180), GUILayout.MaxWidth(180));
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndHorizontal();
        }
    }



}
