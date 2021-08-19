using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CSharpZombieDetector
{

	public class ZombieObjectDetector : MonoBehaviour
	{

		[SerializeField]
		private bool m_runOnStart = false;

		// Types within Assemblies whose name matches any of the following patterns will not be used as starting points.
		[SerializeField]
		private string[] m_ignoreAssemblyPatterns = DefaultAssemblyIgnorePatterns;

		public static readonly string[] DefaultAssemblyIgnorePatterns = {
			"^Unity\\.", "^UnityEngine", "^UnityEditor", "^mscorlib$", "^System", "^Mono\\."
		};



		// Types whose name matches any of these regex patterns will not be used as starting points.
		[SerializeField]
		private string[] m_ignoreTypePatterns = DefaultIgnoreTypePatterns;

		public static readonly string[] DefaultIgnoreTypePatterns =
		{
			"^System\\.", "^Unity\\.", "^UnityEngine\\.", "^UnityEditor\\.", "^Mono\\."
		};


		// Types whose name matches any of these regex patterns will be skipped from the recursion.
		[SerializeField]
		private string[] m_ignoreTypePatternsDuringSearch;

		// An exception will be thrown if we recurse deeper than this.
		// (Avoids crashing the editor/player)
		[SerializeField]
		private int m_maxDepth = 200;





		// An individual invocation of a search.
		public class SearchContext
		{

			public struct TestInfo
			{
				public object obj;
				public Type type;
				public FieldInfo fieldInfo;
			};
			public event System.Action<TestInfo> TestingObjectField;

			public event System.Action<System.Exception> CaughtExceptionAndContinued;

			public event System.Action<object> ZombieHit;
			public event System.Action SearchCompleted;

			/// <summary>
			///  Fired every 1024 objects.
			///  Allows for progress reporting, e.g. to prove the process has not hung.
			/// </summary>
			public event System.Action MadeProgress;


			private HashSet<object> m_scannedObjects = new HashSet<object>();
			private Stack<FieldInfo> m_fieldInfos = new Stack<FieldInfo>();

			/// <summary>
			/// Note that the order is "backwards"; the most recent field is first,
			/// and the static field where the search started is last.
			/// </summary>
			public IEnumerable<FieldInfo> FieldInfoChain => m_fieldInfos;

			private int m_maxDepth;

			public uint NumTestsPerformed { get; private set; }

			private IEnumerable<Regex> m_ignoreTypes;

			public SearchContext (int maxDepth, IEnumerable<string> ignoreTypeNameRegexStrings)
			{
				m_maxDepth = maxDepth;
				if (ignoreTypeNameRegexStrings != null && ignoreTypeNameRegexStrings.Any())
					m_ignoreTypes = ignoreTypeNameRegexStrings.Select(s => new Regex(s)).ToList();
			}

			public static bool IsValidZombieType(Type type)
			{
				if (!TypeHelper.IsPotentialZombieType(type))
					return false;

				if (type.IsPointer)
					return false;

				if (type == typeof(System.Reflection.Pointer))
					return false; // Otherwise FieldInfo.GetValue recurses infinitely.

				if (type.ContainsGenericParameters || type.IsGenericParameter || type.IsGenericTypeDefinition)
					return false; // (Why?)

				return true;
			}



			private void CheckObjectField(object o, Type oType, FieldInfo fieldInfo)
			{
				if (m_fieldInfos.Count > m_maxDepth)
					throw new System.OverflowException("Max depth exceeded.");

				string typeName = oType.FullName;
				if (m_ignoreTypes != null && m_ignoreTypes.Any(r => r.IsMatch(typeName)))
					return;

				TestingObjectField?.Invoke(new TestInfo { obj = o, type = oType, fieldInfo = fieldInfo });

				m_fieldInfos.Push(fieldInfo);
				try
				{
					object val = null;
					try { val = fieldInfo.GetValue(o); }
					catch (System.Exception) { } // val stays null.  See next line.
					if (val == null)
						return; // (Still runs the "finally" below)
					TestAndRecurse(val);

					// Special cases:

					// If the value is an array then iterate through the values.
					if (val.GetType().IsArray)
					{
						IEnumerable collection = val as IEnumerable;
						foreach (object arrayEntry in collection)
							TestAndRecurse(arrayEntry);
					}

					// If the value is the backing field of an event, then iterate through the listeners.
					if (val is MulticastDelegate)
					{
						var mcd = val as MulticastDelegate;
						Delegate[] children = mcd.GetInvocationList();
						foreach (Delegate d in children)
							TestAndRecurse(d.Target);
					}
				}
				finally
				{
					FieldInfo x = m_fieldInfos.Pop();
					Debug.Assert(x == fieldInfo); // Otherwise we have a Push/Pop mismatch.
				}
			}


			private void TestAndRecurse(object obj)
			{
				if (obj == null)
					return;

				System.Type objType = obj.GetType();
				// Note that we're thinking about the *runtime* type of this object,
				// rather than the compile-time type of whatever field it was stored under.

				if (!IsValidZombieType(objType))
					return;

				bool testedAlready = true;
				try { testedAlready = m_scannedObjects.Contains(obj); }
				catch (System.Exception x) { } // testedAlready remains true.  See subsequent line.
				if (testedAlready)
					return;

				m_scannedObjects.Add(obj);

				TestObject(obj);

				// Recurse into this object's instance fields.
				BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
				FieldInfo[] fields = objType.GetFields(flags);
				foreach (FieldInfo field in fields)
					CheckObjectField(obj, objType, field);
			}

			/// <summary>
			/// Uses Unity Fake Null
			/// https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
			/// https://forum.unity.com/threads/fun-with-null.148090/
			/// </summary>
			private void TestObject(object obj)
			{
				// An object is a zombie if:
				//
				// * It is not actually null.
				// * It is a subclass of UnityEngine.Object
				// * It compares itself as equal to null.
				//
				bool exists = obj != null; // Comparing a System.object. "Fake null" doesn't get a chance to run here.
				bool isUnityObj = obj is UnityEngine.Object;
				UnityEngine.Object unityObj = obj as UnityEngine.Object;
				bool zombie = exists && isUnityObj && (unityObj == null);

				++NumTestsPerformed;
				// Emit an event every 1024 tests.
				bool madeProgress = (NumTestsPerformed & 0x3ff) == 0;
				if (madeProgress)
					MadeProgress?.Invoke();

				if (zombie)
					ZombieHit?.Invoke(obj);
			}


			private void CheckStaticFields(Type type)
			{
				BindingFlags flags =
					BindingFlags.Public | BindingFlags.NonPublic |
					BindingFlags.Static | BindingFlags.FlattenHierarchy;
				// We *do* need FlattenHierarchy here.
				// A parent type might have been skipped if it was generic (See IsValidZombieType)).
				// (TODO I wonder why generic types are skipped?)
				FieldInfo[] staticFields = type.GetFields(flags);
				foreach (FieldInfo field in staticFields)
					CheckObjectField(null, type, field);
			}


			public void Search (IEnumerable<Type> types)
			{
				foreach (Type type in types)
					CheckStaticFields(type);
				SearchCompleted?.Invoke();
			}
		}

		public event System.Action<SearchContext> SearchStarted;



		private IEnumerable<Assembly> GetAssemblies ()
		{
			IEnumerable<Assembly> assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

			// Ignore the ones whose names match any of m_ignoreAssembyPatterns.
			// There are many "names" to do with an assembly. We use the following:
			System.Func<Assembly, string> assemblyGetName = a => a.GetName().Name;
			var assemblyIgnoreRegexes = m_ignoreAssemblyPatterns
				.Select(x => new Regex(x))
				.ToList();
			System.Func<Assembly, bool> assemblyIsIgnored = a => assemblyIgnoreRegexes.Any(r => r.IsMatch(assemblyGetName(a)));
			assemblies = assemblies.Where(x => !assemblyIsIgnored(x));
			Debug.Log($"Assemblies are: {string.Join(", ", assemblies.Select(a => assemblyGetName(a)))}");
			return assemblies;
		}


		private IEnumerable<Type> GetStartTypes ()
		{
			IEnumerable<Assembly> assemblies = GetAssemblies();
			IEnumerable<Type> types = assemblies.SelectMany(a => a.GetTypes());

			// Ignore the ones whose names are in m_ignoreTypePatterns.
			var typeIgnoreRegexes = m_ignoreTypePatterns
				.Select(x => new Regex(x))
				.ToList();
			System.Func<Type, bool> typeIsIgnored = t => typeIgnoreRegexes.Any(r => r.IsMatch(t.FullName));
			types = types.Where(t => !typeIsIgnored(t));

			// Also ignore some types.  I cannot remember the reasoning here.
			types = types.Where(t => SearchContext.IsValidZombieType(t));

			Debug.Log($"Types are {string.Join(", ", types.Select(t => t.FullName))}");
			return types;
		}

		private void Start()
		{
			if (m_runOnStart)
				RunZombieObjectDetection();
		}

		public void RunZombieObjectDetection()
		{
			IEnumerable<Type> types = GetStartTypes();

			SearchContext ctx = new SearchContext(m_maxDepth, m_ignoreTypePatternsDuringSearch);
			if (SearchStarted != null)
				SearchStarted(ctx);

			ctx.Search(types);
		}

	}
}
