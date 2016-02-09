﻿using System.Collections;
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
	public class EnemyManagerSingleton : MyMonoBehaviour
	{
		#region - シングルトンを実現させるための処理 -
		// 唯一のインスタンス
		private static EnemyManagerSingleton _instance;

		/// <summary>
		/// プライベートコンストラクタ―
		/// </summary>
		private EnemyManagerSingleton()
		{
		}

		/// <summary>
		/// インスタンス(ヒエラルキー内に配置されていない場合nullが返る)
		/// </summary>
		public static EnemyManagerSingleton instance
		{
			get
			{
				if (!_instance)
				{
					if (!(_instance = FindObjectOfType<EnemyManagerSingleton>()))
					{
						UnityEngine.Debug.LogError("EnemyManagerSingletonが存在しませんでした\n規定値としてnullを使用します。");
					}
				}

				return _instance;
			}
		}
		#endregion

		[System.Serializable]
		private class CreateEnemyInformation
		{
			[Tooltip("生成するエネミーのプレハブ")]
			public List<GameObject> enemyPrefabs = new List<GameObject>();
			[Tooltip("生成数")]
			public int createNumber = 50;
			[Tooltip("生成地点と範囲")]
			public BoxCollider createArea = null;
		}

		[System.Serializable]
		private class Wave
		{
			[Tooltip("エネミーの生成情報")]
			public List<CreateEnemyInformation> createEnemyInformations = new List<CreateEnemyInformation>();
		}

		[Tooltip("WAVE"), SerializeField]
		private List<Wave> waves = new List<Wave>();
		private int currentWaveIndex = -1;			// Waveの選択と同時に加算するため、-1で初期化
		private GameObject enemyFolder = null;      // エネミーを格納する入れ物

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
				UnityEngine.Debug.LogWarning("EnemyManagerSingletonが複数存在しました\n一方を削除します。");
				Destroy(this);
				return;
			}
		}

		private void Start()
		{
			if(waves.Count <= 0)
			{
				UnityEngine.Debug.LogWarning("Waveが存在しません");
				Destroy(this);
				return;
            }
			
			// エネミーをまとめるGameObjectを探し、無ければ生成
			if (!enemyFolder)
			{
				if (!(enemyFolder = GameObject.Find("Enemies")))
				{
					enemyFolder = new GameObject("Enemies");
				}
			}

			// Waveの選択と同時に加算するため、-1で初期化
			if(currentWaveIndex != -1)
			{
				currentWaveIndex = -1;
			}

			enemies = new List<BaseEnemy>();

			StartWave();
		}

		public void FixedUpdate()
		{
			if(enemies.Count <= 0)
			{
				StartWave();
			}
		}

		/// <summary>
		/// エネミーを生成する
		/// </summary>
		/// <param name="enemyPrefabs"> エネミーのプレハブ</param>
		/// <param name="centerPosition"> 生成範囲の中心</param>
		/// <param name="range"> 範囲</param>
		/// <param name="createNumber"> 生成数</param>
		private void Create(List<GameObject> enemyPrefabs, BoxCollider createArea, int createNumber)
		{
			if(enemyPrefabs.Count <= 0)
			{
				UnityEngine.Debug.LogError("enemyPrefabsが正しく設定されていません。");
				return;
			}

			if(!createArea)
			{
				UnityEngine.Debug.LogError("createAreaが正しく設定されていません。");
				return;
			}
			else if(!createArea.isTrigger)
			{
				UnityEngine.Debug.LogWarning("createAreaのisTriggerがfalseになっています!");
			}

			foreach(GameObject enemyPrefab in enemyPrefabs)
			{
				if(!enemyPrefab)
				{
					UnityEngine.Debug.LogError("enemyPrefabがnullです。");
					return;
				}
			}

			// transformの位置,大きさも反映
			Vector3 centerPosition = createArea.center + createArea.transform.position;
			float createX = createArea.size.x * createArea.transform.lossyScale.x;
			float createY = createArea.size.y * createArea.transform.lossyScale.y;
			float createZ = createArea.size.z * createArea.transform.lossyScale.z;

			// 範囲内のランダムな位置に生成
			for (int i = 0; i < createNumber; i++)
			{
				Quaternion rotation = new Quaternion();

				// ランダムな位置・角度を計算
				rotation.eulerAngles = new Vector3(0, Random.Range(0, 359));
				float randamX = Random.Range(-(createX / 2), createX / 2);
				float randamY = Random.Range(-(createY / 2), createY / 2);
				float randamZ = Random.Range(-(createZ / 2), createZ / 2);
				Vector3 position = centerPosition + new Vector3(randamX, randamY, randamZ);

				// 登録されているエネミーをランダムに選択
				int selectIndex = Random.Range(0, enemyPrefabs.Count);

				GameObject enemy = Instantiate(enemyPrefabs[selectIndex], position, rotation) as GameObject;
				enemy.transform.SetParent(enemyFolder.transform);

				if (enemies == null)
				{
					enemies = new List<BaseEnemy>();
				}

				enemies.Add(enemy.GetSafeComponent<BaseEnemy>());
			}
		}

		/// <summary>
		/// 次のWaveを開始する
		/// </summary>
		private void StartWave()
		{
			// 最後の要素まで到達した場合最初の要素へループ
			if(++currentWaveIndex >= waves.Count)
			{
				currentWaveIndex = 0;
			}

			if(waves[currentWaveIndex].createEnemyInformations.Count <= 0)
			{
				UnityEngine.Debug.LogError("createEnemyInformationsが正しく設定されていません。");
				return;
			}

			foreach (CreateEnemyInformation createEnemyInformation in waves[currentWaveIndex].createEnemyInformations)
			{
				// コードの可読性のために値を変数に格納
				List<GameObject> enemyPrefabs = createEnemyInformation.enemyPrefabs;
				BoxCollider createArea = createEnemyInformation.createArea;
				int createNumber = createEnemyInformation.createNumber;

				Create(enemyPrefabs, createArea, createNumber);
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
