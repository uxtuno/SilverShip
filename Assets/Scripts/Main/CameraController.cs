using UnityEngine;
using System.Collections.Generic;

namespace Uxtuno
{
	/// <summary>
	/// カメラを制御するクラス
	/// 設計につぁE��:
	/// シングルトンクラス
	/// こ��EクラスはPlayerクラスが持ってぁE��
	/// Playerクラスからのみアクセス出来ると
	/// カメラは注視点を中忁E��して回転する
	/// 注視点はシーン上に褁E��存在してよい
	/// 場面に応じて注視点を��Eり替えることで柔軟なカメラが可能
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
		private Transform defaultTarget;
		[Tooltip("上に向ける限界角度"), SerializeField]
		private float facingUpLimit = 5.0f;                 // 視点移動の上方向制限
		[Tooltip("下に向ける限界角度"), SerializeField]
		private float facingDownLimit = 45.0f;              // 視点移動の下方向制限
		private float defaultDistance;
		private Quaternion _newRotation;// 注視点からカメラへの初期距離

		/// <summary>
		/// 新しいカメラ角度
		/// </summary>
		private Quaternion newRotation
		{
			get { return _newRotation; }
			set
			{
				_newRotation = value;
			}
		}
		private Quaternion oldRotation;                     // 補間開始時のカメラ角度
		private float _distance;                            // ターゲットまでの距離
		private float currentInterpolationSeconds = 0.2f;   // 補間時間
		private float currentInterpolationCount;            // 補間カウント

		/// <summary>
		/// 補間中かどうか
		/// </summary>
		public bool isInterpolation { get; private set; }

		/// <summary>
		/// 補間モード
		/// </summary>
		public enum InterpolationMode
		{
			// 線形補間
			Liner,
			// 曲線補間
			Curve,
		}
		private InterpolationMode currentInterpolationMode = InterpolationMode.Curve; // 現在の補間モード

		/// <summary>
		/// ターゲットまでの距離を返す
		/// </summary>
		public float targetToDistance
		{
			get { return _distance; }
			private set { _distance = value; }
		}

		private IList<Transform> overlappedObjects = new List<Transform>(); // カメラが接触したもの
		private float radius; // 障害物と一定距離を置くために使用

		private Vector3 oldPosition;
		private float currentPositionInterpolationSeconds = 0.5f;   // 補間時間
		private float currentPositionInterpolationCount;            // 補間カウント

		/// <summary>
		/// 座標補間中かどうか
		/// </summary>
		public bool isPositionInterpolation { get; private set; }

		void Start()
		{
			radius = GetComponentInChildren<SphereCollider>().radius;
			defaultDistance = (target.position - cameraTransform.position).magnitude;
			targetToDistance = defaultDistance;
			isInterpolation = false;

			cameraTransform.LookAt(target);
			cameraTransform.parent = target;
			oldRotation = target.rotation;
			newRotation = oldRotation;
			defaultTarget = target; // デフォルトのターゲット

			oldPosition = cameraTransform.position;
			SetTarget(target);
		}

