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

	private Transform cameraTransform = null;   // プレイヤーカメラのトランスフォーム
	private float rotateSpeed = 1.5f;   // 視点回転の速度
	private float facingUpLimit = 60.0f; // 視点移動の上方向制限
	private float facingDownLimit = 70.0f;  // 視点移動の下方向制限

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

		//CameraMove(-mouseY, mouseX); // カメラの操作

	}

	void Move()
	{
		Vector3 direction = Vector3.zero;
		// directionは進行方向を表すので上下入力はzに格納
		direction.x = Input.GetAxisRaw("Horizontal");
		direction.z = Input.GetAxisRaw("Vertical");

		Vector3 moveVector = Vector3.zero;
		if (direction != Vector3.zero)
		{
			Vector3 rotateAngles = Vector3.zero;

			// プレイヤーを進行方向に向ける処理
			//direction = cameraTransform.rotation * direction.normalized;
			direction = cameraTransform.rotation * direction.normalized;
			//direction = direction.normalized;
			rotateAngles.y = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg; // xz平面の進行方向から、Y軸回転角を得る
			transform.eulerAngles = rotateAngles;

			if (Input.GetKey(KeyCode.LeftShift))
			{
				moveVector = transform.forward * highSpeed;
				animator.SetFloat(speedId, highSpeed);
			}
			else
			{
				moveVector = transform.forward * speed;
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
			// ジャンプさせる
			if (Input.GetButtonDown("Jump") && !animator.GetBool(isJumpId))
			{
				jumpVY = jumpPower;
				animator.SetBool(isJumpId, true);
			}
			else
			{
				animator.SetBool(isJumpId, false);
				jumpVY = Physics.gravity.y;
			}
		}

		moveVector.y = jumpVY;
		characterController.Move(moveVector * Time.deltaTime);
	}
}
