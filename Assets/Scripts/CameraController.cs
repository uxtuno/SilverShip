using UnityEngine;
using System.Collections;

public class CameraController : MyMonoBehaviour
{
	private Vector3 lookPointToCameraVector; // カメラからプレイヤーまでの距離を格納
	[SerializeField]
	private Transform lookPoint; // カメラの注視点
	private const float horizontalRotationSpeed = 60.0f; // 水平方向へのカメラ移動速度
	private const float verticaltalRotationSpeed = 60.0f; // 垂直方向へのカメラ移動速度
	private const float facingUpLimit = 5.0f; // 視点移動の上方向制限
	private const float facingDownLimit = 45.0f;  // 視点移動の下方向制限
												  // private float rotationAmountXAxis = 0.0f; // X軸に回転した量を蓄積
												  // private float rotationAmountYAxis = 0.0f; // Y軸に回転した量を蓄積
	private const float minDistance = 2.0f; // 注視点に近づける限界距離
	private float limitDistance; // 注視点から離れられる限界距離
	private float defaultLookPointY; // 注視点の初期Y座標

	void Start()
	{
		// 初期状態の距離をカメラが離れられる最大距離とする
		limitDistance = (lookPoint.position - transform.position).magnitude;
		defaultLookPointY = lookPoint.position.y;
	}

	// Update is called once per frame
	protected override void LateUpdate()
	{
		float vx = Input.GetAxis("Mouse X");
		float vy = Input.GetAxis("Mouse Y");

		CameraMove(vx, vy);
	}

	private void CameraMove(float vx, float vy)
	{
		// 注視点からカメラへのベクトル
		Vector3 lookPointToCamera = transform.position - lookPoint.position;
		float lookPointDistance = lookPointToCamera.magnitude;
		if (lookPointDistance > limitDistance)
		{
			transform.position = lookPoint.position + lookPointToCamera.normalized * limitDistance;
		}
		else if (lookPointDistance < minDistance)
		{
			transform.position = lookPoint.position + lookPointToCamera.normalized * minDistance;
		}
		lookPointToCamera = transform.position - lookPoint.position;
		lookPointDistance = lookPointToCamera.magnitude;

		Vector3 position = Vector3.forward * lookPointDistance;
		Quaternion q = Quaternion.LookRotation(lookPointToCamera);
		Vector3 v = q.eulerAngles;
		if(v.x > 180.0f)
		{
			v.x -= 360.0f;
		}

		v.x += vy * verticaltalRotationSpeed * Time.deltaTime;
		v.y += vx * horizontalRotationSpeed * Time.deltaTime;
		v.z = 0.0f;
		if (v.x < -facingDownLimit)
		{
			v.x = -facingDownLimit;
		}
		else if(v.x > facingUpLimit)
		{
			v.x = facingUpLimit;
		}

		q.eulerAngles = v;
		position = q * position + lookPoint.position;
		//position.y += (lookPoint.position.y - defaultLookPointY) / 2.0f;
		Debug.Log(lookPoint.position.y - defaultLookPointY);
		transform.position = position;
		transform.LookAt(lookPoint);
	}

}
