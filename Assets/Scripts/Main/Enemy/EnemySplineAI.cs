using UnityEngine;
using System.Collections.Generic;

namespace Uxtuno
{
	[ExecuteInEditMode]
	public class MonsterSplineAI : MyMonoBehaviour
	{
		private Bezier curve = new Bezier();
		private List<Vector3> positions = new List<Vector3>();

		void OnDrawGizmos()
		{
			Vector3 o = curve.GetPointAtTime(0); ;
			for (float i = 0; i <= 1.0f; i += 1.0f / 10.0f)
			{
				float t = curve._bezier_linearlen(i);
				Vector3 v = curve.GetPointAtTime(t);
				Gizmos.DrawLine(o + transform.position, v + transform.position);
				o = v;
                Gizmos.DrawSphere(v + transform.position, 0.01f);
			}
		}

		void Start()
		{
			foreach (Transform child in transform)
			{
				positions.Add(child.position);
			}

			curve.Initialize(10, new Vector3(0, 0, 0),
				new Vector3(0, 1, 0),
				new Vector3(1, 1, 0),
				new Vector3(1, 0, 0));
		}

		// Update is called once per frame
		void Update()
		{
			//Debug.Log(curve._bezier_linearlen(0.5f));
		}
	}
}
