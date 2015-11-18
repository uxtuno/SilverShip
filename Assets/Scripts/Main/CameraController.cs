using UnityEngine;
using System.Collections.Generic;

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

		/// <summary>
		/// カメラの実際の回転角度を返す
		/// </summary>
		public Quaternion actualRotation { get { return newRotation; } }

		[Tooltip("カメラで追いかける対象"), SerializeField]
		private Transform target = null;
		[Tooltip("上に向ける限界角度"), SerializeField]
		private float facingUpLimit = 5.0f; // 視点移動の上方向制限
		[Tooltip("下に向ける限界角度"), SerializeField]
		private float facingDownLimit = 45.0f;  // 視点移動の下方向制限
		private float defaultDistance; // 注視点からカメラへの初期距離
		private Quaternion newRotation; // 新しいカメラ角度
		private float _distance; // ターゲットまでの距離

		/// <summary>
		/// ターゲットまでの距離を返す
		/// </summary>
		public float targetToDistance
		{
			get { return _distance; }
			private set { _distance = value; }
		}

		private List<Transform> overlappedObjects = new List<Transform>(); // カメラが接触したもの
		private float radius; // 障害物と一定距離を置くために使用
		private bool isSetRotation = false;

		void Start()
		{
			radius = GetComponentInChildren<SphereCollider>().radius;
			newRotation = transform.rotation;
			defaultDistance = (target.position - cameraTransform.position).magnitude;
			targetToDistance = defaultDistance;
			transform.LookAt(target);
			transform.rotation = newRotation;
		}

		void LateUpdate()
		{
			if (isSetRotation)
			{
				isSetRotation = false;
				transform.rotation = newRotation;
			}
			else
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, 0.4f); // TODO:
			}
				//cameraTransform.LookAt(target);
			// 壁にぶつかっている時だけ処理を行う
			if (overlappedObjects != null)
			{
				RaycastHit hit;
				LayerMask mask = LayerName.Wall.maskValue;
				Ray ray = new Ray(target.position, -cameraTransform.forward);

				targetToDistance = defaultDistance;
				if (Physics.Raycast(ray, out hit, defaultDistance, mask))
				{
					foreach (Transform overlap in overlappedObjects)
					{
						if (hit.transform == overlap)
						{
							// 壁にぶつかったのでカメラの位置を壁の手前まで近づける
							targetToDistance = hit.distance;
						}
					}
				}
			}
			Vector3 newPosition = target.position - cameraTransform.forward * targetToDistance;
			cameraTransform.position = Vector3.Lerp(cameraTransform.position, newPosition, 0.4f); // TODO
		}

		/// <summary>
		/// カメラの回転(補間なし)
		/// </summary>
		/// <param name="rotation"></param>
		public void CameraActualMove(float vx, float vy)
		{
			CameraMove(vx, vy);
			isSetRotation = true;
		}

		/// <summary>
		/// カメラを回転させる
		/// </summary>
		/// <param name="vx">水平方向角度</param>
		/// <param name="vy">垂直方向角度</param>
		public void CameraMove(float vx, float vy)
		{
			if (vx == 0.0f && vy == 0.0f)
			{
				return;
			}
			Vector3 angles = newRotation.eulerAngles;

			angles.x -= vy;
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

		void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.layer == LayerName.Wall.id)
			{
				overlappedObjects.Add(other.transform);
			}
		}

		void OnTriggerExit(Collider other)
		{
			int index = overlappedObjects.FindIndex((obj) => obj == other.transform);
			if (index >= 0)
			{
				overlappedObjects.RemoveAt(index);
			}
		}

		/// <summary>
		/// カメラの新しい方向を指定
		/// </summary>
		/// <param name="rotation"></param>
		public void SetRotation(Quaternion rotation)
		{
			newRotation = rotation;
		}
		
		/// <summary>
		/// 新しい追尾対象を設定
		/// </summary>
		/// <param name="target"></param>
		public void SetTarget(Transform target)
		{
			this.target = target;
			if(target != null)
			{
			}
		}

		/// <summary>
		/// 中止点からターゲットまでの距離を設定
		/// </summary>
		/// <param name="distance"></param>
		public void SetDistance(float distance)
		{
			defaultDistance = distance;
		}
	}
}