using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;


namespace CSharpZombieDetector
{



	public class ZombieObjectDetector_Report_TTY : MonoBehaviour
	{

		private event System.Action Destroyed;

		private void Awake()
		{
			var zod = GetComponent<ZombieObjectDetector>();
			zod.ZombieHit += PrintHit;
			Destroyed += () => zod.ZombieHit -= PrintHit;
		}

		private void OnDestroy()
		{
			if (Destroyed != null )
				Destroyed();
		}


		private void PrintHit(ZombieObjectDetector.ZombieHitInfo info)
		{
			string objType = info.obj.GetType().FullName;
			string startType = info.memberChain.Last().DeclaringType.FullName;
			IEnumerable<string> chain = info.memberChain.Reverse().Select(m => m.Name);
			Debug.Log($"Found zombie of type {objType}, at {startType}.{string.Join(".", chain)}");
		}
	}


}