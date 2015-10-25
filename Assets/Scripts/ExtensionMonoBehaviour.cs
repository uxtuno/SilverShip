﻿using UnityEngine;
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
}
