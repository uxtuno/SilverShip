using UnityEngine;
using System.Collections;

/// <summary>
/// 単純にカメラの方向を向く
/// </summary>
public class Billboard : MonoBehaviour
{
	void Update()
	{
		transform.LookAt(Camera.main.transform);
	}
}
