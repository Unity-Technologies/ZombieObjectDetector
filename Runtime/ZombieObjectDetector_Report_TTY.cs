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
			zod.SearchStarted += (search) => search.ZombieHit += PrintHit;
		}

		private void PrintHit(ZombieObjectDetector.SearchContext.ZombieHitInfo info)
		{
			string objType = info.obj.GetType().FullName;
			string startType = info.fieldChain.Last().DeclaringType.FullName;
			IEnumerable<string> chain = info.fieldChain.Reverse().Select(m => m.Name);
			Debug.Log($"Found zombie of type {objType}, at {startType}.{string.Join(".", chain)}");
		}
	}


}