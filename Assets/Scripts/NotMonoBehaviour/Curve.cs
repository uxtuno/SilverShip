using UnityEngine;
using System.Collections.Generic;

public class Curve
{

	//private const int NodeMaxNumber = 10; // 追加できるポイント最大数

	//private class Node
	//{
	//	public Vector3 position;
	//	public Vector3 tan;
	//	public float speed;
	//}
	//private List<Node> nodes = new List<Node>(NodeMaxNumber); // 曲線上の接点リスト

	//private class Section
	//{
	//	public int index;
	//	public float distance;
	//}
	//private List<Section> sections = new List<Section>(NodeMaxNumber); // 曲線上の区間リスト

	//private class CurStatus
	//{
	//	public int sectionIndex;
	//	public float distance; // 現在の位置
	//	public float speed; // 現在の速さ
	//}

	//int buildCount = 1;

	///// <summary>
	///// 曲線上の接点を追加
	///// </summary>
	///// <param name="position"></param>
	//public void AddNode(Vector3 position)
	//{
	//	if (nodes.Count >= NodeMaxNumber)
	//	{
	//		return;
	//	}

	//	Node node = new Node();
	//	node.position = position;
	//	node.speed = 0.0f;
	//	nodes.Add(node);
	//}

	///// <summary>
	///// 接点情報から曲線を定義する。接点を追加したら必ず実行する事
	///// </summary>
	//public void Build()
	//{
	//	Vector3 v1;
	//	Vector3 v2;
	//	Vector3 vT;
	//	Vector3 vT1;
	//	Vector3 vT2;
	//	Vector3 vP1;
	//	Vector3 vP2;

	//	if (nodes.Count < 3)
	//	{
	//		return;
	//	}

	//	for (int i = 1; i < nodes.Count; ++i)
	//	{
	//		v1 = nodes[i - 1].position - nodes[i].position;

	//		// 中間ノード
	//		if (i < nodes.Count - 1)
	//		{
	//			Node node = nodes[i];
	//			v2 = nodes[i + 1].position - nodes[i].position;
	//			vT1 = v1.normalized;
	//			vT2 = v2.normalized;
	//			vT = vT2 - vT1;
	//			node.tan = vT.normalized;
	//			nodes[i] = node;
	//		}

	//		Section section = new Section();
	//		section.index = i;
	//		section.distance = v1.magnitude;
	//		sections.Add(section);
	//	}

	//	GetStartVelocity();
	//	buildCount = nodes.Count;
	//	GetEndVelocity();
	//}

	//private void GetStartVelocity()
	//{
	//	Vector3 vTemp;

	//	vTemp = 3.0f * (nodes[1].position - nodes[0].position)
	//		  / sections[0].distance;
	//	nodes[0].tan = 0.5f * (vTemp - nodes[1].position);
	//}

	//private void GetEndVelocity()
	//{
	//	Vector3 vTemp;
	//	int iIndex = buildCount - 1;

	//	vTemp = 3.0f * (nodes[iIndex].position - nodes[iIndex - 1].position)
	//		  / sections[iIndex - 1].distance;
	//	nodes[iIndex].tan = 0.5f * (vTemp - nodes[iIndex - 1].tan);
	//}

	/////////////////////////////////////////////////////////////////////////////////
	//// 関数名：GetPosition
	//// 概要　：移動状況を基に曲線上の座標を取得
	//// 引数　：pvPos	- 取得する座標
	////     　：psStatus	- 移動状況
	////     　：iElpTime	- 経過時間。フレーム単位の速さの時は経過フレーム数
	//// 戻り値：-
	/////////////////////////////////////////////////////////////////////////////////
	//private bool GetPosition(out Vector3 pvPos,
	//								CurStatus psStatus, int iElpTime)
	//{
	//	float fSpeed = 0.0f;
	//	float fDist = 0.0f;
	//	pvPos = Vector2.zero;

	//	int iIndex = psStatus.sectionIndex;

	//	// 区間外
	//	if (iIndex > nodes.Count - 1)
	//		return false;

	//	Section sSData1 = sections[iIndex];
	//	Node sPoint1 = nodes[iIndex];
	//	Node sPoint2 = nodes[iIndex + 1];

	//	// 現在の速さから次を計算
	//	fDist = psStatus.speed * iElpTime;

	//	// 区間の距離を超えた場合
	//	if (psStatus.distance + fDist > sSData1.distance)
	//	{
	//		// 次の区間の移動距離を計算
	//		if (iIndex < nodes.Count - 2)
	//		{
	//			Section sSData2 = sections[iIndex + 1];
	//			Node sPoint3 = nodes[iIndex + 2];
	//			float fTime = 0.0f;
	//			float fSpeed2 = 0.0f;
	//			float fDist2 = 0.0f;

	//			fTime = (float)iElpTime
	//						  - (sSData1.distance - psStatus.distance) / psStatus.speed;

	//			fDist2 = psStatus.speed * fTime;

	//			psStatus.distance = fDist2;
	//			psStatus.sectionIndex++;

	//			// 次区間の曲線上の座標を計算
	//			float u = psStatus.distance / sSData2.distance;
	//			Vector3 vVel1 = sSData2.distance * sPoint2.tan;
	//			Vector3 vVel2 = sSData2.distance * sPoint3.tan;

	//			//D3DXVec2Hermite(pvPos, sPoint2.position, vVel1, sPoint3.position, vVel2, u);
	//		}
	//		// 末端まで達した
	//		else
	//		{
	//			pvPos = nodes[nodes.Count - 1].position;
	//			psStatus.distance = 0.0f;
	//			psStatus.sectionIndex = 0;

	//			return false;
	//		}
	//	}
	//	// 現在の区間内
	//	else
	//	{
	//		psStatus.distance += fDist;

	//		// 区間の曲線上の座標を計算
	//		float u = psStatus.distance / sSData1.distance;
	//		Vector3 vVel1 = sSData1.distance * sPoint1.tan;
	//		Vector3 vVel2 = sSData1.distance * sPoint2.tan;

	//		//D3DXVec2Hermite(pvPos, sPoint1.position, vVel1, sPoint2.position, vVel2, u);
	//	}

	//	return true;
	//}

}
