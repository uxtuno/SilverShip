using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuEvent : MonoBehaviour
{
	/// <summary>
	/// 以前選択状態にあったゲームオブジェクト(キャンセル処理用)
	/// </summary>
	private static GameObject oldSelectedGameObject { get; set; }
	
	/// <summary>
	/// 指定されたゲームオブジェクトの状態を有効にする
	/// </summary>
	/// <param name="obj"> 対象のゲームオブジェクト</param>
	public void OnShowGameObject(Object obj)
	{
		GameObject gameObject = obj as GameObject;

		if (!gameObject)
		{
			Debug.LogError("指定されたObjectはGameObjectではありません");
			return;
		}

		if (gameObject.activeInHierarchy)
		{
			return;
		}

		gameObject.SetActive(true);
	}

	/// <summary>
	/// 指定されたゲームオブジェクトの状態を無効にする	
	/// </summary>
	/// <param name="obj"> 対象のゲームオブジェクト</param>
	public void OnHiddenGameObject(Object obj)
	{
		GameObject gameObject = obj as GameObject;

		if (!gameObject)
		{
			Debug.LogError("指定されたObjectはGameObjectではありません");
			return;
		}

		if (!gameObject.activeInHierarchy)
		{
			return;
		}

		gameObject.SetActive(false);
	}

	/// <summary>
	/// シーンを切り替える
	/// </summary>
	/// <param name="sceneName"> 遷移先のシーン名</param>
	public void OnSceneChange(string sceneName)
	{
		Application.LoadLevel(sceneName);
	}

	/// <summary>
	/// 指定したプレハブからダイアログを開く
	/// </summary>
	/// <param name="dialogPrefab"> ダイアログのプレハブ</param>
	public void OnOpenDialog(Object dialogPrefab)
	{
		GameObject dialog = dialogPrefab as GameObject;

		if (!dialog)
		{
			Debug.LogError("指定されたオブジェクトはGameObjectではありません");
			return;
		}

		Canvas parent = FindObjectOfType<Canvas>() as Canvas;

		if (!parent)
		{
			Debug.LogError("Canvasが存在しません");
			return;
		}

		if (!parent.transform)
		{
			Debug.LogError("CanvasにTramsformが存在しません");
			return;
		}

		Selectable[] selectables = FindObjectsOfType<Selectable>();

		foreach (Selectable selectsble in selectables)
		{
			if (selectsble.interactable)
			{
				selectsble.interactable = false;
			}
		}

		dialog = Instantiate(dialog) as GameObject;
		dialog.transform.SetParent(parent.transform);
		dialog.GetComponent<RectTransform>().localScale = Vector3.one;
		dialog.GetComponent<RectTransform>().localPosition = Vector3.zero;

		oldSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		EventSystem.current.SetSelectedGameObject(dialog.transform.FindChild("CancelButton").gameObject);
	}

	/// <summary>
	/// 指定したダイアログを閉じる
	/// </summary>
	/// <param name="dialog"> ダイアログ</param>
	public void OnCloseDialog(Object dialog)
	{
		Destroy(dialog);

		Selectable[] selectables = FindObjectsOfType<Selectable>();

		foreach (Selectable selectsble in selectables)
		{
			if (!selectsble.interactable)
			{
				selectsble.interactable = true;
			}
		}

		EventSystem.current.SetSelectedGameObject(oldSelectedGameObject);
	}

	/// <summary>
	/// 指定したゲームオブジェクトに選択状態を以降する
	/// </summary>
	/// <param name="obj"> 移動先のゲームオブジェクト</param>
	public void OnCurrentSelectedGameObjectChange(Object obj)
	{
		GameObject nextGameObject = obj as GameObject;

		if (!nextGameObject)
		{
			Debug.LogError("指定されたObjectはGameObjectではありません");
			return;
		}

		oldSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		EventSystem.current.SetSelectedGameObject(nextGameObject);
	}
}
