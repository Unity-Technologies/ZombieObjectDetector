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

		// Assemblies whose name matches any of the following patterns will be skipped.
		[SerializeField]
		private string[] m_ignoreAssemblyPatterns = DefaultAssemblyIgnorePatterns;

		public static readonly string[] DefaultAssemblyIgnorePatterns = {
			"^Unity\\.", "^UnityEngine", "^UnityEditor", "^mscorlib$", "^System", "^Mono\\."
		};



		// Types whose name matches any of these regex patterns will be skipped.
		[SerializeField]
		private string[] m_ignoreTypePatterns = DefaultIgnoreTypePatterns;

		public static readonly string[] DefaultIgnoreTypePatterns =
		{
			"^System\\.", "^Unity\\.", "^UnityEngine\\.", "^UnityEditor\\.", "^Mono\\."
		};


		// An exception will be thrown if we recurse deeper than this.
		// (Avoids crashing the editor/player)
		[SerializeField]
		private int m_maxDepth = 200;





		// An individual invocation of a search.
		public class SearchContext
		{

			public struct ZombieHitInfo
			{
				public object obj;
				public FieldInfo[] fieldChain;
			};

			public event System.Action<ZombieHitInfo> ZombieHit;


			private HashSet<object> m_scannedObjects = new HashSet<object>();
			private Stack<FieldInfo> m_fieldInfos = new Stack<FieldInfo>();
			private int m_maxDepth;

			public SearchContext (int maxDepth)
			{
				m_maxDepth = maxDepth;
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



			private void CheckObjectField(object o, FieldInfo fieldInfo)
			{
				if (m_fieldInfos.Count > m_maxDepth)
					throw new System.OverflowException("Max depth exceeded.");

				m_fieldInfos.Push(fieldInfo);
				try
				{
					object val = fieldInfo.GetValue(o);
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

				if (!IsValidZombieType(obj.GetType()))
					return;

				if (m_scannedObjects.Contains(obj))
					return;
				m_scannedObjects.Add(obj);

				TestObject(obj);

				// Recurse into this object's instance fields.
				BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
				FieldInfo[] fields = obj.GetType().GetFields(flags);
				foreach (FieldInfo field in fields)
					CheckObjectField(obj, field);
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

				if (!zombie)
					return;

				if (ZombieHit != null)
					ZombieHit(new ZombieHitInfo
					{
						obj = obj,
						fieldChain = m_fieldInfos.ToArray()
					});
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
					CheckObjectField(null, field);
			}


			public void Search (IEnumerable<Type> types)
			{
				foreach (Type type in types)
				{
					if (type.Name == "Thing")
					{
						int x = 123;
					}
					CheckStaticFields(type);
				}
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


		public void RunZombieObjectDetection()
		{
			IEnumerable<Type> types = GetStartTypes();

			SearchContext ctx = new SearchContext(m_maxDepth);
			if (SearchStarted != null)
				SearchStarted(ctx);

			ctx.Search(types);
		}

	}
}
