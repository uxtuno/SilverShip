using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// レイヤー名を管理するクラス
/// </summary>
public static class Layers
{
	public struct LayerData
	{
		public string name;
		public int layer;
	}

	// 組み込みレイヤー
	public static readonly LayerData Default;
	public static readonly LayerData Transparent;
	public static readonly LayerData IgnoreRaycast;
	public static readonly LayerData Water;
	public static readonly LayerData UI;

	// ユーザー定義レイヤー
	public static readonly LayerData Leaser;
	public static readonly LayerData Title;
	public static readonly LayerData Menu;
	public static readonly LayerData SelectStage;

	static Layers()
	{
		Default.name = "Default";
		Default.layer = LayerMask.NameToLayer(Default.name);
		Transparent.name = "Transparent";
		Transparent.layer = LayerMask.NameToLayer(Transparent.name);
		IgnoreRaycast.name = "Ignore Raycast";
		IgnoreRaycast.layer = LayerMask.NameToLayer(IgnoreRaycast.name);
		Water.name = "Water";
		Water.layer = LayerMask.NameToLayer(Water.name);
		UI.name = "UI";
		UI.layer = LayerMask.NameToLayer(UI.name);

		Leaser.name = "Laser";
		Leaser.layer = LayerMask.NameToLayer(Leaser.name);
		Title.name = "Title";
		Title.layer = LayerMask.NameToLayer(Title.name);
		Menu.name = "Menu";
		Menu.layer = LayerMask.NameToLayer(Menu.name);
		SelectStage.name = "SelectStage";
		SelectStage.layer = LayerMask.NameToLayer(SelectStage.name);
	}
}
