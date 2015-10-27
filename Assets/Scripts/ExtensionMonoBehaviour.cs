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
	/// 型パラメータとして指定したコンポーネントを返す
	/// コンポーネントがアタッチされていなかった場合はその場でアタッチしてから返す
	/// </summary>
	/// <typeparam name="T">取得するコンポーネント</typeparam>
	/// <param name="self"></param>
	/// <returns></returns>
	public static T GetSafeComponent<T>(this GameObject self) where T : Component
	{
		T component = self.GetComponent<T>();
		if (component == null)
		{
			component = self.gameObject.AddComponent<T>();
			//Debug.LogWarning(typeof(T) + "を追加しました(実行時のみ)");
		}

		return component;
	}

	/// <summary>
	/// 型パラメータとして指定したコンポーネントを返す
	/// コンポーネントがアタッチされていなかった場合はその場でアタッチしてから返す
	/// </summary>
	/// <typeparam name="T">取得するコンポーネント</typeparam>
	/// <param name="self"></param>
	/// <returns></returns>
	public static T GetSafeComponent<T>(this Component self) where T : Component
	{
		T component = self.GetComponent<T>();
		if (component == null)
		{
			component = self.gameObject.AddComponent<T>();
			//Debug.LogWarning(typeof(T) + "を追加しました(実行時のみ)");
		}

		return component;
	}
}
