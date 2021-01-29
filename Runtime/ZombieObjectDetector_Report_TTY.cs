using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;


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
			search.ZombieHit += PrintHit;
			search.SearchCompleted += () => CompleteSearch(startTime);
		}


		private void PrintHit(ZombieObjectDetector.SearchContext.ZombieHitInfo info)
		{
			string objType = info.obj.GetType().FullName;
			string startType = info.fieldChain.Last().DeclaringType.FullName;
			IEnumerable<string> chain = info.fieldChain.Reverse().Select(m => m.Name);
			Debug.Log($"Found zombie of type {objType}, at {startType}.{string.Join(".", chain)}");
		}

		private void CompleteSearch (System.DateTime startTime)
		{
			var now = System.DateTime.Now;
			System.TimeSpan duration = now - startTime;
			Debug.Log($"Search completed at {now} after {duration}.");
		}

		
	}


}