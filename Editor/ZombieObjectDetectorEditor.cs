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
		}

		public override void OnInspectorGUI()
		{
			m_SerializedZombieDetector.Update();

			DrawIgnoreAssemblyList();
			DrawIgnoredTypeList();


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


		private bool m_showAssemblyIgnores = false;

		private void DrawIgnoreAssemblyList()
		{
			DrawRegexList(
				"m_ignoreAssemblyPatterns",
				"Assembly Ignore Patterns",
				ref m_showAssemblyIgnores,
				ZombieObjectDetector.DefaultAssemblyIgnorePatterns);
		}


		private bool m_showTypeIgnores = false;

		private void DrawIgnoredTypeList()
		{
			DrawRegexList(
				"m_ignoreTypePatterns",
				"Type Ignore Patterns",
				ref m_showTypeIgnores,
				ZombieObjectDetector.DefaultIgnoreTypePatterns);
		}


		private void DrawRegexList(
			string propName,
			string name,
			ref bool show,
			string[] defaults)
		{

			SerializedProperty prop = serializedObject.FindProperty(propName);
			bool reset = false;
			using (new GUILayout.HorizontalScope())
			{
				show = EditorGUILayout.Foldout(show, name);
				if (!show)
					reset |= GUILayout.Button("Reset", GUILayout.ExpandWidth(false));
			}
			if (show)
			{
				using (new EditorGUI.IndentLevelScope())
				{
					EditorGUILayout.PropertyField(prop, new GUIContent("Values"));
					reset |= GUILayout.Button("Reset to Defaults");
					if (reset)
					{
						prop.arraySize = defaults.Length;
						for (int i = 0; i < defaults.Length; ++i)
							prop.GetArrayElementAtIndex(i).stringValue = defaults[i];
					}
				}
				serializedObject.ApplyModifiedProperties();
			}
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
