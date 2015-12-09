using UnityEngine;
using System.Collections;

public class UICanvasGenerator : MonoBehaviour {
	private static GameObject _followIconCanvas;
	public static GameObject followIconCanvas
	{
		get {
			if(_followIconCanvas == null)
			{
				_followIconCanvas = FollowIconCanvasGenerate();
			}
			return _followIconCanvas;
		}
	}

	public static GameObject FollowIconCanvasGenerate()
	{
		GameObject go = Resources.Load<GameObject>("Prefabs/UI/FollowIconCanvas");
		return Instantiate(go);
	}
}
