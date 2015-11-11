using UnityEngine;
using System.Collections;

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
			if(Mathf.Abs(rotationInput.y) < 0.5f)
			{
				rotationInput.y = 0.0f;
            }

			cameraHorizontal = rotationInput.x;
			cameraVertical = rotationInput.y;
		}

		// ゲームコントローラのABXY
		attack = Input.GetButtonDown(InputName.Atack);
		jump = Input.GetButtonDown(InputName.Jump);
		barrier = Input.GetButtonDown(InputName.Barrier);
		itemGet = Input.GetButtonDown(InputName.ItemGet);

		cameraToFront = Input.GetButtonDown(InputName.CameraToFront);
	}
}
