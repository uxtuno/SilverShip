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

		[Tooltip("カメラで追いかける対象"), SerializeField]
		private Transform target = null;
		[Tooltip("上に向ける限界角度"), SerializeField]
		private float facingUpLimit = 5.0f; // 視点移動の上方向制限
		[Tooltip("下に向ける限界角度"), SerializeField]
		private float facingDownLimit = 45.0f;  // 視点移動の下方向制限
		private float limitDistance; // 注視点から離れられる限界距離
		private Quaternion newRotation; // 新しいカメラ角度
		private float _distance; // ターゲットまでの距離

		/// <summary>
		/// ターゲットまでの距離を返す
		/// </summary>
		public float distance
		{
			get { return _distance; }
			private set { _distance = value; }
		}

		void Start()
		{
			newRotation = transform.rotation;
			transform.LookAt(target);
			limitDistance = (target.position - cameraTransform.position).magnitude;
			distance = limitDistance;
		}

		void LateUpdate()
		{
			transform.rotation = newRotation;
			Player player = GameManager.instance.player;

			RaycastHit hit;
			Ray ray = new Ray(transform.position, -cameraTransform.forward);

			if (Physics.Raycast(ray, out hit, limitDistance))
			{
				distance = hit.distance;
			}
			else
			{
				distance = limitDistance;
			}
			Vector3 newPosition = target.position - cameraTransform.forward * distance;
			cameraTransform.position = Vector3.Lerp(cameraTransform.position, newPosition, 0.5f);
		}

		public void CameraMove(float vx, float vy)
		{
			if (vx == 0.0f && vy == 0.0f)
			{
				return;
			}
			Vector3 angles = newRotation.eulerAngles;

			angles.x += -vy;
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

			angles.y += vx;
			angles.z = 0.0f;
			newRotation.eulerAngles = angles;

		}
	}
}