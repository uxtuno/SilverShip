using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 似たような処理の塊になっている、できるならループなどで一括で処理したい

/// <summary>
/// 押した瞬間を返すプロパティは取得した段階でfalseとなる
/// そのため値を使いまわしたい場合は変数に代入して使用すること
/// UpdateでもFixedUpdateでも利用できるように、このような仕様になっている
/// FixedUpdateが呼ばれるまえにUpdateが複数回呼ばれた場合
/// 通常、FixedUpdate内では入力した瞬間などを検知できないからだ
/// todo : ただしこの仕様だといろいろ問題があるので見直しが必要
/// </summary>
public class PlayerInput
{
	private PlayerInput()
	{
		Initialize();
	}

	/// <summary>
	/// 初期化
	/// </summary>
	private void Initialize()
	{
	}

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

	#region - 押した瞬間を検知するボタン

	private bool _attack;
	/// <summary>
	/// 攻撃ボタン
	/// </summary>
	public bool attack
	{
		get
		{
			bool ret = _attack;
			// 同時入力判定中はfalseを返す
			if (jumpTrampledInputCount <= jumpTrampledInputSeconds)
			{
				return false;
			}
			_attack = false;
			return ret;
		}
		private set { _attack = value; }
	}

	private bool _jump;
	/// <summary>
	/// ジャンプボタン
	/// </summary>
	public bool jump
	{
		get
		{
			bool ret = _jump;
			// 同時入力判定中はfalseを返す
			if (jumpTrampledInputCount <= jumpTrampledInputSeconds)
			{
				return false;
			}
			_jump = false;
			return ret;
		}
		private set { _jump = value; }
	}

	private bool _barrier;
	/// <summary>
	/// 結界発動ボタン
	/// </summary>
	public bool barrier
	{
		get
		{
			bool ret = _barrier;
			_barrier = false;
			return ret;
		}
		private set { _barrier = value; }
	}

	private bool _itemGet;
	/// <summary>
	/// アイテム入手ボタン
	/// </summary>
	public bool itemGet
	{
		get
		{
			bool ret = _itemGet;
			_itemGet = false;
			return ret;
		}
		private set { _itemGet = value; }
	}

	private bool _jumpTrampled;
	/// <summary>
	/// 踏みつけジャンプ入力
	/// </summary>
	public bool jumpTrampled
	{
		get
		{
			bool ret = _jumpTrampled;
			_jumpTrampled = false;
			return ret;
		}
		private set { _jumpTrampled = value; }
	}

	private bool _cameraToFront;
	/// <summary>
	/// カメラを前方に向ける
	/// </summary>
	public bool cameraToFront
	{
		get
		{
			bool ret = _cameraToFront;
			_cameraToFront = false;
			return ret;
		}
		private set { _cameraToFront = value; }
	}

	private bool _lockOn;
	/// <summary>
	/// ロックオン
	/// </summary>
	public bool lockOn
	{
		get
		{
			bool ret = _lockOn;
			lockOn = false;
			return ret;
		}
		private set { _lockOn = value; }
	}

	#endregion

	/// <summary>
	/// 同時押し判定
	/// </summary>
	public bool attackAndJump
	{
		get; private set;
	}

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
	}

	private enum JumpTrampledInput
	{
		None,
		Jump,
		Attack,
		All,
	}

	private static readonly float jumpTrampledInputSeconds = 0.1f; //　同時押し判定用 猶予時間
	private float jumpTrampledInputCount; //　同時押し判定用 猶予カウンタ
	private JumpTrampledInput jumpTrampledInput = JumpTrampledInput.None;

	private int updateCount; // エラーチェック用カウンタ

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

		cameraToFront = Input.GetButtonDown(InputName.CameraToFront) ? true : cameraToFront;
		itemGet = Input.GetButtonDown(InputName.ItemGet) ? true : itemGet;
		barrier = Input.GetButtonDown(InputName.Barrier) ? true : barrier;

		jump = Input.GetButtonDown(InputName.Jump) ? true : jump;
		attack = Input.GetButtonDown(InputName.Attack) ? true : attack;
		if (Input.GetButton(InputName.Jump))
		{
			jumpTrampledInput |= JumpTrampledInput.Jump;
		}

		if (Input.GetButton(InputName.Attack))
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
			jumpTrampledInputCount += Time.deltaTime;
		}

		// 時間内に同時押しが成立
		if (jumpTrampledInput == JumpTrampledInput.All &&
			jumpTrampledInputCount < jumpTrampledInputSeconds)
		{
			jumpTrampled = true;
			// 同時押し判定が複数回発生しないようにカウンタを猶予時間外に設定
			jumpTrampledInputCount = jumpTrampledInputSeconds;
        }

		// 同時入力猶予時間を超えた
		if (jumpTrampledInputCount >= jumpTrampledInputSeconds)
		{
			jumpTrampled = false;
		}

		lockOn = Input.GetButtonDown(InputName.LockOn) ? true : lockOn;

		if (updateCount > 2)
		{
			//Debug.Log("入力情報が正しく検知できません。入力更新メソッドを適切に呼び出してください");
		}
		++updateCount;
	}

	/// <summary>
	/// 入力更新
	/// FixedUpdate内でこのメソッドをStartCoroutineで呼び出す
	/// </summary>
	public IEnumerator LateFixedUpdate()
	{
		yield return new WaitForFixedUpdate();
		cameraToFront = false;
		itemGet = false;
		barrier = false;

		// attack と jump については同時押しに使用するボタンなので判定に使用後のタイミングでリセット
		if (jumpTrampledInputCount >= jumpTrampledInputSeconds)
		{
			jump = false;
			attack = false;
		}

		lockOn = false;
		updateCount = 0;
	}
}
