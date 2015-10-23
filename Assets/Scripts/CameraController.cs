using UnityEngine;
using System.Collections;

namespace Uxtuno
{
	/// <summary>
	/// カメラを制御するクラス
	/// 設計について:
	/// シングルトンクラス
	/// このクラスはPlayerクラスが持っていて
	/// Playerクラスからのみアクセス出来ると
	/// カメラは注視点を中心として回転する
	/// 注視点はシーン上に複数存在してよい
	/// 場面に応じて注視点を切り替えることで柔軟なカメラが可能
	/// プレイヤーは常に画面に映す
	/// </summary>
	public class CameraController : MyMonoBehaviour
	{
		private Transform playerLookPoint = null; // プレイヤー注視点
		private Transform currentLookPoint = null; // 現在の注視点(ロックオン対象など)
		[Tooltip("水平方向のカメラ移動速度"), SerializeField]
		private float horizontalRotationSpeed = 60.0f; // 水平方向へのカメラ移動速度
		[Tooltip("垂直方向のカメラ移動速度"), SerializeField]
		private float verticaltalRotationSpeed = 60.0f; // 垂直方向へのカメラ移動速度
		[Tooltip("上に向ける限界角度"), SerializeField]
		private float facingUpLimit = 5.0f; // 視点移動の上方向制限
		[Tooltip("下に向ける限界角度"), SerializeField]
		private float facingDownLimit = 45.0f;  // 視点移動の下方向制限
		private const float minDistance = 2.0f; // 注視点に近づける限界距離
		private float limitDistance; // 注視点から離れられる限界距離

		void Start()
		{

			// 初期状態の距離をカメラが離れられる最大距離とする
			limitDistance = (playerLookPoint.position - transform.position).magnitude;
		}

		// プレイヤーを追従する処理はプレイヤーの移動後に行う必要があるためここで行う
		protected void Update()
		{
			float vx = Input.GetAxis("Mouse X");
			float vy = Input.GetAxis("Mouse Y");

			CameraMove(vx, vy);
		}

		private void CameraMove(float vx, float vy)
		{
			Vector3 angles = transform.eulerAngles;

			angles.x += -vy * horizontalRotationSpeed * Time.deltaTime;
			if (angles.x > 180.0f)
			{
				angles.x -= 360.0f;
			}

			if (vy > 0.0f)
			{
				if(angles.x < -facingUpLimit)
				{
					angles.x = -facingUpLimit;
				}
			}

			if(vy < 0.0f)
			{
				if (angles.x > facingDownLimit)
				{
					angles.x = facingDownLimit;
				}
			}

			angles.y += vx * verticaltalRotationSpeed * Time.deltaTime;
			transform.eulerAngles = angles;

			//// 注視点からカメラへのベクトル
			//Vector3 lookPointToCamera = transform.position - playerLookPoint.position;
			//float lookPointDistance = lookPointToCamera.magnitude;
			//if (lookPointDistance > limitDistance)
			//{
			//	transform.position = playerLookPoint.position + lookPointToCamera.normalized * limitDistance;
			//}
			//else if (lookPointDistance < minDistance)
			//{
			//	transform.position = playerLookPoint.position + lookPointToCamera.normalized * minDistance;
			//}
			//lookPointToCamera = transform.position - playerLookPoint.position;
			//lookPointDistance = lookPointToCamera.magnitude;
			//transform.LookAt(playerLookPoint);

			//Vector3 position = Vector3.forward * lookPointDistance;
			//Quaternion q = Quaternion.LookRotation(lookPointToCamera);
			//Vector3 v = q.eulerAngles;
			//if (v.x > 180.0f)
			//{
			//	v.x -= 360.0f;
			//}

			//v.x += vy * verticaltalRotationSpeed * Time.deltaTime;
			//v.y += vx * horizontalRotationSpeed * Time.deltaTime;
			//v.z = 0.0f;
			//if (v.x < -facingDownLimit)
			//{
			//	v.x = -facingDownLimit;
			//}
			//else if (v.x > facingUpLimit)
			//{
			//	v.x = facingUpLimit;
			//}

			//q.eulerAngles = v;
			//position = q * position + playerLookPoint.position;
			//transform.position = position;
			//transform.LookAt(playerLookPoint);
		}
	}
}