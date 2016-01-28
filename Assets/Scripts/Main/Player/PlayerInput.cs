using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 論理ボタン名
/// </summary>
public enum ButtonName
{
	None,
	Attack,
	Jump,
	Barrier,
	ItemGet,
	LockOn,
	CameraToFront,
	JumpTrampled,
	LeftLockOnChanged,
	RightLockOnChanged,
}

namespace Uxtuno
{
	/// <summary>
	/// プレイヤーの入力情報を管理するクラス
	/// Update()とFixedUpdate()のどちらで使用するかによって呼び出すメソッドを変更すること
	/// 通常のGetButtonDown()ではFixedUpdate()内で正しく動作しないためこのような対応が必要になる
	/// また、Inputクラスと同じように使用するためにstaticクラスとした
	/// </summary>
	public static class PlayerInput
	{
		private static readonly float lockOnChangeThreshold = 0.7f; // これ以上スティックが倒されたら入力したことにする
		private static int lockOnChangedInputFrameCount; // ロックオン対象変更入力フレーム(右スティック水平入力)

		private static Dictionary<ButtonName, bool> buttonState = new Dictionary<ButtonName, bool>(); // 各ボタンの状態

		static PlayerInput()
		{
			foreach (ButtonName button in Enum.GetValues(typeof(ButtonName)))
			{
				buttonState.Add(button, false);
			}
		}

		/// <summary>
		/// 初期化
		/// </summary>
		private static void Initialize()
		{
		}

		/// <summary>
		/// 水平方向の入力
		/// </summary>
		public static float horizontal { get; private set; }

		/// <summary>
		/// 垂直方向の入力
		/// </summary>
		public static float vertical { get; private set; }

		/// <summary>
		/// カメラの水平方向入力
		/// </summary>
		public static float cameraHorizontal { get; private set; }

		/// <summary>
		/// カメラの垂直方向入力
		/// </summary>
		public static float cameraVertical { get; private set; }

		/// <summary>
		/// 同時押し判定
		/// </summary>
		public static bool attackAndJump
		{
			get;
			private set;
		}

		private enum JumpTrampledInput
		{
			None = 0,
			Jump = 1,
			Attack = 2,
			All = Jump | Attack,
		}

		private static bool jumpTrampled; // 踏みつけジャンプ入力
		private static readonly float jumpTrampledInputSeconds = 0.15f; //　同時押し判定用 猶予時間
		private static int jumpTrampledInputFrame; //　同時押し継続フレーム数
		private static float jumpTrampledInputCount; //　同時押し判定用 猶予カウンタ
		private static JumpTrampledInput jumpTrampledInput = JumpTrampledInput.None;

		/// <summary>
		/// プレイヤー入力情報更新
		/// GameManagerが毎フレーム呼ぶこと
		/// </summary>
		public static void Update(float elapsedTime)
		{
			// プレイヤーの移動入力
			horizontal = Input.GetAxisRaw(InputName.Horizontal);
			vertical = Input.GetAxisRaw(InputName.Vertical);

			// カメラ回転入力(-1 ~ 1)
			cameraHorizontal = Input.GetAxisRaw(InputName.CameraX);
			cameraVertical = Input.GetAxisRaw(InputName.CameraY);

			// 微小な値を無視
			if (Mathf.Abs(cameraHorizontal) < 0.2f)
				cameraHorizontal = 0.0f;
			if (Mathf.Abs(cameraVertical) < 0.2f)
				cameraVertical = 0.0f;

			// 画面クリック時のカメラ回転
			if (Input.GetMouseButton(1))
			{
				Vector3 rotationInput = Camera.main.ScreenToViewportPoint(Input.mousePosition);
				// 中央を(0, 0)にする
				rotationInput.x -= 0.5f;
				rotationInput.y -= 0.5f;
				rotationInput *= 2.0f;
				if (Mathf.Abs(rotationInput.y) < 0.5f)
				{
					rotationInput.y = 0.0f;
				}

				cameraHorizontal = rotationInput.x;
				cameraVertical = rotationInput.y;
			}

			// コントローラでは同時押しだがキーボードでは辛いのでキーボード用の設定
			buttonState[ButtonName.JumpTrampled] = Input.GetButtonDown(InputName.JumpAndAttack) ? true : buttonState[ButtonName.JumpTrampled];

			buttonState[ButtonName.CameraToFront] = Input.GetButtonDown(InputName.CameraToFront) ? true : buttonState[ButtonName.CameraToFront];
			buttonState[ButtonName.ItemGet] = Input.GetButtonDown(InputName.ItemGet) ? true : buttonState[ButtonName.ItemGet];
			buttonState[ButtonName.Barrier] = Input.GetButtonDown(InputName.Barrier) ? true : buttonState[ButtonName.Barrier];

			if (Input.GetAxis(InputName.RightStickHorizontal) < -lockOnChangeThreshold)
			{
				if (!buttonState[ButtonName.LeftLockOnChanged]
					&& lockOnChangedInputFrameCount == 0)
				{
					buttonState[ButtonName.LeftLockOnChanged] = true;
				}
				++lockOnChangedInputFrameCount;
			}
			else if (Input.GetAxis(InputName.RightStickHorizontal) > lockOnChangeThreshold)
			{
				if (!buttonState[ButtonName.RightLockOnChanged]
					&& lockOnChangedInputFrameCount == 0)
				{
					buttonState[ButtonName.RightLockOnChanged] = true;
				}
				++lockOnChangedInputFrameCount;
			}
			else
			{
				lockOnChangedInputFrameCount = 0;
			}

			buttonState[ButtonName.Jump] = Input.GetButtonDown(InputName.Jump) ? true : buttonState[ButtonName.Jump];
			buttonState[ButtonName.Attack] = Input.GetButtonDown(InputName.Attack) ? true : buttonState[ButtonName.Attack];
			if (buttonState[ButtonName.Jump])
			{
				jumpTrampledInput |= JumpTrampledInput.Jump;
			}

			if (buttonState[ButtonName.Attack])
			{
				jumpTrampledInput |= JumpTrampledInput.Attack;
			}

			if (jumpTrampledInput == JumpTrampledInput.None)
			{
				jumpTrampledInputCount = 0.0f;
			}
			else
			{
				// 何らかのボタンが押されていたので同時押し判定用のカウンタを増加させる
				jumpTrampledInputCount += elapsedTime;
			}

			// 時間内に同時押しが成立
			if (jumpTrampledInput == JumpTrampledInput.All &&
				jumpTrampledInputCount < jumpTrampledInputSeconds)
			{
				// 同時押し判定が複数回発生しないように入力フレームをカウント
				++jumpTrampledInputFrame;
			}
			else
			{
				jumpTrampledInputFrame = 0;
			}

			// 同時押しの瞬間にtrue
			if (jumpTrampledInputFrame == 1)
			{
				jumpTrampled = true;
				buttonState[ButtonName.JumpTrampled] = true;
				// 同時押しが成立した瞬間それぞれのボタン入力は無効
				buttonState[ButtonName.Attack] = false;
				buttonState[ButtonName.Jump] = false;
			}
			else
			{
				jumpTrampled = false;
			}

			buttonState[ButtonName.LockOn] = Input.GetButtonDown(InputName.LockOn) ? true : buttonState[ButtonName.LockOn];
		}

