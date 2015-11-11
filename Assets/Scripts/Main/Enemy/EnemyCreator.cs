using UnityEngine;
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
		private float fieldDepth = 100.0f; // フィールドの奥行
		private float fieldWidth = 100.0f; // フィールドの幅
		private int generateNumber = 5; // 生成数
		private GameObject enemyFolder = null;
		// Use this for initialization
		void Start()
		{
			enemyFolder = new GameObject("Enemies");
			for (int i = 0; i < generateNumber; ++i)
			{
				float z = (Random.value * fieldDepth) - fieldDepth / 2.0f;
				float x = (Random.value * fieldWidth) - fieldWidth / 2.0f;

				Quaternion q = Quaternion.identity;
				float r = Random.Range(0.0f, 360.0f);
				q.eulerAngles = new Vector3(0.0f, r, 0.0f);
				GameObject monster = (GameObject)Instantiate(monsterPrefab, new Vector3(x, 2.0f, z), q);
				monster.transform.parent = enemyFolder.transform;
			}
		}
	}
}
