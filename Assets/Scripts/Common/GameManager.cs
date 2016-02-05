using UnityEngine;
using Uxtuno;
using Kuvo;

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
				DontDestroyOnLoad(go);
				_instance = go.AddComponent<GameManager>();
			}
			return _instance;
		}
	}

	private Player _player;
	public Player player
	{
		get
		{
			if (_player == null)
			{
				_player = GameObject.FindGameObjectWithTag(TagName.Player).GetComponent<Player>();
			}
			return _player;
		}
	}

	public int score { get; set; }

	private static readonly float timeLimitSeconds = 300.0f; // 制限時間
	public float _timeLeft = timeLimitSeconds;
	public float timeLeft { get { return _timeLeft; } private set { _timeLeft = value; } }

	/// <summary>
	/// シーン切り替え時に呼ばれる
	/// </summary>
	/// <param name="level"></param>
	public void OnLevelWasLoaded(int level)
	{
		// 制限時間で残り時間を初期化
		timeLeft = timeLimitSeconds;
	}

	//private static readonly string followIconCanvas = "FollowIconCanvas";
	void Start()
	{

	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			Application.CaptureScreenshot("ScreenShot.png");
		}

		// プレイヤーの入力情報を更新
		PlayerInput.Update(Time.deltaTime);

		timeLeft -= Time.deltaTime;
	}

	void FixedUpdate()
	{
		StartCoroutine(PlayerInput.LateFixedUpdate());
	}
}
