using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// TODO : このクラスは突貫工事なので後で書き直す。
	/// </summary>
	public class EnemyCreator : MyMonoBehaviour
	{
		[Tooltip("生成するエネミーのプレハブ"), SerializeField]
		private GameObject monsterPrefab = null;
		private float fieldDepth = 10.0f; // フィールドの奥行
		private float fieldWidth = 10.0f; // フィールドの幅
		[SerializeField]
		private int generateNumber = 50; // 生成数

		public ICollection<Enemy> enemies { get; private set; }

		public void Start()
		{
			GameObject enemyFolder = new GameObject("Enemies");
			enemies = new List<Enemy>();

			for (int i = 0; i < generateNumber; i++)
			{
				Quaternion rotate = new Quaternion();
				rotate.eulerAngles = new Vector3(0, Random.Range(0, 359));
				GameObject enemy = Instantiate(monsterPrefab, new Vector3(Random.Range(-(fieldWidth / 2), fieldWidth / 2), 2f, Random.Range(-(fieldDepth / 2), fieldDepth / 2)), rotate) as GameObject;

				enemy.transform.SetParent(enemyFolder.transform);
				enemies.Add(enemy.GetComponent<Enemy>());
			}
		}

		//	// Use this for initialization
		//	void Start()
		//	{
		//		GameObject enemyFolder = new GameObject("Enemies");
		//		for (int i = 0; i < generateNumber; ++i)
		//		{
		//			float z = (Random.value * fieldDepth) - fieldDepth / 2.0f;
		//			float x = (Random.value * fieldWidth) - fieldWidth / 2.0f;

		//			Quaternion q = Quaternion.identity;
		//			float r = Random.Range(0.0f, 360.0f);
		//			q.eulerAngles = new Vector3(0.0f, r, 0.0f);
		//			GameObject monster = (GameObject)Instantiate(monsterPrefab, new Vector3(x, 2.0f, z), q);
		//			monster.transform.parent = enemyFolder.transform;
		//		}
		//	}

		//	// Update is called once per frame
		//	void Update()
		//	{
		//	}


	}
}
