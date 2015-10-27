using UnityEngine;
using System.Collections;

namespace Uxtuno
{
	public class Enemy : MyMonoBehaviour
	{
		private CameraController cameraController;
		void Start()
		{
			cameraController = GameObject.FindGameObjectWithTag(TagName.CameraController).GetComponent<CameraController>();
		}

		void Update()
		{
			Vector3 cameraToVector = cameraController.cameraTransform.position - transform.position;
			Debug.Log(cameraToVector.magnitude);
			if (cameraToVector.magnitude < 2.0f)
			{
				isShow = false;
			}
			else
			{
				isShow = true;
			}
		}
	}
}