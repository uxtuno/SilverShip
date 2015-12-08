using UnityEngine;
using System.Collections.Generic;

namespace Uxtuno
{
	/// <summary>
	/// カメラを制御するクラス
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
		private Transform defaultTarget;
		[Tooltip("上に向ける限界角度"), SerializeField]
		private float facingUpLimit = 5.0f;                 // 視点移動の上方向制限
		[Tooltip("下に向ける限界角度"), SerializeField]
		private float facingDownLimit = 45.0f;              // 視点移動の下方向制限

		private Quaternion oldRotation; // 補間前の角度
		private Quaternion newRotation; // 新しい角度

		// 補間
		public enum InterpolationMode
		{
			None,
			Linner,
			Curve,
		}

		private InterpolationMode interpolationMode; // 補間モード
		private float interpolationCount; // 補間カウンタ
		private float interpolationSeconds; // 補間時間

		/// <summary>
		/// 強制補間中か(補間が完了するまでCameraMove()を無効化)
		/// </summary>
		public bool isForceInterpolation { get; set; }
		/// <summary>
		/// 補間中か
		/// </summary>
		public bool isInterpolation { get; set; }

		void Start()
		{
		}

		void LateUpdate()
		{
			if (isInterpolation)
			{
				Interpolation();
			}
			else
			{
				target.rotation = newRotation;
			}

		}

		/// <summary>
		/// 補間
		/// </summary>
		private void Interpolation()
		{
			// 補間中の経過時間を0~1に正規化
			interpolationCount += Time.deltaTime * (1 / interpolationSeconds);
			// 補間位置
			float currentInterpolationPosition = interpolationCount;
			if (interpolationMode == InterpolationMode.Curve)
			{
				// 補間位置を計算。0~1をsin()によって滑らかに補間
				currentInterpolationPosition = Mathf.Sin((Mathf.PI * 0.5f) * interpolationCount);
			}

			target.rotation = Quaternion.Lerp(oldRotation, newRotation, currentInterpolationPosition);

			// 補間終了
			if (interpolationCount >= 1.0f)
			{
				isInterpolation = false;
				isForceInterpolation = false;
			}
		}

		/// <summary>
		/// カメラを回転させる
		/// </summary>
		/// <param name="horizontal">水平方向角度</param>
		/// <param name="vertical">垂直方向角度</param>
		/// <param name="mode">補間モード</param>
		public void CameraMove(float horizontal, float vertical, float interpolationSeconds = 1.0f, InterpolationMode mode = InterpolationMode.Curve)
		{
			if (horizontal == 0.0f && vertical == 0.0f ||
				isForceInterpolation)
			{
				return;
			}
			Vector3 angles = newRotation.eulerAngles;

			angles.x -= vertical;
			if (angles.x > 180.0f)
			{
				angles.x -= 360.0f;
			}

			// 上方制限
			if (vertical > 0.0f)
			{
				if (angles.x < -facingUpLimit)
				{
					angles.x = -facingUpLimit;
				}
			}

			// 下方制限
			if (vertical < 0.0f)
			{
				if (angles.x > facingDownLimit)
				{
					angles.x = facingDownLimit;
				}
			}

			angles.y += horizontal;
			angles.z = 0.0f;
			newRotation.eulerAngles = angles;
		}

		/// <summary>
		/// カメラが向く方向を指定
		/// このメソッドによる補間が完了するまではCameraMove()は無効
		/// </summary>
		/// <param name="rotation">クオータニオン</param>
		/// <param name="interpolationSeconds">補間時間</param>
		/// <param name="mode">補間モード</param>
		public void SetRotation(Quaternion rotation, float interpolationSeconds, InterpolationMode mode)
		{
			Vector3 angles = rotation.eulerAngles;
			if (angles.x > 180.0f)
			{
				angles.x -= 360.0f;
			}

			// 上方制限
			if (angles.x < -facingUpLimit)
			{
				angles.x = -facingUpLimit;
			}

			if (angles.x > facingDownLimit)
			{
				angles.x = facingDownLimit;
			}

			newRotation.eulerAngles = angles;
			isForceInterpolation = true;
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
			this.interpolationSeconds = interpolationSeconds;
			interpolationCount = 0.0f;
			interpolationMode = mode;
		}

		/// <summary>
		/// 回転の中心点を変更
		/// </summary>
		public void SetPovot(Vector3 position)
		{
			// カメラに位置を反映させないように一度親子関係を解除
			cameraTransform.SetParent(null);
			target.position = position;
			target.LookAt(cameraTransform);
			cameraTransform.SetParent(target);
		}
	}
}