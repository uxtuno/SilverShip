using UnityEngine;
using System.Collections;

public class PlayerInput {

	public float horizontal
	{
		get { return Input.GetAxis(InputName.Horizontal); }
	}

	public float vertical {
		get { return Input.GetAxis(InputName.Vertical); }
	}

	/// <summary>
	/// カメラ回転入力
	/// </summary>
    public Vector2 cameraRotation
	{
		get
		{
			Vector2 rotationInput = Vector2.zero;
			if (Input.GetMouseButton(1))
			{
				rotationInput = Camera.main.ScreenToViewportPoint(Input.mousePosition);
				// 中央を(0, 0)にする
				rotationInput.x -= 0.5f;
				rotationInput.y -= 0.5f;
			}

			rotationInput.x = Input.GetAxisRaw(InputName.CameraX);
			rotationInput.y = Input.GetAxisRaw(InputName.CameraY);
			return rotationInput;
		}
	}

	/// <summary>
	/// ハイジャンプ入力には二つのキーの同時押しが必要
	/// そのためのキーそれぞれを列挙体で表す。
	/// 現在はハイジャンプ限定の仕組みだが、いずれ汎用的な処理にするべき
	/// </summary>
	public enum HighJumpKey
	{
		None,
		Main,
		Sub,
		All = Main | Sub,
	}

	private HighJumpKey highJumpKey = HighJumpKey.None;
	private float highJumpInputTime = 0.3f;
	private float highJumpInputCount = 0.0f;

	public bool jump
	{
		get { return Input.GetButtonDown(InputName.Jump); }
	}

	public bool highJump { get; private set; }
	public bool atack { get; private set; }

	/// <summary>
	/// プレイヤー入力情報更新
	/// </summary>
	public void Update(float elapsedTime)
	{
		// 入力情報を追加
		if(Input.GetButton(InputName.Jump))
		{
			highJumpKey |= HighJumpKey.Main;
		}

		if(Input.GetButton(InputName.HighJump))
		{
			highJumpKey |= HighJumpKey.Sub;
		}

		// 受付時間のうちに両方のボタンが入力されている
		if (highJumpInputCount < highJumpInputTime && !highJump)
		{
			if (highJumpKey == HighJumpKey.All)
			{
				highJump = true;
			}
			else if(highJumpKey == HighJumpKey.None)
			{
				highJumpInputCount = 0.0f;
			}
		}
		else
		{
			highJump = false;
		}

		// ハイジャンプボタンのどちらかが押されていればカウントしていく
		if (highJumpKey != HighJumpKey.None)
		{
			highJumpInputCount += elapsedTime;
		}
		else
		{
			highJumpInputCount = 0.0f;
		}
	}
}
