using UnityEngine;
using System.Collections;

/// <summary>
/// 弾クラス
/// 弾の移動に関する処理を行う
/// 現段階ではEnemyからの使用しか想定していない
/// </summary>
public class Bullet : MonoBehaviour
{
	[SerializeField]
	float speed = 3.0f;
	void Start()
	{
		Transform target = GameManager.instance.player.transform;
		//target.position += Vector3.up;
		transform.LookAt(target);
	}

	void Update()
	{
		transform.Translate(Vector3.forward * speed * Time.deltaTime);

		if(transform.position.y > 60.0f || transform.position.y < -5.0f)
		{
			Destroy(gameObject);
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.tag == TagName.Player)
		{
			Destroy(gameObject);
		}
	}
}
