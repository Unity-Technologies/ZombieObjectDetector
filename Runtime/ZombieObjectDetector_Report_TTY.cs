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
			StringWriter w = new StringWriter();
			w.WriteLine($"Found zombie object of type {info.obj.GetType()}");
			w.WriteLine($"Member chain is:");
			w.WriteLine(info.memberChain.Last().DeclaringType);
			foreach (var m in info.memberChain.Reverse())
				w.WriteLine($"-> {m}");
			Debug.Log(w.ToString());
		}
	}


}