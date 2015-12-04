using UnityEngine;
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

		public static EnemyCreatorSingleton Instance
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

		/// <summary>
		/// enemyのリストを格納する
		/// </summary>
		public IList<Enemy> enemies { get; private set; }

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
		/// 小隊の隊長を取得する
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

		public void Start()
		{
			GameObject enemyFolder = new GameObject("Enemies");
			enemies = new List<Enemy>();

			for (int i = 0; i < generateNumber; i++)
			{
				Quaternion rotate = new Quaternion();
				rotate.eulerAngles = new Vector3(0, Random.Range(0, 359));
				GameObject enemy = Instantiate(enemyPrefab, new Vector3(Random.Range(-(fieldWidth / 2), fieldWidth / 2), 2f, Random.Range(-(fieldDepth / 2), fieldDepth / 2)), rotate) as GameObject;

				enemy.transform.SetParent(enemyFolder.transform);
				enemies.Add(enemy.GetComponent<Enemy>());
			}
		}
	}
}
