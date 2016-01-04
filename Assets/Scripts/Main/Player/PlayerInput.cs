using UnityEngine;
using System.Collections;

// 似たような処理の塊になっている、できるならループなどで一括で処理したい
public class PlayerInput
{
	private PlayerInput() { }
	private static PlayerInput _instance; // 唯一のインスタンス

	/// <summary>
	/// 唯一のインスタンスを返す
	/// </summary>
	public static PlayerInput instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new PlayerInput();
			}

			return _instance;
		}
	}

	/// <summary>
	/// 水平方向の入力
	/// </summary>
	public float horizontal { get; private set; }

	/// <summary>
	/// 垂直方向の入力
	/// </summary>
	public float vertical { get; private set; }

	/// <summary>
	/// カメラの水平方向入力
	/// </summary>
	public float cameraHorizontal { get; private set; }

	/// <summary>
	/// カメラの垂直方向入力
	/// </summary>
	public float cameraVertical { get; private set; }

	#region - ここの処理はFixedUpdate内でも問題なく押した瞬間を検知するためにこのような回りくどい実装になっている
	private bool _attack;
	/// <summary>
	/// 攻撃ボタン
	/// </summary>
	public bool attack
	{
		get
		{
			if (_attack)
			{
				_attack = Input.GetButtonDown(InputName.Atack);
				return false;
			}
			_attack = Input.GetButtonDown(InputName.Atack);
			return _attack;
		}
	}

	private bool _jump;
	/// <summary>
	/// ジャンプボタン
	/// </summary>
	public bool jump
	{
		get
		{
			if (_jump)
			{
				_jump = Input.GetButtonDown(InputName.Jump);
				return false;
			}
			_jump = Input.GetButtonDown(InputName.Jump);
			return _jump;
		}
	}

	private bool _barrier;
	/// <summary>
	/// 結界発動ボタン
	/// </summary>
	public bool barrier
	{
		get
		{
			if (_barrier)
			{
				_barrier = Input.GetButtonDown(InputName.Barrier);
				return false;
			}
			_barrier = Input.GetButtonDown(InputName.Barrier);
			return _barrier;
		}
	}

	private bool _itemGet;
	/// <summary>
	/// アイテム入手ボタン
	/// </summary>
	public bool itemGet
	{
		get
		{
			if (_itemGet)
			{
				_itemGet = Input.GetButtonDown(InputName.ItemGet);
				return false;
			}
			_itemGet = Input.GetButtonDown(InputName.ItemGet);
			return _itemGet;
		}
	}

	private bool _cameraToFront;
	/// <summary>
	/// カメラを前方に向ける
	/// </summary>
	public bool cameraToFront
	{
		get
		{
			if (_cameraToFront)
			{
				_cameraToFront = Input.GetButtonDown(InputName.CameraToFront);
				return false;
			}
			_cameraToFront = Input.GetButtonDown(InputName.CameraToFront);
			return _cameraToFront;
		}
	}

	private bool _lockOn;
	/// <summary>
	/// ロックオン
	/// </summary>
	public bool lockOn
	{
		get
		{
			if (_lockOn)
			{
				_lockOn = Input.GetButtonDown(InputName.LockOn);
				return false;
			}
			_lockOn = Input.GetButtonDown(InputName.LockOn);
			return _lockOn;
		}
	}
	#endregion

	private enum DoubleButtonInput
	{
		None,
		OneButton,
		TwoButton,
		DoubleInput = OneButton | TwoButton,
	}

	DoubleButtonInput attackAndJumpInput; // 攻撃ボタンとジャンプボタンの同時押し判定用
	private static readonly float inputGraceSeconds = 0.2f; // 同時入力猶予時間
	private float inputGraceCount; // 入力猶予時間カウント用

	/// <summary>
	/// 同時押し判定
	/// </summary>
    public bool attackAndJump
	{
		get; private set;
	}

	/// <summary>
	/// プレイヤー入力情報更新
	/// GameManagerが毎フレーム呼ぶこと
	/// </summary>
	public void Update(float elapsedTime)
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

		DoubleButtonInput oldattackAndJumpInput = attackAndJumpInput;
		if(Input.GetButtonDown(InputName.Atack))
		{
			attackAndJumpInput |= DoubleButtonInput.OneButton;
		}

		if (Input.GetButtonDown(InputName.Jump))
		{
			attackAndJumpInput |= DoubleButtonInput.TwoButton;
		}

		if(attackAndJumpInput != DoubleButtonInput.None)
		{
			if(inputGraceCount < inputGraceSeconds)
			{
				if (attackAndJumpInput == DoubleButtonInput.DoubleInput)
				{
					attackAndJump = true;
				}
			}
			else
			{
				// 猶予時間を超えているので入力状態をリセット
				attackAndJumpInput = DoubleButtonInput.None;
				inputGraceCount = 0.0f;
				attackAndJump = false;
			}
			inputGraceCount += Time.deltaTime;
		}
	}
}
