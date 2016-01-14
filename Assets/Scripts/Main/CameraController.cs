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
		private Transform target = null;
		[SerializeField, Tooltip("追従対象との距離")]
		private float distance = 2.0f;

		[SerializeField, Tooltip("追従速度")]
		private float movenSmoothing = 0.2f;
		[SerializeField, Tooltip("X軸回転の下方向最大角")]
		private float xAngleMin = 45.0f;
		[SerializeField, Tooltip("X軸回転の上方向最大角")]
		private float xAngleMax = 45.0f;
		[SerializeField, Tooltip("カメラ回転を滑らかにするための値")]
		private float turnSeconds = 0.2f;

		private float yAngle; // Y軸方向の回転角
		private float xAngle; // X軸方向の回転角

		private Transform pivot; // 基準位置(X軸回転に使用)
		private Vector3 pivotEulers; // 基準位置のオイラー角を保持
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
			pivot = cameraTransform.transform.parent;
			pivotTargetRotation = pivot.localRotation;
			pivotEulers = pivot.localEulerAngles;
			transformTargetRotation = transform.localRotation;
			cameraTransform.localPosition = -Vector3.forward * distance;
		}

		void LateUpdate()
		{
			//if (isInterpolation)
			//{
			//	Interpolation();
			//}
			//else
			//{
			//	pivot.rotation = newRotation;
			//}

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
		/// カメラが向く方向を指定
		/// このメソッドによる補間が完了するまではCameraMove()は無効
		/// </summary>
		/// <param name="rotation">クオータニオン</param>
		/// <param name="interpolationSeconds">補間時間</param>
		/// <param name="mode">補間モード</param>
		public void SetRotation(Quaternion rotation, float interpolationSeconds, InterpolationMode mode)
		{
			//Vector3 angles = rotation.eulerAngles;
			//if (angles.x > 180.0f)
			//{
			//	angles.x -= 360.0f;
			//}

			//// 上方制限
			//if (angles.x < -facingUpLimit)
			//{
			//	angles.x = -facingUpLimit;
			//}

			//if (angles.x > facingDownLimit)
			//{
			//	angles.x = facingDownLimit;
			//}

			//angles.z = 0.0f;

			//newRotation.eulerAngles = angles;
			//isForceInterpolation = true;
			//InterpolationStart(interpolationSeconds, mode);
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
		/// 回転の中心点を変更
		/// </summary>
		public void SetPovot(Vector3 position)
		{
			//// カメラに位置を反映させないように一度親子関係を解除
			//Vector3 vec = cameraTransform.position - position;
			//Vector3 cameraPosition = cameraTransform.position;
			//cameraTransform.SetParent(null, false);
			//pivot.position = position;
			//float distance = vec.magnitude;
			//         pivot.LookAt(position - cameraPosition);
			//newRotation = pivot.rotation;
			//cameraTransform.SetParent(pivot, false);
			//cameraTransform.position = pivot.position + vec;
			//cameraTransform.SetParent(null);
			//pivot.position = position;
			//cameraTransform.SetParent(pivot);
		}

		/// <summary>
		/// カメラの中心点との位置関係を初期状態に戻す
		/// </summary>
		public void DefaultLocalCameraPosition()
		{
			//cameraTransform.localPosition = defaultLocalCameraPosition;
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

		void Update()
		{
			// 追尾対象からの距離を反映
			cameraTransform.localPosition = -Vector3.forward * distance;

		}

		void FixedUpdate()
		{
			// カメラの追従を行う
			StartCoroutine(TargetTracking());
		}

		// このコードにより、すべてのFixedUpdate終了後に呼び出される
		IEnumerator TargetTracking()
		{
			yield return new WaitForFixedUpdate();
			transform.position = Vector3.Lerp(transform.position, target.position, movenSmoothing);

			float interpolationPosition = Interpolation();
			transform.localRotation = Quaternion.Lerp(transform.localRotation, transformTargetRotation, interpolationPosition);
			pivot.localRotation = Quaternion.Lerp(pivot.localRotation, pivotTargetRotation, interpolationPosition);
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
			HorizontalRotation(-(yAngle - toTargetAngleY));

			// XZ座標の距離と高さの差からX軸回転量を求める
			float toTargetDistanceXZ = toTargetXZ.magnitude;
			float toTargetHeightDiff = targetPosition.y - transform.position.y;
			float toTargetAngleX = Mathf.Atan2(toTargetDistanceXZ, toTargetHeightDiff) * Mathf.Rad2Deg - 90.0f;
			toTargetAngleX = Mathf.Abs(toTargetAngleX);
			VerticalRotation((xAngle - toTargetAngleX));

			InterpolationStart(interpolationSeconds, mode);
		}
		#endregion
	}
}