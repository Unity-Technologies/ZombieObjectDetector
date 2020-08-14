using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CSharpZombieDetector
{

    public class ZombieObjectDetector : MonoBehaviour
    {
        const BindingFlags k_AllStaticFields =
            BindingFlags.NonPublic |        // Include protected and private
            BindingFlags.Public |           // Also include public 
            BindingFlags.FlattenHierarchy | // Include parent members
            BindingFlags.Static;            // Specify to retrieve static

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

        [Flags]
        public enum LoggingOptions
        {
            InvalidType                     = 1 << 0,   // Logs information about type that are ignroed because they are invalid for causing zombie objects (int,float etc)
            ListBadEqualsImplementations    = 1 << 1,   // Lists all Bad .Equals Implementations that were found causing the object and all its members to be ignored.
            ListScannedObjects              = 1 << 2,   // Lists all objects that have had there members checked for zombie objects.
            ListScannedStaticMembers        = 1 << 3,   // List of all Static Members that have been checked from there roots.
            ZombieCountForEachStaticField   = 1 << 4,   // Individual Zombie Object count for Static field roots.
            ZombieStackTrace                = 1 << 5    // Default zombie object stack trace, used to find root to zombie object causes of leaks.
        }

        [Flags]
        public enum DebugLoggingOptions
        {
            Exceptions                      = 1 << 0,   // Exceptions that are generally ignored as logged more succinctly else where.
            IgnoredMembers                  = 1 << 1,   // Logs each member that is ignored by the zombie detector.
            FieldInfoNotFound               = 1 << 2,   // Reflection failed to find the FieldInfo for the connected object.
            MemberType                      = 1 << 3,   // Logs each Members Type.
            FieldType                       = 1 << 4,   // Logs each Member of type Field's type.
            AlreadyScanned                  = 1 << 5    // Logs Objects that are ignored as they have already been scanned previously.
        }

        [SerializeField]
        LoggingOptions m_LoggingOptions = LoggingOptions.ZombieStackTrace | LoggingOptions.ZombieCountForEachStaticField;

        [SerializeField]
        DebugLoggingOptions m_DebugLoggingOptions;

        [SerializeField]
        string m_LogTag = "";

        [SerializeField]
        string[] m_IgnoredTypeStrings = new string[0];

        [SerializeField]
        string[] m_TypesToScanStrings = new string[0];

        [SerializeField]
        KeyCode m_LogZombieKeyCode = KeyCode.None;

        bool m_IsLogging = false;

        Stack<MemberInfo> m_MemberInfoChain = new Stack<MemberInfo>();

        Stack<MemberInfo> m_ScannedStaticMembers = new Stack<MemberInfo>();

		HashSet<object> m_ScannedObjects = new HashSet<object>();

        List<Type> m_InvalidTypes = new List<Type>();

        float m_Progress = 0.0f;

        float m_ProgressIncrements = 0.0f;

        object m_TestObject = new object();

        int m_ZombieObjectCount = 0;

        int m_TotalZombieObjectCount = 0;

        int m_TotalMembersLookedAt = 0;

        StreamWriter m_FileOutputStream;

        string m_LogFileFolder;

        int m_TotalScannedStaticMembers = 0;

        public bool IsLogging()
        {
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

        private void Start()
        {
            m_LogFileFolder = Path.Combine(Application.dataPath, ".ZombieLogs");
        }
        private void Update()
        {
            if (Input.GetKeyDown(m_LogZombieKeyCode))
            {
                RunZombieObjectDetection();
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
            // Just to clean up stack traces from debug.Log so easier to read.
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

            if (m_LoggingOptions == 0)
            {
                Debug.LogWarning("No Logging Options Set");
                return;
            }
            CreateFile();

            m_MemberInfoChain.Clear();
            m_ScannedObjects.Clear();
            m_ScannedStaticMembers.Clear();
            m_InvalidTypes.Clear();

            m_TotalZombieObjectCount = 0;
            m_TotalMembersLookedAt = 0;
            m_TotalScannedStaticMembers = 0;

            Log("Logging Zombie Objects");
            Log(string.Format("Logging Options: {0}{1}IgnoredTypes: {2}{1}ScannedTypes: {3}",
                m_LoggingOptions.ToString(),
                Environment.NewLine,
                string.Join(", ", m_IgnoredTypeStrings),
                string.Join(", ", m_TypesToScanStrings)
                ));

            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();

            m_ProgressIncrements = 1.0f / types.Length;
            m_Progress = 0.0f;

            foreach (Type type in types)
            {
                m_Progress += m_ProgressIncrements;

                // only care about classes and structs, and valid types.
				if (!IsValidZombieType (type))
					continue;
				

				// skip types not in includedInitialTypes, if it is non-empty.
                if (m_TypesToScanStrings.Length > 0)
                {
                    if (!m_TypesToScanStrings.Contains(type.FullName))
                    {
                        continue;
                    }
                }
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayProgressBar("ZombieDetector",
                    " Searching: " + type.FullName.ToString(),
                    m_Progress);
#endif

                m_MemberInfoChain.Clear();
                TraverseStaticMembersFromType(type);

            }
            if (HasLoggingOptions(LoggingOptions.ListBadEqualsImplementations))
            {
                if (m_InvalidTypes.Count > 0)
                {
                    Log(string.Format("Bad Implementation of .Equals In Types: {0}{1}{2}",
                        m_InvalidTypes.Count.ToString(),
                        Environment.NewLine,
                        string.Join(Environment.NewLine,
                        m_InvalidTypes.Select(x => x.ToString()).Reverse().ToArray())));
                }
            }

            if (HasLoggingOptions(LoggingOptions.ListScannedObjects))
            {
                if (m_ScannedObjects.Count > 0)
                {
                    Log(string.Format("Scanned Objects: {0}{1}{2}",
                        m_ScannedObjects.Count.ToString(),
                        Environment.NewLine,
                        string.Join(Environment.NewLine,
                        m_ScannedObjects.Select(x => x.GetType().ToString()).Reverse().ToArray())));
                }
            }

            if (HasLoggingOptions(LoggingOptions.ListScannedStaticMembers))
            {
                if (m_ScannedStaticMembers.Count > 0)
                {
                    Log(string.Format("Scanned Static Members: {0}{1}{2}",
                        m_ScannedStaticMembers.Count.ToString(),
                        Environment.NewLine,
                        FormatStackOfMemberInfo(m_ScannedStaticMembers)));
                }
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif

            Log(string.Format("Scan Complete: Zombie Objects Found:{1}{0}Total Scanned Objects:{2}, Total Scanned Static Members:{3}, Total Members:{4}",
                Environment.NewLine,
                m_TotalZombieObjectCount.ToString(),
                m_ScannedObjects.Count.ToString(),
                m_TotalScannedStaticMembers.ToString(),
                m_TotalMembersLookedAt.ToString()));

            CloseFile();


            // Reset the logging type.
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
        }

        private void TraverseStaticMembersFromType(Type type)
        {

            List<MemberInfo> memberInfos = type.GetMembers(k_AllStaticFields).ToList();

            foreach (MemberInfo memberInfo in memberInfos)
            {
                m_TotalMembersLookedAt++;
                m_ZombieObjectCount = 0;
                m_MemberInfoChain.Push(memberInfo);
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Event:
                        if (HasDebugLoggingOptions(DebugLoggingOptions.MemberType))
                        {
                            Log("Static Event: " + FormatMemberInfo(memberInfo));
                        }
                        if (HasLoggingOptions(LoggingOptions.ListScannedStaticMembers))
                        {
                            m_ScannedStaticMembers.Push(memberInfo);
                        }
                        m_TotalScannedStaticMembers++;
                        TraverseMemberEvent(memberInfo, null);
                        break;
                    case MemberTypes.Field:
                        if (HasDebugLoggingOptions(DebugLoggingOptions.MemberType))
                        {
                            Log("Static Field: " + FormatMemberInfo(memberInfo));
                        }
                        if (HasLoggingOptions(LoggingOptions.ListScannedStaticMembers))
                        {
                            m_ScannedStaticMembers.Push(memberInfo);
                        }
                        m_TotalScannedStaticMembers++;
                        TraverseMemberField(memberInfo, null);
                        break;
                    default:
                        if (HasDebugLoggingOptions(DebugLoggingOptions.IgnoredMembers))
                        {
                            Log("Static Member Ignored: " + memberInfo.MemberType);
                        }
                        break;
                }
                m_MemberInfoChain.Pop();

                if (HasLoggingOptions(LoggingOptions.ZombieCountForEachStaticField))
                {
                    if (m_ZombieObjectCount > 0)
                    {
                        LogWarning(string.Format("Number of ZombieObjects: {0}{1}{2}",
                            m_ZombieObjectCount, Environment.NewLine, FormatMemberInfo(memberInfo)));
                    }
                }
            }
        }

        private void TraverseMemberEvent(MemberInfo memberInfo, object memberParentObject)
        {
            FieldInfo fieldInfo = memberInfo.ReflectedType.GetField(memberInfo.Name, k_AllFields);

			if (fieldInfo == null) {
				if (HasDebugLoggingOptions (DebugLoggingOptions.FieldInfoNotFound)) {
					Log (string.Format ("Failed to find FieldInfo: Type: {0} Member: {1}",
						memberInfo.ReflectedType,
						memberInfo.Name));
				}
				return;
			}
				
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
				if (HasDebugLoggingOptions (DebugLoggingOptions.FieldInfoNotFound)) {
					Log (string.Format ("Failed to find FieldInfo: Type: {0} Member: {1}",
						memberInfo.ReflectedType,
						memberInfo.Name));
				}
				return;
			}


			if (!IsValidZombieType (fieldInfo.FieldType))
				return;

            if (fieldInfo.FieldType.IsArray)
            {
                if (HasDebugLoggingOptions(DebugLoggingOptions.FieldType))
                {
                    Log("Array: " + FormatMemberInfo(memberInfo));
                }

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
                    if (HasDebugLoggingOptions(DebugLoggingOptions.FieldType))
                    {
                        Log("Other: " + FormatMemberInfo(memberInfo));
                    }
                    object fieldObj = fieldInfo.GetValue(memberParentObject) as object;

                    // if it is refrencing something
                    if (fieldObj != null)
                    {
                        // search all of its members
                        TraverseAllMembersFromObject(fieldObj);
                    }
                }
                catch (Exception e)
                {
                    if (HasDebugLoggingOptions(DebugLoggingOptions.Exceptions))
                    {
                        Debug.LogErrorFormat("Error: {0}{1}{2}",
                            FormatMemberInfo(memberInfo),
                            Environment.NewLine,
                            e);
                    }
                }
            }
        }

        private void TraverseAllMembersFromObject(object obj)
        {
			if (!IsValidZombieType (obj.GetType ()))
				return;
			
            try
            {
                // Checks if .Equals has been implemented properly before adding to the object list.
                obj.Equals(m_TestObject);

                if (m_ScannedObjects.Contains(obj))
                {
                    if (HasDebugLoggingOptions(DebugLoggingOptions.AlreadyScanned))
                    {
                        Log("Already Scanned: " + obj.GetType());
                    }
                    return;
                }
            }
            catch (Exception e)
            {

                if (HasDebugLoggingOptions(DebugLoggingOptions.Exceptions))
                {
                    Debug.LogErrorFormat("Error In Objects .Equals: {0}{1}{2}",
                        obj.GetType(),
                        Environment.NewLine,
                        e);
                }
                if (!m_InvalidTypes.Contains(obj.GetType()))
                {
                    m_InvalidTypes.Add(obj.GetType());
                }
                return;
            }

            m_ScannedObjects.Add(obj);

            LogIsZombieObject(obj);

            List<MemberInfo> memberInfos = obj.GetType().GetMembers(k_AllNonStaticFields).ToList();

            foreach (MemberInfo memberInfo in memberInfos)
            {
                m_TotalMembersLookedAt++;
                m_MemberInfoChain.Push(memberInfo);
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Event:
                        if (HasDebugLoggingOptions(DebugLoggingOptions.MemberType))
                        {
                            Log("Event: " + FormatMemberInfo(memberInfo));
                        }
                        TraverseMemberEvent(memberInfo, obj);
                        break;
                    case MemberTypes.Field:
                        if (HasDebugLoggingOptions(DebugLoggingOptions.MemberType))
                        {
                            Log("Field: " + FormatMemberInfo(memberInfo));
                        }
                        TraverseMemberField(memberInfo, obj);
                        break;
                    default:
                        if (HasDebugLoggingOptions(DebugLoggingOptions.IgnoredMembers))
                        {
                            Log("Member Ignored: " + memberInfo.MemberType);
                        }
                        break;
                }
                m_MemberInfoChain.Pop();
            }
        }

        /// <summary>
        /// Uses Unity Fake Null
        /// https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
        /// https://forum.unity.com/threads/fun-with-null.148090/
        /// ToString results in "null" when unity has passed the object to garbage collector.
        /// </summary>
        private void LogIsZombieObject(object obj)
        {
			bool isZombie = obj.ToString () == "null";

			if (!isZombie)
				return;
			
            m_TotalZombieObjectCount++;
            m_ZombieObjectCount++;
            if (HasLoggingOptions(LoggingOptions.ZombieStackTrace))
            {
                // Ouput the Zombie Object type, as obj.ToString() => "null" due to unity wiping the data.
                LogWarning(string.Format("ZombiedObject: {0}{1}{2}",
                    obj.GetType(),
                    Environment.NewLine,
                    FormatStackOfMemberInfo(m_MemberInfoChain)));
            }
        }

        private string FormatStackOfMemberInfo(Stack<MemberInfo> memberInfoList)
        {
            return string.Join(Environment.NewLine, memberInfoList.Select(x => FormatMemberInfo(x)).ToArray());
        }

        private string FormatMemberInfo(MemberInfo memberInfo)
        {
            return string.Format("->Class: {0}: Member: {1}", memberInfo.ReflectedType, memberInfo.ToString());
        }

        private bool IsValidZombieType(Type type)
        {
            bool result = false;

            if (!TypeHelper.IsZombieType(type))
            {
                if (HasLoggingOptions(LoggingOptions.InvalidType))
                {
                    Log(string.Format("InvalidType: Basic value types: {0}", type));
                }
                // shouldnt handle basic value types (int,float etc):/
            }
            else if (m_InvalidTypes.Contains(type))
            {
                if (HasLoggingOptions(LoggingOptions.InvalidType))
                {
                    Log(string.Format("InvalidType: Already found: {0}", type));
                }
            }
            else if (type.IsPointer)
            {
                if (HasLoggingOptions(LoggingOptions.InvalidType))
                {
                    Log(string.Format("InvalidType: C# style pointer: {0}", type));
                }
                // shouldn't try to handle unsafe c# style pointers.
            }
            else if (type.ContainsGenericParameters || type.IsGenericParameter || type.IsGenericTypeDefinition)
            {
                if (HasLoggingOptions(LoggingOptions.InvalidType))
                {
                    Log(string.Format("InvalidFieldType: GenericParamType: {0}{1}{2}",
                        type,
                        Environment.NewLine,
                        "Ex Singleton<T> : MonoBehaviour where T: MonoBehaviour"));
                }
                // Doesn't currently handle Ex Singleton<T> : MonoBehaviour where T : MonoBehaviour: nicely. 
                // but all derived classes will get scanned, including parent members.
            }
            else if (m_IgnoredTypeStrings.Contains(type.FullName))
            {
                if (HasLoggingOptions(LoggingOptions.InvalidType))
                {
                    Log(string.Format("InvalidType: User Defined Ignore: {0}", type));
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        private void CreateFile()
        {
            try
            {
                Directory.CreateDirectory(m_LogFileFolder);

                m_FileOutputStream = File.CreateText(Path.Combine(m_LogFileFolder,
                    string.Format("{0}_{1}_{2}{3}", "ZombieDetector",
                    DateTime.UtcNow.ToString("yyyy-dd-M--HH-mm-ss"),
                    m_LogTag,
                    ".log")));
            }
            catch (Exception)
            {
                Debug.LogWarning(string.Format("Failed to write to path:{0}{1}This is due to a write permision failure. i.e Console Packaged Builds.{1}If on Console try a Push-Build.",
                    m_LogFileFolder, Environment.NewLine));
                m_FileOutputStream = null;
            }

        }

        private void CloseFile()
        {
            if (m_FileOutputStream != null)
            {
                m_FileOutputStream.Close();
                Debug.Log("ZombieLog Saved to: " + m_LogFileFolder);
            }
            else
            {
                Debug.Log("ZombieLog Failed to Save to: " + m_LogFileFolder);
            }
        }

        private bool HasLoggingOptions(LoggingOptions loggingOptionsToCheckFor)
        {
            return ((m_LoggingOptions & loggingOptionsToCheckFor) != 0);
        }

        private bool HasDebugLoggingOptions(DebugLoggingOptions loggingOptionsToCheckFor)
        {
            return ((m_DebugLoggingOptions & loggingOptionsToCheckFor) != 0);
        }

        private void Log(string outputText)
        {
            Debug.Log(outputText);
            if (m_FileOutputStream != null)
            {
                m_FileOutputStream.WriteLine(Environment.NewLine + outputText);
            }
        }

        private void LogWarning(string outputText)
        {
            //Debug.LogWarning(outputText);
            if (m_FileOutputStream != null)
            {
                m_FileOutputStream.WriteLine(Environment.NewLine + outputText);
            }
        }
    }
}
