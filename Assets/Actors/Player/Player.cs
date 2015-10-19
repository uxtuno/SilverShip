using UnityEngine;
using System.Collections.Generic;

//[RequireComponent(typeof(CharacterController))]

public class Player : MyMonoBehaviour
{
	[SerializeField]
	private float speed = 1.0f; // 移動速度
	private float highSpeed = 5.0f; // 移動速度(ダッシュ時)
	private CharacterController characterController = null;

	private float jumpVY = 0.0f;
	private float jumpPower = 7.0f;
	private Vector3 moveVec = Vector3.zero;

	private Transform cameraTransform = null;   // プレイヤーカメラのトランスフォーム
	private float rotateSpeed = 1.5f;   // 視点回転の速度
	private float facingUpLimit = 60.0f; // 視点移動の上方向制限
	private float facingDownLimit = 70.0f;  // 視点移動の下方向制限
											//private Vector3 defaultCameraDirection = Vector3.zero;	 // 開始時の視点方向

	private Animator animator;
	private int speedId;
	private int isJumpId;

	protected override void Awake()
	{
		base.Awake();
		characterController = GetComponent<CharacterController>();
		characterController.detectCollisions = false;
		animator = GetComponentInChildren<Animator>(); // アニメーションをコントロールするためのAnimatorを子から取得
		speedId = Animator.StringToHash("Speed"); // ハッシュIDを取得しておく
		isJumpId = Animator.StringToHash("IsJump"); // ハッシュIDを取得しておく
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
		Vector3 localMoveVector = Vector3.zero;
		if (dy != 0.0f || dx != 0.0f)
		{
			Vector3 rotateAngle = Vector3.zero;
			rotateAngle.y = Mathf.Atan2(dx, dy) * Mathf.Rad2Deg;
			transform.eulerAngles = rotateAngle;

			if (Input.GetKey(KeyCode.LeftShift))
			{
				localMoveVector = transform.forward * highSpeed;
				animator.SetFloat(speedId, highSpeed);
			}
			else
			{
				localMoveVector = transform.forward * speed;
				animator.SetFloat(speedId, speed);
			}
		}
		else
		{
			animator.SetFloat(speedId, 0.0f); // 待機アニメーション
		}

		if (!characterController.isGrounded)
		{
			// 空中では重力により落下速度を加算する
			jumpVY += Physics.gravity.y * Time.deltaTime;
		}
		else // 地面についている
		{
			animator.SetBool(isJumpId, false);
			// ジャンプさせる
			if (Input.GetButtonDown("Jump"))
			{
				jumpVY = jumpPower;
				animator.SetBool(isJumpId, true);
			}
			else
			{
				jumpVY = Physics.gravity.y * Time.deltaTime;
			}
		}

		moveVec.y += jumpVY;

		if (moveVec != Vector3.zero)
		{
			localMoveVector.y = jumpVY;
			characterController.Move(localMoveVector * Time.deltaTime);
		}
	}
}
