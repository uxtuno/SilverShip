using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour {
	public int i;

	// Update is called once per frame
	void LateUpdate () {
		i = transform.childCount;
	}
}
