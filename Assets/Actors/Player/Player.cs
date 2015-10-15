using UnityEngine;
using System.Collections.Generic;

//[RequireComponent(typeof(CharacterController))]

public class Player : MyMonoBehaviour
{
	[SerializeField]
	private float speed = 3.0f; // 移動速度
	private CharacterController characterController = null;

	private float jumpVY = 0.0f;
	private float jumpPower = 2f;
	private Vector3 moveVec = Vector3.zero;

	private Transform cameraTransform = null;   // プレイヤーカメラのトランスフォーム
	private float rotateSpeed = 1.5f;   // 視点回転の速度
	private float facingUpLimit = 60.0f; // 視点移動の上方向制限
	private float facingDownLimit = 70.0f;  // 視点移動の下方向制限
	//private Vector3 defaultCameraDirection = Vector3.zero;	 // 開始時の視点方向

	protected override void Awake()
	{
		base.Awake();
		characterController = GetComponent<CharacterController>();
		characterController.detectCollisions = false;
	}

	void Start()
	{
		cameraTransform = Camera.main.transform;
	}

	protected override void Update()
	{
		Cursor.lockState = CursorLockMode.Locked;

		Move(); // プレイヤーの移動など

		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");

		CameraMove(-mouseY, mouseX); // カメラの操作

	}

	/// <summary>
	/// 視点を移動する
	/// </summary>
	/// <param name="vx">X方向の移動量</param>
	/// <param name="vy">Y方向の移動量</param>
	private void CameraMove(float vx, float vy)
	{
		transform.Rotate(0.0f, vy * rotateSpeed, 0.0f);

		float cameraRotX = vx * rotateSpeed;
		cameraTransform.Rotate(cameraRotX, 0.0f, 0.0f);

		// カメラの見ている方向
		Vector3 direction = cameraTransform.forward;

		float fFront;
		// カメラの前方方向値
		Vector3 front = direction;
		front.y = 0;     // XZ平面での距離なのでYはいらない
		fFront = front.magnitude;

		// Y軸とXZ平面の前方方向との角度を求める
		float deg = Mathf.Atan2(-direction.y, fFront) * Mathf.Rad2Deg;

		// 可動範囲を制限
		if (deg > facingDownLimit)
		{
			cameraTransform.Rotate(-cameraRotX, 0.0f, 0.0f);
		}
		if (deg < -facingUpLimit)
		{
			cameraTransform.Rotate(-cameraRotX, 0.0f, 0.0f);
		}
	}

	void Move()
	{
		float dx = Input.GetAxisRaw("Horizontal");
		float dy = Input.GetAxisRaw("Vertical");

		moveVec = Vector3.zero; // 今回の移動量計算用
		if (dy != 0.0f || dx != 0.0f)
		{
			moveVec = transform.rotation * new Vector3(dx, 0.0f, dy).normalized;
		}

		if (!characterController.isGrounded)
		{
			// 空中では重力により落下速度を加算する
			jumpVY += Physics.gravity.y * Time.deltaTime;
		}
		else // 地面についている
		{
			// ジャンプさせる
			if (Input.GetButtonDown("Jump"))
			{
				jumpVY = jumpPower;
			}
			else
			{
				jumpVY = Physics.gravity.y * Time.deltaTime;
			}
		}

		moveVec.y += jumpVY;

		if (moveVec != Vector3.zero)
		{
			characterController.Move(moveVec * speed * Time.deltaTime);
		}
	}
}
