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
		private void Awake()
		{
			var zod = GetComponent<ZombieObjectDetector>();
			zod.SearchStarted += StartSearch;
		}

		private void StartSearch(ZombieObjectDetector.SearchContext search)
		{
			var startTime = System.DateTime.Now;
			Debug.Log($"Search started at {startTime}");
			search.TestingObjectField += ReportTest;
			search.MadeProgress += () => PrintProgress(search);
			search.ZombieHit += (obj) => PrintHit(search, obj);
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