		void LateUpdate()
		{
			if (!isInterpolation)
			{
				target.rotation = newRotation;
			}
			else
			{
				// 補間中の経過時間を0~1に正規化
				currentInterpolationCount += Time.deltaTime * (1 / currentInterpolationSeconds);
				// 補間位置
				float currentInterpolationPosition = currentInterpolationCount;
				if (currentInterpolationMode == InterpolationMode.Curve)
				{
					// 補間位置を計算。0~1をsin()によって滑らかに補間
					currentInterpolationPosition = Mathf.Sin((Mathf.PI * 0.5f) * currentInterpolationCount);
				}

				target.rotation = Quaternion.Lerp(oldRotation, newRotation, currentInterpolationPosition);

				// 補間終了
				if (currentInterpolationCount >= 1.0f)
				{
					isInterpolation = false;
				}
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

			currentPositionInterpolationCount += Time.deltaTime * (1 / currentInterpolationSeconds);
			if (currentPositionInterpolationCount > 1.0f)
			{
				oldPosition = cameraTransform.position;
				currentPositionInterpolationCount = 1.0f;
				isPositionInterpolation = false;
			}

			CameraPositionUpdate();
		}

		/// <summary>
		/// カメラの回転(補間なし)
		/// </summary>
		/// <param name="rotation"></param>
		public void CameraActualMove(float vx, float vy)
		{
			CameraMove(vx, vy, 0.0f);
			isInterpolation = false;
		}

		/// <summary>
		/// カメラを回転させる
		/// </summary>
		/// <param name="vx">水平方向角度</param>
		/// <param name="vy">垂直方向角度</param>
		/// <param name="mode">補間モード</param>
		public void CameraMove(float vx, float vy, float interpolationSeconds = 1.0f, InterpolationMode mode = InterpolationMode.Curve)
		{
			if (vx == 0.0f && vy == 0.0f)
			{
				return;
			}
			Vector3 angles = newRotation.eulerAngles;
			Quaternion nextRotation = Quaternion.identity;

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
			nextRotation.eulerAngles = angles;
			SetNextRotation(nextRotation, interpolationSeconds, mode);
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
			int index = overlappedObjects.IndexOf(other.transform);
			//int index = overlappedObjects.FindIndex((obj) => obj == other.transform);
			if (index >= 0)
			{
				overlappedObjects.RemoveAt(index);
			}
		}

		/// <summary>
		/// カメラの新しい方向を指定
		/// </summary>
		/// <param name="rotation">新しい方向</param>
		/// <param name="interpolationSeconds">補間時間(秒)</param>
		/// <param name="mode">補間モード</param>
		public void SetNextRotation(Quaternion rotation, float interpolationSeconds = 0.2f, InterpolationMode mode = InterpolationMode.Curve)
		{
			newRotation = rotation;
			InterpolationStart(interpolationSeconds, mode);
		}

		/// <summary>
		/// 補間開始時の初期化
		/// </summary>
		/// <param name="interpolationSeconds">補間時間(秒)</param>
		/// <param name="mode">補間モード</param>
		private void InterpolationStart(float interpolationSeconds = 0.2f, InterpolationMode mode = InterpolationMode.Curve)
		{
			oldRotation = transform.rotation;
			if (interpolationSeconds > 0.0f)
			{
				isInterpolation = true;
			}
			currentInterpolationSeconds = interpolationSeconds;
			currentInterpolationCount = 0.0f;
			currentInterpolationMode = mode;
		}

		/// <summary>
		/// 座標補間開始時の初期化
		/// </summary>
		/// <param name="interpolationSeconds">補間時間(秒)</param>
		/// <param name="mode">補間モード</param>
		private void PositionInterpolationStart(float interpolationSeconds = 0.2f, InterpolationMode mode = InterpolationMode.Curve)
		{
			oldRotation = transform.rotation;
			if (interpolationSeconds > 0.0f)
			{
				isPositionInterpolation = true;
			}
			currentPositionInterpolationSeconds = interpolationSeconds;
			currentPositionInterpolationCount = 0.0f;
			oldPosition = cameraTransform.position;
		}

		/// <summary>
		/// 新しい追尾対象を設定
		/// </summary>
		/// <param name="target"></param>
		public void SetTarget(Transform target)
		{
			oldRotation = this.target.rotation;
			this.target = target;
			PositionInterpolationStart(1.0f);
			cameraTransform.LookAt(target);
			target.forward = cameraTransform.forward;
			cameraTransform.parent = target;
			SetNextRotation(target.rotation, 0.2f);
		}

		public void ResetTarget()
		{
			oldRotation = this.target.rotation;
			this.target = defaultTarget;
			cameraTransform.parent = null;
			PositionInterpolationStart(1.0f);
			cameraTransform.LookAt(target);
			target.forward = cameraTransform.forward;
			cameraTransform.parent = target;
			SetNextRotation(target.rotation, 0.2f);
		}

		/// <summary>
		/// 中止点からターゲットまでの距離を設定
		/// </summary>
		/// <param name="distance"></param>
		public void SetDistance(float distance)
		{
			defaultDistance = distance;
		}

		/// <summary>
		/// カメラ座標更新
		/// </summary>
		public void CameraPositionUpdate()
		{
			Vector3 newPosition = target.position - cameraTransform.forward * targetToDistance;
			//cameraTransform.position = Vector3.Lerp(oldPosition, newPosition, currentPositionInterpolationCount); // TODO
			cameraTransform.position = newPosition;
		}
	}
}