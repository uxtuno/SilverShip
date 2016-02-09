using UnityEngine;
using System.Collections;

public class PowerPoint : MonoBehaviour 
{
	[SerializeField]
	private float lifeSeconds = 5.0f; // 生存期間
	private float lifeCount; // 生存期間カウンタ

	void Update()
	{
		lifeCount += Time.deltaTime;
		if(lifeCount > lifeSeconds)
		{
			Destroy(gameObject);
		}
	}
}
