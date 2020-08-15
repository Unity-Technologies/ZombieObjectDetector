using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CSharpZombieDetector
{

    public class ZombieObjectDetector : MonoBehaviour
    {
        const BindingFlags k_AllFields =
            BindingFlags.NonPublic |        // Include protected and private
            BindingFlags.Public |           // Also include public 
            BindingFlags.FlattenHierarchy | // Include parent members
            BindingFlags.Static |           // Specify to retrieve static
            BindingFlags.Instance;          // Include instance members

        const BindingFlags k_AllNonStaticFields =
            BindingFlags.NonPublic |        // Include protected and private
            BindingFlags.Public |           // Also include public 
            BindingFlags.FlattenHierarchy | // Include parent members
            BindingFlags.Instance;          // Include instance members


		// Assemblies whose name matches any of the following patterns will be skipped.
		[SerializeField]
		private List<string> m_ignoreAssemblyPatterns = new List<string>();

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

		// When we ask for members, whether static members of a Type, or instance members of an object,
		// we only care about these member types.
		private const MemberTypes RelevantMemberTypes = MemberTypes.Field | MemberTypes.Event;
		// i.e. we don't care about methods, etc.
		//
		// (TODO:  What about auto-backed properties?  Do they end up having a secret field?)


        Stack<MemberInfo> m_MemberInfoChain = new Stack<MemberInfo>();

        Stack<MemberInfo> m_ScannedStaticMembers = new Stack<MemberInfo>();

		HashSet<object> m_ScannedObjects = new HashSet<object>();

		// Types for which we have encountered "trouble".
		// (e.g. .Equals throws exception).
		// TODO I can't remember what the point of this was.  Remove?
		HashSet<Type> m_InvalidTypes = new HashSet<Type>();

        object m_TestObject = new object();

		// Prevent concurrent runs.
		private bool m_IsLogging = false;
		public bool IsLogging() {
			return m_IsLogging;
		}
		

        public void RunZombieObjectDetection()
        {
            if (!m_IsLogging)
            {
                StartCoroutine(LogZombies());
            }
            else
            {
                Debug.LogWarning("ZombieDetector Already Logging!");
            }
        }



        private void OnGUI()
        {
            if (m_IsLogging)
            {
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Logging Zombie Objects!");
            }
        }

        private IEnumerator LogZombies()
        {
            m_IsLogging = true;

            yield return new WaitForEndOfFrame();

            ProcessAllObjectsFromStaticRoots();

            m_IsLogging = false;

            yield return null;
        }

        private void ProcessAllObjectsFromStaticRoots()
        {

            m_MemberInfoChain.Clear();
            m_ScannedObjects.Clear();
            m_ScannedStaticMembers.Clear();
            m_InvalidTypes.Clear();


			// Iterate through all Types from all Assemblies.
			// (apart from ones we've been told to ignore)

			// Assemblies.
			// There are many "names" to do with an assembly. We use the following:
			System.Func<Assembly, string> assemblyGetName = a => a.GetName().Name;
			var assemblyIgnoreRegexes = m_ignoreAssemblyPatterns
				.Select(x => new Regex(x))
				.ToList();
			IEnumerable<Assembly> assemblies =
				System.AppDomain.CurrentDomain.GetAssemblies()
				.Where(x => !assemblyIgnoreRegexes.Any(r => r.IsMatch(assemblyGetName(x))));
			Debug.Log($"Assemblies are: {string.Join(", ", assemblies.Select(a => assemblyGetName(a)))}");

			// Types
			var typeIgnoreRegexes = m_ignoreTypePatterns
				.Select(x => new Regex(x))
				.ToList();
			IEnumerable<Type> types = assemblies
				.SelectMany(x => x.GetTypes())
				.Where (t => ! typeIgnoreRegexes.Any (r => r.IsMatch (t.FullName)))
				.Where (t => IsValidZombieType (t))
				.ToArray();

			Debug.Log($"Types are {string.Join(", ", types.Select(t => t.FullName))}");

            foreach (Type type in types)
            {
                m_MemberInfoChain.Clear();
                TraverseStaticMembersFromType(type);
            }
        }

        private void TraverseStaticMembersFromType(Type type)
        {
			//Debug.Log($"Type: {type.FullName}");
			var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;
			// (Do we want to FlattenHierarchy? Hmm)

			var memberInfos = type.GetMembers(flags)
				.Where(m => (m.MemberType & RelevantMemberTypes) != 0);

            foreach (MemberInfo memberInfo in memberInfos)
            {
                m_MemberInfoChain.Push(memberInfo);
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Event:
                        TraverseMemberEvent(memberInfo, null);
                        break;
                    case MemberTypes.Field:
                        TraverseMemberField(memberInfo, null);
                        break;
                    default:
                        break;
                }
                m_MemberInfoChain.Pop();
            }
        }

        private void TraverseMemberEvent(MemberInfo memberInfo, object memberParentObject)
        {
            FieldInfo fieldInfo = memberInfo.ReflectedType.GetField(memberInfo.Name, k_AllFields);
			Debug.Assert(fieldInfo != null);
			if (fieldInfo == null)
				return;


			if (!IsValidZombieType (fieldInfo.FieldType))
				return;
			// (Hmm, is this even required?  Won't it fail the "as MulticastDelegate" cast below?)

            MulticastDelegate eventMulticastDelegate = fieldInfo.GetValue(memberParentObject) as MulticastDelegate;
			if (eventMulticastDelegate == null)
				return;
			
            Delegate[] delegates = eventMulticastDelegate.GetInvocationList();
            foreach (Delegate eventDelegate in delegates)
            {
                object delegateObject = eventDelegate.Target;
				if (delegateObject == null)
					continue;
				
                // Search all of the delegateObjects members.
                TraverseAllMembersFromObject(delegateObject);
            }
        }

        private void TraverseMemberField(MemberInfo memberInfo, object memberParentObject)
        {
            FieldInfo fieldInfo = memberInfo.ReflectedType.GetField(memberInfo.Name, k_AllFields);

			if (fieldInfo == null) {
				return;
			}


			if (!IsValidZombieType (fieldInfo.FieldType))
				return;

            if (fieldInfo.FieldType.IsArray)
            {
                IEnumerable collection = fieldInfo.GetValue(memberParentObject) as IEnumerable;
                if (collection != null)
                {
                    foreach (object collectionObject in collection)
                    {
                        // if it is refrencing something
                        if (collectionObject != null)
                        {
                            // search all of its members
                            TraverseAllMembersFromObject(collectionObject);
                        }
                    }
                }
            }
            else
            { // try to get the value of everything else.

                try
                {
                    object fieldObj = fieldInfo.GetValue(memberParentObject) as object;

                    // if it is refrencing something
                    if (fieldObj != null)
                    {
                        // search all of its members
                        TraverseAllMembersFromObject(fieldObj);
                    }
                }
                catch (Exception)
                {
					// TODO Report this using an event.
                }
            }
        }

        private void TraverseAllMembersFromObject(object obj)
        {
			if (!IsValidZombieType (obj.GetType ()))
				return;

			if (m_ScannedObjects.Contains(obj))
				return;

            try
            {
                // Checks if .Equals has been implemented properly before adding to the object list.
                obj.Equals(m_TestObject);
            }
            catch (Exception e)
            {

                if (!m_InvalidTypes.Contains(obj.GetType()))
                {
                    m_InvalidTypes.Add(obj.GetType());
                }
                return;
            }

            LogIsZombieObject(obj);

            List<MemberInfo> memberInfos = obj.GetType().GetMembers(k_AllNonStaticFields).ToList();

            foreach (MemberInfo memberInfo in memberInfos)
            {
                m_MemberInfoChain.Push(memberInfo);
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Event:
                        TraverseMemberEvent(memberInfo, obj);
                        break;
                    case MemberTypes.Field:
                        TraverseMemberField(memberInfo, obj);
                        break;
                    default:
                        break;
                }
                m_MemberInfoChain.Pop();
            }
        }


		public struct ZombieHitInfo
		{
			public object obj;
			public IEnumerable<MemberInfo> memberChain;
		};

		public event System.Action<ZombieHitInfo> ZombieHit;

        /// <summary>
        /// Uses Unity Fake Null
        /// https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
        /// https://forum.unity.com/threads/fun-with-null.148090/
        /// ToString results in "null" when unity has passed the object to garbage collector.
        /// </summary>
        private void LogIsZombieObject(object obj)
        {
			UnityEngine.Object unityObj = obj as UnityEngine.Object;
			bool isZombie = obj.ToString () == "null";
			m_ScannedObjects.Add(obj);
			if (!isZombie)
				return;

			if (ZombieHit != null)
				ZombieHit(new ZombieHitInfo
				{
					obj = obj,
					memberChain = m_MemberInfoChain
				});
		}


        private bool IsValidZombieType(Type type)
        {
            bool result = false;

            if (!TypeHelper.IsZombieType(type))
            {
                // shouldnt handle basic value types (int,float etc):/
            }
            else if (m_InvalidTypes.Contains(type))
            {
            }
            else if (type.IsPointer)
            {
                // shouldn't try to handle unsafe c# style pointers.
            }
            else if (type.ContainsGenericParameters || type.IsGenericParameter || type.IsGenericTypeDefinition)
            {
                // Doesn't currently handle Ex Singleton<T> : MonoBehaviour where T : MonoBehaviour: nicely. 
                // but all derived classes will get scanned, including parent members.
            }
            else
            {
                result = true;
            }
            return result;
        }

    }
}
