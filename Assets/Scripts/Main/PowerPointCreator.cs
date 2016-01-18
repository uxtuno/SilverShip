using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PowerPointCreator : MonoBehaviour
{
	private GameObject powerPointPrefab;
	private Transform powerPointParent; // 結界の点をまとめる親
	private LinkedList<Transform> powerPointList = new LinkedList<Transform>();
	private static readonly int powerPointMax = 5; // 結界点の同時設置可能数

	void Awake()
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

		if (powerPointList.Count >= powerPointMax)
		{
			LinkedListNode<Transform> first = powerPointList.First;
			// 最初の点をシーン上から削除
			if (first.Value != null)
			{
				Destroy(first.Value.gameObject);
			}
			// リスト上から削除
			powerPointList.Remove(first);
		}
		powerPointList.AddLast(powerPoint.transform);
	}

	/// <summary>
	/// 生成した結界点の数を返す
	/// </summary>
	public int count
	{
		// リスト上にnullのまま残っている可能性もあるのでここでは実際のシーン上の数を返す
		get { return powerPointParent.childCount; }
	}

	/// <summary>
	/// シーン上に存在する結界の点を返す
	/// </summary>
	/// <returns></returns>
	public ICollection<Transform> GetPowerPoints()
	{
		List<Transform> powerPoints = new List<Transform>();
		foreach(Transform powerPoint in powerPointParent)
		{
			powerPoints.Add(powerPoint);
		}
		return powerPoints;
	}
}
