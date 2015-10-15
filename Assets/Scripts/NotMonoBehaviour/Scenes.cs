using System;
using System.IO;
using System.Text;
//using UnityEditor;
//using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// シーン名を管理するクラス
/// </summary>
public static class Scenes
{
	public struct SceneData
	{
		public string name;
		public int id;
	}

	public static readonly SceneData Title;
	public static readonly SceneData Menu;
	public static readonly SceneData Stage;
	public static readonly SceneData TestStage01;
	public static readonly SceneData Player;
	public static readonly SceneData TutorialStage01;
	public static readonly SceneData EndScene;

	static Scenes()
	{
		Title.name = "Title";
		Title.id = GetSceneToId(Title.name);
		Menu.name = "Menu";
		Menu.id = GetSceneToId(Menu.name);
		TestStage01.name = "TestStage01";
		TestStage01.id = GetSceneToId(TestStage01.name);
		Player.name = "Player";
		Player.id = GetSceneToId(Player.name);
		TutorialStage01.name = "TutorialStage01";
		TutorialStage01.id = GetSceneToId(TutorialStage01.name);
		EndScene.name = "EndScene";
		EndScene.id = GetSceneToId(EndScene.name);
    }

	/// <summary>
	/// シーン名からシーン番号を得る
	/// </summary>
	/// <param name="name">シーン名</param>
	/// <returns></returns>
	public static int GetSceneToId(string name)
	{
		//for (int i = 0; i < EditorBuildSettings.scenes.Count(); i++)
		//{
		//	string sceneName = Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path);
		//	if (name == sceneName)
		//	{
		//		return i;
		//	}
		//}

		return -1;
	}
}
