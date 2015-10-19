using UnityEngine;
using System.Collections;

public class CameraController : MyMonoBehaviour
{
	private Vector3 lookPointToPlayerVector; // カメラからプレイヤーまでの距離を格納
	private Transform lookPoint; // カメラの注視点
	private const float horizontalRotationSpeed = 30.0f; // 水平方向へのカメラ移動速度
	private const float verticaltalRotationSpeed = 30.0f; // 垂直方向へのカメラ移動速度
	private const float facingUpLimit = 30.0f; // 視点移動の上方向制限
	private const float facingDownLimit = 45.0f;  // 視点移動の下方向制限
	private float rotationAmount = 0.0f; // 縦方向に回転した量を蓄積

	void Start()
	{
		lookPoint = transform.parent; // このカメラの親の位置を注視点とする
		lookPointToPlayerVector = lookPoint.position - player.transform.position;
	}

	// Update is called once per frame
	protected override void LateUpdate()
	{
		float vx = Input.GetAxis("Mouse X");
		float vy = -Input.GetAxis("Mouse Y");

		CameraMove(vx, vy);
	}

	private void CameraMove(float vx, float vy)
	{
		lookPoint.position = player.transform.position + lookPointToPlayerVector;

		Vector3 angles = lookPoint.eulerAngles;
		rotationAmount += vy * horizontalRotationSpeed * Time.deltaTime;
		// 視点上下移動の制限
		if (vy < 0.0f && rotationAmount < -facingUpLimit)
		{
			rotationAmount = -facingUpLimit;
		}
		else if (vy > 0.0f && rotationAmount > facingDownLimit)
		{
			rotationAmount = facingDownLimit;
		}

		angles.x = rotationAmount;
		angles.y += vx * verticaltalRotationSpeed * Time.deltaTime;

		angles.z = 0.0f;
		lookPoint.eulerAngles = angles;
	}

}
