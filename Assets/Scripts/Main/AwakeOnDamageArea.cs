using UnityEngine;
using System.Collections;
using Kuvo;

[RequireComponent(typeof(Collider))]
public class AwakeOnDamageArea : MonoBehaviour {
	[SerializeField]
	private int _attack;
	public int attack{
		get { return _attack; }
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<BaseEnemy>() != null)
		{
			other.GetComponent<BaseEnemy>().Damage(attack, 1.0f);
        }
	}
}
