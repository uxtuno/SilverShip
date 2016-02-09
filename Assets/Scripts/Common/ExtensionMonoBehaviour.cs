using UnityEngine;
using System.Linq;

/// <summary>
/// GameObjectに対する様々な操作を短く書くための拡張メソッド
/// </summary>
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
	/// 指定したコンポーネントを取得して返す
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
			if(component == null)
			{
				Debug.LogError( typeof(T) + "コンポーネントの追加に失敗しました(" + self.name + ")");
			}
			//Debug.LogWarning(typeof(T) + "を追加しました(実行時のみ)");
		}

		return component;
	}

	/// <summary>
	/// 指定したコンポーネントを取得して返す
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
			if (component == null)
			{
				Debug.LogError(typeof(T) + "コンポーネントの追加に失敗しました(" + self.name + ")");
			}
			//Debug.LogWarning(typeof(T) + "を追加しました(実行時のみ)");
		}

		return component;
	}

	/// <summary>
	/// 指定したコンポーネントが存在するか
	/// </summary>
	/// <typeparam name="T">コンポーネント</typeparam>
	/// <param name="self"></param>
	/// <returns></returns>
	public static bool HasComponent<T>(this GameObject self) where T : Component
	{
		return self.GetComponent<T>() != null;
	}

	/// <summary>
	/// 指定したコンポーネントが存在するか
	/// </summary>
	/// <typeparam name="T">コンポーネント</typeparam>
	/// <param name="self"></param>
	/// <returns></returns>
	public static bool HasComponent<T>(this Component self) where T : Component
	{
		return self.GetComponent<T>() != null;
	}

	/// <summary>
	/// 子オブジェクトを検索した結果をGameObjectとして返す
	/// </summary>
	/// <param name="self"></param>
	/// <param name="name">検索する名前</param>
	/// <returns></returns>
	public static GameObject FindGameObject(this GameObject self, string name)
	{
		var result = self.transform.Find(name);
		return result != null ? result.gameObject : null;
	}

	/// <summary>
	/// 子オブジェクトを検索した結果をGameObjectとして返す
	/// </summary>
	/// <param name="self"></param>
	/// <param name="name">検索する名前</param>
	/// <returns></returns>
	public static GameObject FindGameObject(this Component self, string name)
	{
		var result = self.transform.Find(name);
		return result != null ? result.gameObject : null;
	}

	/// <summary>
	/// 子オブジェクトが存在するか
	/// </summary>
	/// <param name="self"></param>
	/// <returns></returns>
	public static bool HasChild(this GameObject self)
	{
		return 0 < self.transform.childCount;
	}

	/// <summary>
	/// 子オブジェクトが存在するか
	/// </summary>
	/// <param name="self"></param>
	/// <returns></returns>
	public static bool HasChild(this Component self)
	{
		return 0 < self.transform.childCount;
	}

	/// <summary>
	/// 親オブジェクトが存在するか
	/// </summary>
	/// <param name="self"></param>
	/// <returns></returns>
	public static bool HasParent(this GameObject self)
	{
		return self.transform.parent != null;
	}

	/// <summary>
	/// 親オブジェクトが存在するか
	/// </summary>
	/// <param name="self"></param>
	/// <returns></returns>
	public static bool HasParent(this Component self)
	{
		return self.transform.parent != null;
	}

	/// <summary>
	/// 深い階層までGameObjectを名前で検索する
	/// </summary>
	/// <param name="self"></param>
	/// <param name="name">検索する名前</param>
	/// <param name="includeInactive">非アクティブ状態のオブジェクトを含めるか</param>
	/// <returns></returns>
	public static GameObject FindDeepGameObject(this GameObject self, string name, bool includeInactive = false)
	{
		var children = self.GetComponentsInChildren<Transform>(includeInactive);
		foreach (var transform in children)
		{
			if (transform.name == name)
			{
				return transform.gameObject;
			}
		}
		return null;
	}

	/// <summary>
	/// 深い階層までGameObjectを名前で検索する
	/// </summary>
	/// <param name="self"></param>
	/// <param name="name">検索する名前</param>
	/// <param name="includeInactive">非アクティブ状態のオブジェクトを含めるか</param>
	/// <returns></returns>
	public static GameObject FindDeepGameObject(this Component self, string name, bool includeInactive = false)
	{
		var children = self.GetComponentsInChildren<Transform>(includeInactive);
		foreach (var transform in children)
		{
			if (transform.name == name)
			{
				return transform.gameObject;
			}
		}
		return null;
	}

	/// <summary>
	/// GameObjectを名前で検索する
	/// </summary>
	/// <param name="self"></param>
	/// <param name="name">検索する名前</param>
	/// <returns></returns>
	public static Transform Find(this GameObject self, string name)
	{
		return self.transform.Find(name);
	}

	/// <summary>
	/// GameObjectを名前で検索する
	/// </summary>
	/// <param name="self"></param>
	/// <param name="name">検索する名前</param>
	/// <returns></returns>
	public static Transform Find(this Component self, string name)
	{
		return self.transform.Find(name);
	}
}
