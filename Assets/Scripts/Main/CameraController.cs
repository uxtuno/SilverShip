using UnityEngine;
using System.Collections;

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

		[SerializeField, Tooltip("追従対象")]
		private Transform _target = null;
		/// <summary>
		/// 追従対象
		/// </summary>
		public Transform target
		{
			get { return _target; }
			set { _target = value; }
		}

		[SerializeField, Tooltip("追従対象との距離")]
		private float _distance = 2.0f;

		/// <summary>
		/// カメラと注視点の距離
		/// </summary>
		public float distance
		{
			get { return _distance; }
			set { _distance = value; }
		}

		private float _defaultDistance;
		/// <summary>
		/// 注視点とカメラの距離のデフォルト値
		/// </summary>
		public float defaultDistance
		{
			get { return _defaultDistance; }
			private set { _defaultDistance = value; }
		}

		[SerializeField, Tooltip("追従速度")]
		private float movenSmoothing = 0.2f;
		[SerializeField, Tooltip("X軸回転の下方向最大角")]
		private float xAngleMin = 45.0f;
		[SerializeField, Tooltip("X軸回転の上方向最大角")]
		private float xAngleMax = 45.0f;
		[SerializeField, Tooltip("カメラ回転を滑らかにするための値")]
		private float turnSeconds = 0.2f;

		private static readonly float inverseToAngleX = -40.0f; // 上下の回転を反転させる境界角

		private float yAngle; // Y軸方向の回転角
		private float xAngle; // X軸方向の回転角

		private Transform _pivot; // 基準位置(X軸回転に使用)
		private Vector3 defaultPivotPosition; // 注視点の初期座標

		/// <summary>
		/// 注視点を返す
		/// </summary>
		public Transform pivot
		{
			get { return _pivot; }
			private set { _pivot = value; }
		}

		private Vector3 pivotEulers; // 基準位置のオイラー角を保持
		private Vector3 pivotTargetPosition;
		private Quaternion pivotTargetRotation; // 基準位置の回転後角度
		private Quaternion transformTargetRotation; // 回転後角度

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
			_pivot = cameraTransform.transform.parent;
			pivotTargetPosition = pivot.position;
			pivotTargetRotation = _pivot.localRotation;
			pivotEulers = _pivot.localEulerAngles;
			transformTargetRotation = transform.localRotation;
			cameraTransform.localPosition = -Vector3.forward * distance;
			defaultDistance = distance;
			defaultPivotPosition = pivot.localPosition;
		}

		/// <summary>
		/// 補間中の位置を0~1で返す
		/// </summary>
		/// <returns></returns>
		private float Interpolation()
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

			// 補間終了
			if (interpolationCount >= 1.0f)
			{
				interpolationCount = 1.0f;
				currentInterpolationPosition = 1.0f;
				isInterpolation = false;
				isForceInterpolation = false;
			}
			return currentInterpolationPosition;
		}

		/// <summary>
		/// カメラを回転させる
		/// </summary>
		/// <param name="horizontal">水平方向角度</param>
		/// <param name="vertical">垂直方向角度</param>
		/// <param name="mode">補間モード</param>
		public void CameraMove(float horizontal, float vertical, float interpolationSeconds = 1.0f, InterpolationMode mode = InterpolationMode.Curve)
		{
			HorizontalRotation(horizontal);
			VerticalRotation(vertical);
			InterpolationStart(interpolationSeconds, mode);
		}

		/// <summary>
		/// 補間開始時の初期化
		/// </summary>
		/// <param name="interpolationSeconds">補間時間(秒)</param>
		/// <param name="mode">補間モード</param>
		private void InterpolationStart(float interpolationSeconds = 0.2f, InterpolationMode mode = InterpolationMode.Curve)
		{
			if (interpolationSeconds > 0.0f)
			{
				isInterpolation = true;
			}
			this.interpolationSeconds = interpolationSeconds;
			interpolationCount = 0.0f;
			interpolationMode = mode;
		}

		/// <summary>
		/// カメラを水平方向に回転
		/// </summary>
		/// <param name="value"></param>
		public void HorizontalRotation(float value)
		{
			yAngle += value;
			yAngle = Mathf.Repeat(yAngle, 360.0f);
			transformTargetRotation = Quaternion.Euler(0.0f, yAngle, 0.0f);
		}

		/// <summary>
		/// カメラを垂直方向に回転
		/// </summary>
		/// <param name="value"></param>
		public void VerticalRotation(float value)
		{
			xAngle -= value;
			xAngle = Mathf.Clamp(xAngle, -xAngleMin, xAngleMax);
			pivotTargetRotation = Quaternion.Euler(xAngle, pivotEulers.y, pivotEulers.z);
		}

		void FixedUpdate()
		{
			// カメラの追従を行う
			StartCoroutine(TargetTracking());
		}

		private float oldDistance; // 前フレームのカメラ距離
		// このコードにより、すべてのFixedUpdate終了後に呼び出される
		IEnumerator TargetTracking()
		{
			yield return new WaitForFixedUpdate();
			// 座標を補間
			transform.position = Vector3.Lerp(transform.position, target.position, movenSmoothing);

			// 角度を補間
			float interpolationPosition = Interpolation();
			pivot.localPosition = Vector3.Lerp(pivot.localPosition, pivotTargetPosition, interpolationPosition);
			transform.localRotation = Quaternion.Lerp(transform.localRotation, transformTargetRotation, interpolationPosition);
			_pivot.localRotation = Quaternion.Lerp(_pivot.localRotation, pivotTargetRotation, interpolationPosition);

			// 最終的な距離
			float finalDistance = distance > defaultDistance ? distance : defaultDistance;
			cameraTransform.localPosition = -Vector3.forward * finalDistance;

			// 障害物を考慮して最終的なカメラの距離を計算
			RaycastHit hit;
			Ray ray = new Ray(transform.position, cameraTransform.position - transform.position);
			if (Physics.Raycast(ray, out hit, distance, LayerName.Obstacle.maskValue))
			{
				finalDistance = Vector3.Distance(hit.point, pivot.position);
			}
			oldDistance = Mathf.Lerp(oldDistance, finalDistance, interpolationPosition);
			// 追尾対象からの距離を反映
			cameraTransform.localPosition = -Vector3.forward * oldDistance;
		}

		#region - LookAt

		/// <summary>
		/// カメラで指定の方向を向く
		/// 注視点点から見た方向を指定
		/// </summary>
		/// <param name="direction">方向</param>
		/// <param name="interpolationSeconds">補間時間</param>
		/// <param name="mode">補間モード</param>
		public void LookDirection(Vector3 direction, float interpolationSeconds, InterpolationMode mode)
		{
			LookAt(transform.position + direction, interpolationSeconds, mode);
		}

		public void LookDirection(Vector3 direction)
		{
			LookAt(transform.position + direction, turnSeconds, InterpolationMode.Curve);
		}

		public void LookAt(Transform target)
		{
			if (target == null)
			{
				return;
			}
			LookAt(target.position, turnSeconds, InterpolationMode.Curve);
		}

		public void LookAt(Vector3 targetPosition)
		{
			LookAt(targetPosition, turnSeconds, InterpolationMode.Curve);
		}

		public void LookAt(Transform target, float interpolationSeconds, InterpolationMode mode)
		{
			if (target == null)
			{
				return;
			}
			LookAt(target.position, interpolationSeconds, mode);
		}

		/// <summary>
		/// 対象の方向へカメラを向ける
		/// </summary>
		/// <param name="targetPosition"></param>
		public void LookAt(Vector3 targetPosition, float interpolationSeconds, InterpolationMode mode)
		{
			// Y軸回転の計算なのでY軸座標を無視する
			Vector3 targetPositionXZ = Vector3.Scale(targetPosition, new Vector3(1.0f, 0.0f, 1.0f));
			Vector3 cameraRigPositionXZ = Vector3.Scale(transform.position, new Vector3(1.0f, 0.0f, 1.0f));
			Vector3 toTargetXZ = targetPositionXZ - cameraRigPositionXZ;
			float toTargetAngleY = Mathf.Atan2(toTargetXZ.x, toTargetXZ.z) * Mathf.Rad2Deg;

			// 角度を360度でループさせる
			toTargetAngleY = Mathf.Repeat(toTargetAngleY, 360);
			if (xAngle < 30.0f)
			{
				HorizontalRotation(-(yAngle - toTargetAngleY));
			}

			// XZ座標の距離と高さの差からX軸回転量を求める
			float toTargetDistanceXZ = toTargetXZ.magnitude;
			float toTargetHeightDiff = targetPosition.y - transform.position.y;
			float toTargetAngleX = Mathf.Atan2(toTargetDistanceXZ, toTargetHeightDiff) * Mathf.Rad2Deg - 90.0f;
			if (toTargetAngleX < inverseToAngleX)
			{
				//toTargetAngleX = Mathf.Abs(toTargetAngleX);
			}

			VerticalRotation((xAngle - toTargetAngleX));

			InterpolationStart(interpolationSeconds, mode);
		}
		#endregion

		/// <summary>
		/// カメラと注視点の距離を初期状態に戻す
		/// </summary>
		public void ResetDistance()
		{
			distance = defaultDistance;
		}

		/// <summary>
		/// 注視点を設定
		/// </summary>
		/// <param name="position"></param>
		public void SetPivot(Vector3 position)
		{
			pivotTargetPosition = transform.InverseTransformPoint(position);
		}

		/// <summary>
		/// 注視点の座標を初期状態に戻す
		/// </summary>
		public void ResetPivot()
		{
			pivotTargetPosition = defaultPivotPosition;
		}
	}
}