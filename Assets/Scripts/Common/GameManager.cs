using UnityEngine;
using Uxtuno;

/// <summary>
/// ゲーム進行に必要な情報を保持し、制御する
/// シングルトンクラスとして設計
/// GameManager.instance でインスタンスを取得可能
/// 使用する際、不便なのでnamespaceは付けていない
/// </summary>
public class GameManager : MyMonoBehaviour
{
	static GameManager _instance; // 唯一のインスタンス

	/// <summary>
	/// 唯一のインスタンスを返す
	/// </summary>
	static public GameManager instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject go = new GameObject("GameManager");
				_instance = go.AddComponent<GameManager>();
			}
			return _instance;
		}
	}

	private Player _player;
	public Player player
	{
		get {
			if(_player == null)
			{
				_player = GameObject.FindGameObjectWithTag(TagName.Player).GetComponent<Player>();
			}
			return _player;
		}
	}

	//private static readonly string followIconCanvas = "FollowIconCanvas";
	void Start()
	{

	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.T))
		{
			Application.CaptureScreenshot("ScreenShot.png");
		}

		// プレイヤーの入力情報を更新
		PlayerInput.instance.Update(Time.deltaTime);
	}

	void FixedUpdate()
	{
		StartCoroutine(PlayerInput.instance.LateFixedUpdate());
	}
}
