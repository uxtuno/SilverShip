using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class ContainedObjects : MonoBehaviour, IEnumerable<Transform>
{
	private IList<Transform> objects = new List<Transform>();
	[Tooltip("対応するタグ"), SerializeField]
	private string[] tagNames = new string[0];

	/// <summary>
	/// タグを追加
	/// </summary>
	/// <param name="name"></param>
	public void AddTagName(string name)
	{
		// 一度Listに変換してから要素を追加(重いかな～？)
		List<string> list = new List<string>(tagNames);
		if (!list.Contains(name))
		{
			list.Add(name);
		}
		tagNames = list.ToArray();
	}

	void Start()
	{
		// トリガー属性に
		GetComponent<Collider>().isTrigger = true;
	}

	void OnTriggerEnter(Collider other)
	{
		foreach (string tagName in tagNames)
		{
			if (other.tag == tagName)
			{
				objects.Add(other.transform);
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		objects.Remove(other.transform);
	}

	public IEnumerator<Transform> GetEnumerator()
	{
		return ((IEnumerable<Transform>)objects).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<Transform>)objects).GetEnumerator();
	}

	/// <summary>
	/// 内包オブジェクトを返す
	/// </summary>
	/// <returns></returns>
	public IList<Transform> GetContainedObjects()
	{
		return objects;
	}
}
