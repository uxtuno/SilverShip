using UnityEngine;
using System.Linq;

public static class ExtensionMonoBehaviour{

	/// <summary>
	/// 全ての子オブジェクトを返す
	/// </summary>
	/// <param name="self"></param>
	/// <param name="includeInactive">非アクティブなGameObjectも探すか</param>
	/// <returns></returns>
	public static GameObject[] GetChildren(this GameObject self, bool includeInactive = false)
	{
		return self
			.GetComponentsInChildren<Transform>(includeInactive)
			.Where(c => c != self.transform)
			.Select(c => c.gameObject)
			.ToArray();
	}

	/// <summary>
	/// 全ての子オブジェクトを返す
	/// </summary>
	/// <param name="self"></param>
	/// <param name="includeInactive">非アクティブなGameObjectも探すか</param>
	/// <returns></returns>
	public static Transform[] GetChildren(this Transform self, bool includeInactive = false)
	{
		return self
			.GetComponentsInChildren<Transform>(includeInactive)
			.Where(c => c != self.transform)
			.ToArray();
	}

	/// <summary>
	/// 全ての子オブジェクトを返す
	/// </summary>
	/// <param name="self"></param>
	/// <param name="includeInactive">非アクティブなGameObjectも探すか</param>
	/// <returns></returns>
	public static Transform[] GetChildren(this MonoBehaviour self, bool includeInactive = false)
	{
		return self
			.GetComponentsInChildren<Transform>(includeInactive)
			.Where(c => c != self.transform)
			.ToArray();
	}

	public static T GetSafeComponent<T>(this GameObject self) where T : Component
	{
		T component = self.GetComponent<T>();
		if(component == null)
		{
			Debug.LogError(typeof(T) + "がアタッチされていません");
		}

		return component;
	}

	public static T GetSafeComponent<T>(this Component self) where T : Component
	{
		T component = self.GetComponent<T>();
		if (component == null)
		{
			Debug.LogError(typeof(T) + "がアタッチされていません", self.transform);
		}

		return component;
	}

}
