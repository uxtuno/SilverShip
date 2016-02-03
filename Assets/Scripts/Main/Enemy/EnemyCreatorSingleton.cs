using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// ステージ内に敵を配置するクラス
	/// シングルトンパターンで実装しているが、
	/// インスタンスの自動生成は行わないので
	/// ヒエラルキーに手動で配置してください
	/// </summary>
	public class EnemyCreatorSingleton : MyMonoBehaviour
	{
		#region - シングルトンを実現させるための処理 -
		// 唯一のインスタンス
		private static EnemyCreatorSingleton _instance;

		/// <summary>
		/// プライベートコンストラクタ―
		/// </summary>
		private EnemyCreatorSingleton()
		{
		}

		/// <summary>
		/// インスタンス(ヒエラルキー内に配置されていない場合nullが返る)
		/// </summary>
		public static EnemyCreatorSingleton instance
		{
			get
			{
				if (!_instance)
				{
					if (!(_instance = FindObjectOfType<EnemyCreatorSingleton>()))
					{
						Debug.LogError("EnemyCreatorSingletonが存在しませんでした\n規定値としてnullを使用します。");
					}
				}

				return _instance;
			}
		}
		#endregion

		[Tooltip("生成するエネミーのプレハブ"), SerializeField]
		private GameObject enemyPrefab = null;
		private float fieldDepth = 10.0f; // フィールドの奥行
		private float fieldWidth = 10.0f; // フィールドの幅
		[Tooltip("生成数"), SerializeField]
		private int generateNumber = 50; // 生成数

		/// <summary>
		/// 現在使用している攻撃コストを格納する
		/// </summary>
		public int currentAttackCostCount { get; private set; }

		/// <summary>
		/// 攻撃コストの最大値を格納する
		/// </summary>
		public int maxAttackCost
		{
			get { return 3; }
		}

		/// <summary>
		/// 使用可能コストを超えているかどうか
		/// </summary>
		public bool isCostOver
		{
			get { return maxAttackCost < currentAttackCostCount; }
		}

		/// <summary>
		/// enemyのリストを格納する
		/// </summary>
		public IList<BaseEnemy> enemies { get; private set; }

		/// <summary>
		/// enemiesからenemyAIのリストを取得する
		/// </summary>
		public IList<BaseEnemyAI> enemyAIs
		{
			get
			{
				// Linqを使用しフィルタリング
				BaseEnemyAI[] _enemyAIs = enemies
					.Select((obj) => obj.GetComponent<BaseEnemyAI>())
					.Where((obj) => obj != null)
					.ToArray();
				return _enemyAIs;
			}
		}

		/// <summary>
		/// 上司を取得する
		/// </summary>
		public BaseEnemyAI captainAI
		{
			get
			{
				foreach (BaseEnemyAI enemyAI in enemyAIs)
				{
					if (enemyAI.isCaptain)
					{
						return enemyAI;
					}
				}

				return null;
			}
		}

		public void Awake()
		{
			// 複数生成の禁止
			if (this != instance)
			{
				Destroy(gameObject);
			}
		}

		private void Start()
		{
			GameObject enemyFolder = new GameObject("Enemies");
			enemies = new List<BaseEnemy>();

			for (int i = 0; i < generateNumber; i++)
			{
				Quaternion rotate = new Quaternion();
				rotate.eulerAngles = new Vector3(0, Random.Range(0, 359));
				GameObject enemy = Instantiate(enemyPrefab, new Vector3(Random.Range(-(fieldWidth / 2), fieldWidth / 2), 2f, Random.Range(-(fieldDepth / 2), fieldDepth / 2)), rotate) as GameObject;

				enemy.transform.SetParent(enemyFolder.transform);
				enemies.Add(enemy.GetComponent<BaseEnemy>());
			}
		}

		/// <summary>
		/// 指定秒後に指定数のコストを加算する
		/// (※負の値を入れることで減算も可能)
		/// (※秒数は絶対値をとる)
		/// </summary>
		/// <param name="cost"> 加算するコスト</param>
		/// <param name="second"> 待機時間(秒)</param>
		private IEnumerator CostAddForSeconds(int cost, float second)
		{
			yield return new WaitForSeconds(Mathf.Abs(second));

			currentAttackCostCount += cost;
		}

		/// <summary>
		/// プライベート・コルーチン:CostAddForSecondsを実行する
		/// --CostAddForSecondsの仕様-->
		/// 指定秒後に指定数のコストを加算する
		/// (※負の値を入れることで減算も可能)
		/// (※秒数は絶対値をとる)
		/// </summary>
		/// <param name="cost"> 加算するコスト</param>
		/// <param name="second"> 待機時間(秒)</param>
		public void StartCostAddForSeconds(int cost, float second)
		{
			StartCoroutine(CostAddForSeconds(cost, second));
		}
	}
}
