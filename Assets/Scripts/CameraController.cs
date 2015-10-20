using UnityEngine;
using System.Collections;

public class CameraController : MyMonoBehaviour
{
	private Vector3 lookPointToCameraVector; // カメラからプレイヤーまでの距離を格納
	[SerializeField]
	private Transform lookPoint; // カメラの注視点
	private const float horizontalRotationSpeed = 30.0f; // 水平方向へのカメラ移動速度
	private const float verticaltalRotationSpeed = 30.0f; // 垂直方向へのカメラ移動速度
	private const float facingUpLimit = 30.0f; // 視点移動の上方向制限
	private const float facingDownLimit = 45.0f;  // 視点移動の下方向制限
	private float rotationAmount = 0.0f; // 縦方向に回転した量を蓄積
	private const float minDistance = 2.0f; // 注視点に近づける限界距離
	private float limitDistance; // 注視点から離れられる限界距離
	private float defaultLookPointY; // 注視点の初期Y座標

	void Start()
	{
		lookPointToCameraVector = transform.position - lookPoint.position;
		limitDistance = (lookPoint.position - transform.position).magnitude;
		defaultLookPointY = lookPoint.position.y;
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

		float lookPointDistance = (lookPoint.position - transform.position).magnitude;
		if (lookPointDistance > limitDistance)
		{
			Vector3 vec = (transform.position - lookPoint.position).normalized * limitDistance;
			transform.position = lookPoint.position + vec;
		}
		else if (lookPointDistance < minDistance)
		{
			Vector3 vec = (transform.position - lookPoint.position).normalized * minDistance;
			transform.position = lookPoint.position + vec;
		}

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

		Vector3 position = Vector3.zero;
		float distance = (transform.position - lookPoint.position).magnitude;
        position.x = Mathf.Cos(rotationAmount * Mathf.Deg2Rad) * distance;
        position.z = Mathf.Sin(rotationAmount * Mathf.Deg2Rad) * distance;

		//transform.RotateAround(lookPoint.position, transform.rotation * Vector3.right, vy * horizontalRotationSpeed * Time.deltaTime);
		//transform.RotateAround(lookPoint.position, transform.rotation * Vector3.up, vx * horizontalRotationSpeed * Time.deltaTime);
		Debug.Log(lookPoint.position);
		lookPoint.eulerAngles = angles;
		transform.LookAt(lookPoint);
	}

}
