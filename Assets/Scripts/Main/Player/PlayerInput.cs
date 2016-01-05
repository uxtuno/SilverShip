using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 似たような処理の塊になっている、できるならループなどで一括で処理したい
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
	/// <summary>
	/// 攻撃ボタン
	/// </summary>
	public bool attack { get; private set; }

	/// <summary>
	/// ジャンプボタン
	/// </summary>
	public bool jump { get; private set; }

	/// <summary>
	/// 結界発動ボタン
	/// </summary>
	public bool barrier { get; private set; }

	/// <summary>
	/// アイテム入手ボタン
	/// </summary>
	public bool itemGet { get; private set; }

	/// <summary>
	/// カメラを前方に向ける
	/// </summary>
	public bool cameraToFront { get; private set; }

	/// <summary>
	/// ロックオン
	/// </summary>
	public bool lockOn { get; private set; }

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
		lockOn = Input.GetButtonDown(InputName.LockOn) ? true : lockOn;

		if(updateCount > 2)
		{
			Debug.Log("入力情報が正しく検知できません。入力更新メソッドを適切に呼び出してください");
		}
		++updateCount;
	}

	/// <summary>
	/// 入力更新
	/// FixedUpdate内でも正しく入力を受け取れるように
	/// FixedUpdate内でこのコルーチンを呼び出す
	/// </summary>
	public IEnumerator LateFixedUpdate()
	{
		yield return new WaitForFixedUpdate();
		cameraToFront = false;
		itemGet = false;
		barrier = false;
		jump = false;
		attack = false;
		lockOn = false;
		updateCount = 0;
    }
}
