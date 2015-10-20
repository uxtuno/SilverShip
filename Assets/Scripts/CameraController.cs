using UnityEngine;
using System.Collections;

public class CameraController : MyMonoBehaviour {
	private Vector3 cameraToPlayerVector; // カメラからプレイヤーまでの距離を格納
	void Start () {
		cameraToPlayerVector = transform.position - player.transform.position;
	}

	// Update is called once per frame
	protected override void LateUpdate()
	{
		transform.position = player.transform.position + cameraToPlayerVector;
	}
}
