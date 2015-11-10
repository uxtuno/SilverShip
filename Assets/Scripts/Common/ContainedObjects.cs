using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ContainedObjects : MonoBehaviour, IEnumerable<Transform>
{
	private List<Transform> objects = new List<Transform>();
	[Tooltip("対応するタグ") , SerializeField]
	private string[] tagNames;

	void Start()
	{
		// トリガー属性に
		GetComponent<Collider>().isTrigger = true;
	}

	void OnTriggerEnter(Collider other)
	{
		foreach(string tagName in tagNames)
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
}
