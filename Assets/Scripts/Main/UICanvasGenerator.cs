using UnityEngine;
using System.Collections;

public class UICanvasGenerator : MonoBehaviour {
	private static GameObject _followIconCanvas;
	public static GameObject followIconCanvas
	{
		get {
			if(_followIconCanvas == null)
			{
				GameObject go = Resources.Load<GameObject>("Prefabs/UI/FollowIconCanvas");
				_followIconCanvas = Instantiate(go);
			}
			return _followIconCanvas;
		}
	}

	public static GameObject FollowIconCanvasGenerate()
	{
		return followIconCanvas;
	}
}
