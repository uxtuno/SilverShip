using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Uxtuno;

namespace Kuvo
{
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

		public static EnemyCreatorSingleton instance
		{
			get
			{
				_instance = FindObjectOfType<EnemyCreatorSingleton>();

				if (_instance == null)
				{
					GameObject go = new GameObject("EnemyCreatorSingleton");
					_instance = go.AddComponent<EnemyCreatorSingleton>();
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
		public int currentAttackCostCount { get; private set; }
		public readonly int maxAttackCost = 3;

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

		private void Update()
		{
			print(currentAttackCostCount);
		}

		/// <summary>
		/// 指定秒後に指定数のコストを加算する
		/// (負の値を入れることで減算も可能)
		/// </summary>
		/// <param name="cost"> 加算するコスト</param>
		/// <param name="second"> 待機時間(秒)</param>
		public IEnumerator CostAddForSeconds(int cost, float second)
		{
			yield return new WaitForSeconds(Mathf.Abs(second));

			currentAttackCostCount += cost;
		}
	}
}
