using UnityEngine;
using System.Collections;

public class TPSCamera : MonoBehaviour
{
	[SerializeField, Tooltip("追従対象")]
	private Transform target = null;
	[SerializeField, Tooltip("追従対象との距離")]
	private float distance = 2.0f;

	[SerializeField, Tooltip("垂直方向の回転速度")]
	private float verticalTurnSpeed;
	[SerializeField, Tooltip("水平方向の回転速度")]
	private float horizontalTurnSpeed;
	[SerializeField, Tooltip("追従速度")]
	private float moveSpeed = 2.0f;
	[SerializeField, Tooltip("X軸回転の下方向最大角")]
	private float xAngleMin = 45.0f;
	[SerializeField, Tooltip("X軸回転の上方向最大角")]
	private float xAngleMax = 45.0f;
	[SerializeField, Tooltip("カメラ回転を滑らかにするための値")]
	private float turnSmoothing = 10.0f;

	private float yAngle; // Y軸方向の回転角
	private float xAngle; // X軸方向の回転角

	private Transform pivot;
	private Vector3 pivotEulers;
	private Quaternion pivotTargetRotation;
	private Quaternion transformTargetRotation;

	/// <summary>
	/// 操作するカメラ
	/// </summary>
	public Transform cameraTransform { get; private set; }

	void Start()
	{
		cameraTransform = GetComponentInChildren<Camera>().transform;
		pivot = cameraTransform.transform.parent;
		pivotTargetRotation = pivot.localRotation;
		pivotEulers = pivot.localEulerAngles;
		transformTargetRotation = transform.localRotation;
	}

	void Update()
	{
		CameraControl();
	}

	/// <summary>
	/// プレイヤーによるカメラ入力
	/// </summary>
	private void CameraControl()
	{
		float x = Input.GetAxis("Mouse X");
		float y = Input.GetAxis("Mouse Y");

		HorizontalRotation(x * horizontalTurnSpeed);
		VerticalRotation(y * verticalTurnSpeed);
		transform.localRotation = Quaternion.Lerp(transform.localRotation, transformTargetRotation, turnSmoothing);
		pivot.localRotation = Quaternion.Lerp(pivot.localRotation, pivotTargetRotation, turnSmoothing);
	}

	/// <summary>
	/// カメラを水平方向に回転
	/// </summary>
	/// <param name="value"></param>
	public void HorizontalRotation(float value)
	{
		yAngle += value;
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

	IEnumerator TargetTracking()
	{
		yield return new WaitForFixedUpdate();
		transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * moveSpeed);
	}
}
