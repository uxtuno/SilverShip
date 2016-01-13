using UnityEngine;
using System.Collections;

public class PowerPointCreator : MonoBehaviour {
	private GameObject powerPointPrefab;
	private Transform powerPointParent; // 結界の点をまとめる親

	void Start()
	{
		powerPointPrefab = Resources.Load<GameObject>("Prefabs/Effects/PowerPoint");
		powerPointParent = new GameObject("PowerPointCollecter").transform;
	}

	/// <summary>
	/// 生成
	/// </summary>
	/// <param name="position"></param>
	public void Create(Vector3 position)
	{
		GameObject powerPoint = Instantiate(powerPointPrefab, position, Quaternion.identity) as GameObject;
		powerPoint.transform.SetParent(powerPointParent);
	}

	/// <summary>
	/// 生成した結界点の数を返す
	/// </summary>
	public int count
	{
		get { return powerPointParent.childCount; }
	}
}