		/// <summary>
		/// 入力更新
		/// GameManagerがFixedUpdate()内でこのメソッドをStartCoroutine()で呼び出す
		/// </summary>
		public static IEnumerator LateFixedUpdate()
		{
			yield return new WaitForFixedUpdate();
			// 同時押し判定後のタイミングでリセット
			if (jumpTrampledInputCount >= jumpTrampledInputSeconds ||
				buttonState[ButtonName.JumpTrampled])
			{
				buttonState[ButtonName.Attack] = false;
				buttonState[ButtonName.Jump] = false;
				jumpTrampledInputCount = 0.0f;
				jumpTrampledInput = JumpTrampledInput.None;
			}

			// Attackと Jumpについては同時押しに使用するためfalseに戻すタイミングを遅延させる
			//buttonState[ButtonName.Attack] = false;
			buttonState[ButtonName.Barrier] = false;
			buttonState[ButtonName.ItemGet] = false;
			//buttonState[ButtonName.Jump] = false;
			buttonState[ButtonName.JumpTrampled] = false;
			buttonState[ButtonName.LockOn] = false;
			buttonState[ButtonName.CameraToFront] = false;
			if (lockOnChangedInputFrameCount >= 1)
			{
				buttonState[ButtonName.RightLockOnChanged] = false;
				buttonState[ButtonName.LeftLockOnChanged] = false;
			}
		}

		/// <summary>
		/// 指定のボタンが押された瞬間、trueを返す
		/// 注意 : FixedUpdate()内でのみ呼ぶこと
		/// </summary>
		/// <param name="buttonName">検知したいボタン</param>
		/// <returns></returns>
		public static bool GetButtonDownInFixedUpdate(ButtonName buttonName)
		{
			switch (buttonName)
			{
				case ButtonName.Attack:
				case ButtonName.Jump:
					// 同時入力判定中、または入力されていないときにfalseを返す
					if (jumpTrampledInputCount < jumpTrampledInputSeconds)
					{

						return false;
					}

					return buttonState[buttonName];

				case ButtonName.Barrier:
				case ButtonName.ItemGet:
				case ButtonName.LockOn:
				case ButtonName.CameraToFront:
				case ButtonName.JumpTrampled:
				case ButtonName.RightLockOnChanged:
				case ButtonName.LeftLockOnChanged:
					return buttonState[buttonName];
				default:
					throw new ArgumentException("不正な引数が渡されました");
			}
		}

		/// <summary>
		/// 指定のボタンが押された瞬間、trueを返す
		/// 注意 : Update()内でのみ呼ぶこと
		/// </summary>
		/// <param name="buttonName">検知したいボタン</param>
		/// <returns></returns>
		public static bool GetButtonDown(ButtonName buttonName)
		{
			switch (buttonName)
			{
				case ButtonName.Attack:
					// 同時入力判定中、または入力されていないときにfalseを返す
					if (jumpTrampledInputCount < jumpTrampledInputSeconds)
					{
						return false;
					}
					return Input.GetButtonDown(InputName.Attack);

				case ButtonName.Jump:
					// 同時入力判定中、または入力されていないときにfalseを返す
					if (jumpTrampledInputCount < jumpTrampledInputSeconds)
					{
						return false;
					}
					return Input.GetButtonDown(InputName.Jump);

				case ButtonName.Barrier:
					return Input.GetButtonDown(InputName.Barrier);
				case ButtonName.ItemGet:
					return Input.GetButtonDown(InputName.ItemGet);
				case ButtonName.LockOn:
					return Input.GetButtonDown(InputName.LockOn);
				case ButtonName.CameraToFront:
					return Input.GetButtonDown(InputName.CameraToFront);
				case ButtonName.JumpTrampled:
					return jumpTrampled;
				default:
					throw new ArgumentException("不正な引数が渡されました");
			}
		}
	}
}