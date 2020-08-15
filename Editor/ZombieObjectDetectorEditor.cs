using UnityEditor;
using UnityEngine;

namespace CSharpZombieDetector
{

	[CustomEditor(typeof(ZombieObjectDetector))]
	public class ZombieObjectDetectorEditor : Editor
	{

		public override void OnInspectorGUI()
		{
			DrawIgnoreAssemblyList();
			DrawIgnoredTypeList();
			DrawDetectionButton();
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


		// Allows the user to trigger detection straight from the inspector.
		private void DrawDetectionButton()
		{
			var zod = target as ZombieObjectDetector;
			bool enabled = Application.isPlaying && !zod.IsLogging;
			using (new EditorGUI.DisabledGroupScope(!enabled))
			{
				if (Application.isPlaying)
				{
					if (GUILayout.Button("Log Zombies"))
					{
						zod.RunZombieObjectDetection();
					}
				}
				else
				{
					GUILayout.Button("Log Zombies (Available during Play)");
				}
			}
		}
	}



}
