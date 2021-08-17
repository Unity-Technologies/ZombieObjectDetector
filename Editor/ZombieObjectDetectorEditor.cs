using UnityEditor;
using UnityEngine;

namespace CSharpZombieDetector
{

	[CustomEditor(typeof(ZombieObjectDetector))]
	public class ZombieObjectDetectorEditor : Editor
	{

		public override void OnInspectorGUI()
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_runOnStart"));
			DrawIgnoreAssemblyList();
			DrawIgnoredTypeList();
			DrawIgnoredTypesDuringSearch();
			DrawDetectionButton();
			serializedObject.ApplyModifiedProperties();
		}



		private bool m_showAssemblyIgnores = false;

		private void DrawIgnoreAssemblyList()
		{
			DrawRegexList(
				"m_ignoreAssemblyPatterns",
				"Do not start from these Assemblies (Regex)",
				ref m_showAssemblyIgnores,
				ZombieObjectDetector.DefaultAssemblyIgnorePatterns);
		}


		private bool m_showTypeIgnores = false;

		private void DrawIgnoredTypeList()
		{
			DrawRegexList(
				"m_ignoreTypePatterns",
				"Do not start from these Types (Regex)",
				ref m_showTypeIgnores,
				ZombieObjectDetector.DefaultIgnoreTypePatterns);
		}


		private bool m_showSearchTypeIgnores = false;

		private void DrawIgnoredTypesDuringSearch()
		{
			DrawRegexList(
				"m_ignoreTypePatternsDuringSearch",
				"Do not recurse into these Types during search (Regex)",
				ref m_showSearchTypeIgnores,
				new string[] { }); // Default is empty string[]
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
				reset |= GUILayout.Button("Reset", GUILayout.ExpandWidth(false));
			}
			if (show)
			{
				using (new EditorGUI.IndentLevelScope())
				{
					EditorGUILayout.PropertyField(prop, new GUIContent("Values"));
				}
			}
			if (reset)
			{
				prop.arraySize = defaults.Length;
				for (int i = 0; i < defaults.Length; ++i)
					prop.GetArrayElementAtIndex(i).stringValue = defaults[i];
			}
		}


		// Allows the user to trigger detection straight from the inspector.
		private void DrawDetectionButton()
		{
			var zod = target as ZombieObjectDetector;
			bool enabled = Application.isPlaying;
			using (new EditorGUI.DisabledGroupScope(!enabled))
			{
				if (enabled)
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
