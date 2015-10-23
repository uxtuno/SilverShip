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
		private Transform _cameraTransform; // 制御するカメラ

		/// <summary>
		/// 制御しているカメラ自体のTransformを返す
		/// </summary>
		public Transform cameraTransform
		{
			get
			{
				if (_cameraTransform == null)
				{
					_cameraTransform = GetComponentInChildren<Camera>().transform;
				}
				return _cameraTransform;
			}
		}

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
		private Quaternion newRotation; // 新しいカメラ角度
		private float time; // 補間中の時間
		private float interpolationTime = 0.5f; // 補間時間(秒)

		void Start()
		{
			newRotation = transform.rotation;
			time = 1.0f; // 1.0f == 補間完了
		}

		void Update()
		{
			//float vx = Input.GetAxis("Mouse X");
			//float vy = Input.GetAxis("Mouse Y");

			//CameraMove(vx, vy);
		}

		public void CameraMove(float vx, float vy)
		{
			if (vx == 0.0f && vy == 0.0f)
			{
				return;
			}
			time = 0.0f; // 補間開始
			Vector3 angles = transform.eulerAngles;

			angles.x += -vy * horizontalRotationSpeed * Time.deltaTime;
			if (angles.x > 180.0f)
			{
				angles.x -= 360.0f;
			}

			if (vy > 0.0f)
			{
				if (angles.x < -facingUpLimit)
				{
					angles.x = -facingUpLimit;
				}
			}

			if (vy < 0.0f)
			{
				if (angles.x > facingDownLimit)
				{
					angles.x = facingDownLimit;
				}
			}

			angles.y += vx * verticaltalRotationSpeed * Time.deltaTime;
			transform.eulerAngles = angles;
		}
	}
}