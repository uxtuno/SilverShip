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

	private GameData data = new GameData();
	public GameData GetGameData() { return data; }

	public int score { get; set; }

	private static readonly float timeLimitSeconds = 300.0f; // 制限時間
	public float _timeLeft = timeLimitSeconds;
	public float timeLeft { get { return _timeLeft; } private set { _timeLeft = value; } }
	public float elapsedTimeSconds {
		get { return timeLimitSeconds - timeLeft; }
	}
	private bool isTransition; // シーン遷移

	/// <summary>
	/// シーン切り替え時に呼ばれる
	/// </summary>
	/// <param name="level"></param>
	public void OnLevelWasLoaded(int level)
	{
		isTransition = false;
		// 制限時間で残り時間を初期化
		timeLeft = timeLimitSeconds;
		score = 0;
		timeLeft = 0.0f;
	}

	//private static readonly string followIconCanvas = "FollowIconCanvas";
	void Start()
	{

	}

	void Update()
	{
		if(Application.loadedLevelName != SceneName.main)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.T))
		{
			Application.CaptureScreenshot("ScreenShot.png");
		}

		// プレイヤーの入力情報を更新
		PlayerInput.Update(Time.deltaTime);

		timeLeft -= Time.deltaTime;
		if(timeLeft < 0.0f)
		{
			timeLeft = 0.0f;
		}
	}

	void LateUpdate()
	{
		if (Application.loadedLevelName != SceneName.main)
		{
			return;
		}

		if (timeLeft <= 0.0f)
		{
			ChangeResultScene();
		}

		if(player.hp <= 0)
		{
			ChangeResultScene();
		}
	}

	void FixedUpdate()
	{
		StartCoroutine(PlayerInput.LateFixedUpdate());
	}

	private void ChangeResultScene()
	{
		if (isTransition)
			return;
		isTransition = true;
		data.clearHP = (int)player.hp;
		data.score = score;
		data.timeLeft = timeLeft;
		SceneChangerSingleton.instance.FadeChange(SceneName.result);
	}
}

/// <summary>
/// シーン間の受け渡しが必要なデータ
/// </summary>
public class GameData
{
	public int score { get; set; }
	public float timeLeft { get; set; }
	public int clearHP { get; set; }
}