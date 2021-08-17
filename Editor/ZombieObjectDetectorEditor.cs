using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpZombieDetector
{

	[CustomEditor(typeof(ZombieObjectDetector))]
	public class ZombieObjectDetectorEditor : Editor
	{

		// Repeated code of a list of regexes.
		class RegexListEditor
		{
			private bool m_show;

			private string m_title;
			private string m_propName;
			private string[] m_defaults;

			private string m_doodle;
			private string m_testResult = "";

			public RegexListEditor(string propName, string title, IEnumerable<string> defaults)
			{
				m_propName = propName;
				m_title = title;
				m_defaults = defaults.ToArray();
			}

			public void Draw(SerializedObject serializedObject)
			{
				serializedObject.ApplyModifiedProperties(); // So that we can detect OUR OWN changes later on.
				SerializedProperty prop = serializedObject.FindProperty(m_propName);
				bool reset = false;
				using (new GUILayout.HorizontalScope())
				{
					m_show = EditorGUILayout.Foldout(m_show, m_title);
					reset |= GUILayout.Button("Reset", GUILayout.ExpandWidth(false));
				}

				// Reset logic.
				if (reset)
				{
					prop.arraySize = m_defaults.Length;
					for (int i = 0; i < m_defaults.Length; ++i)
						prop.GetArrayElementAtIndex(i).stringValue = m_defaults[i];
				}

				// Show values when folded-out.
				if (m_show)
				{
					using (new EditorGUI.IndentLevelScope())
					{
						EditorGUILayout.PropertyField(prop, new GUIContent("Values"));
						DrawTestField(prop);
					}
				}
				
				serializedObject.ApplyModifiedProperties();
			}

			private void DrawTestField(SerializedProperty prop)
			{
				var thin = GUILayout.ExpandWidth(false);
				var fat = GUILayout.ExpandWidth(true);
				using (new EditorGUILayout.HorizontalScope(fat))
				{
					string prev = m_doodle;
					m_doodle = EditorGUILayout.TextField("Test:", m_doodle, fat);

					bool change = (m_doodle != prev) || prop.serializedObject.hasModifiedProperties;
					if (change)
						RecalculateTestResult(prop);

					EditorGUILayout.LabelField(m_testResult, thin);
				}
			}

			private void RecalculateTestResult(SerializedProperty prop)
			{
				IEnumerable<Regex> regexes =
					Enumerable.Range(0, prop.arraySize)
					.Select(i => prop.GetArrayElementAtIndex(i).stringValue)
					.Select(s => new Regex(s));
				bool match = regexes.Any(r => r.IsMatch(m_doodle));
				m_testResult = match ? "Match" : "Not a match";
			}
		}




		private RegexListEditor m_ignoreAssemblies, m_ignoreTypes, m_ignoreTypesDuringSearch;

		private void Awake()
		{
			m_ignoreAssemblies = new RegexListEditor(
				"m_ignoreAssemblyPatterns",
				"Do not start from these Assemblies (Regex)",
				ZombieObjectDetector.DefaultAssemblyIgnorePatterns);

			m_ignoreTypes = new RegexListEditor(
				"m_ignoreTypePatterns",
				"Do not start from these Types (Regex)",
				ZombieObjectDetector.DefaultIgnoreTypePatterns);

			m_ignoreTypesDuringSearch = new RegexListEditor(
				"m_ignoreTypePatternsDuringSearch",
				"Do not recurse into these Types during search (Regex)",
				new string[] { }); // Default is empty string[]
		}




		public override void OnInspectorGUI()
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_runOnStart"));

			m_ignoreAssemblies.Draw(serializedObject);
			m_ignoreTypes.Draw(serializedObject);
			m_ignoreTypesDuringSearch.Draw(serializedObject);
			DrawDetectionButton();
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
