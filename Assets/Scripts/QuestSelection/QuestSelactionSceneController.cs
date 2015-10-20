using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class QuestSelactionSceneController : MonoBehaviour
{
	/// <summary>
	/// 以前選択状態にあったゲームオブジェクト(切り替え確認用)
	/// </summary>
	private GameObject oldSelectedGameObject { get; set; }

	private void Awake()
	{
		oldSelectedGameObject = EventSystem.current.currentSelectedGameObject;
	}
	
	void Update()
	{
		// 選択状態のゲームオブジェクトが切り替わった直後に1度実行する
		if (oldSelectedGameObject != EventSystem.current.currentSelectedGameObject)
		{
			StartCoroutine(ChangeSelectedGameObject());
			oldSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		}
	}

	private IEnumerator ChangeSelectedGameObject()
	{
		// 選択状態のゲームオブジェクトが切り替わった直後の処理
		yield break;
	}
}
