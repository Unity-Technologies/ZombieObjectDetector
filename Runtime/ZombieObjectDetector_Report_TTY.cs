using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Reflection;


namespace CSharpZombieDetector
{



	public class ZombieObjectDetector_Report_TTY : MonoBehaviour
	{

		[SerializeField]
		[Tooltip("Print every test performed (VERY verbose)")]
		private bool m_reportTest = false;

		[SerializeField]
		[Tooltip("Print when a zombie is encountered (You probably want this on)")]
		private bool m_reportHit = true;

		[SerializeField]
		[Tooltip("Print a message every 1024 tests (useful to show it hasn't hung)")]
		private bool m_reportProgress = false;

		[SerializeField]
		[Tooltip("Print when the search completes")]
		private bool m_reportCompletion = true;

		private void Awake()
		{
			var zod = GetComponent<ZombieObjectDetector>();
			zod.SearchStarted += StartSearch;
		}

		private void StartSearch(ZombieObjectDetector.SearchContext search)
		{
			var startTime = System.DateTime.Now;
			Debug.Log($"Search started at {startTime}");
			if (m_reportTest)
				search.TestingObjectField += ReportTest;
			if (m_reportProgress)
				search.MadeProgress += () => PrintProgress(search);
			if (m_reportHit)
				search.ZombieHit += (obj) => PrintHit(search, obj);
			if (m_reportCompletion)
				search.SearchCompleted += () => CompleteSearch(search, startTime);
		}

		private void PrintProgress (ZombieObjectDetector.SearchContext ctx)
		{
			Debug.Log($"Searched {ctx.NumTestsPerformed} objects.");
		}

		private void ReportTest (ZombieObjectDetector.SearchContext.TestInfo info)
		{
			System.Type type = info.type;
			FieldInfo fieldInfo = info.fieldInfo;

			Debug.Log($"Testing field {fieldInfo.Name} of type {type.FullName}.");
		}

		private void PrintHit(ZombieObjectDetector.SearchContext ctx, object o)
		{
			string objType = o.GetType().FullName;
			var fieldChain = ctx.FieldInfoChain;
			// The class holding the very first field
			// (The one with the static field)
			System.Type startType = fieldChain.Last().DeclaringType;
			IEnumerable<string> chain = fieldChain.Reverse().Select(m => m.Name);
			Debug.Log($"Found zombie of type {objType}, at {startType.FullName}.{string.Join(".", chain)}");
		}

		private void CompleteSearch (ZombieObjectDetector.SearchContext ctx, System.DateTime startTime)
		{
			var now = System.DateTime.Now;
			System.TimeSpan duration = now - startTime;
			Debug.Log($"Search completed at {now} after {duration}.  Tested {ctx.NumTestsPerformed} object(s).");
		}

		
	}


}