using UnityEngine;
using System.Collections;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// 敵の共通動作を規定した抽象クラス
	/// </summary>
	abstract public class Enemy : Actor
	{
		private CameraController cameraController;

		/// <summary>
		/// エネミーを目視することができる最も近い距離
		/// (エネミーの大きさに応じて変更する必要がある)
		/// </summary>
		abstract protected float sight { get; set; }

		private GameObject _shortRangeAttackAreaObject;   // attackAreaObjectプロパティの実体

		/// <summary>
		/// 攻撃判定用のゲームオブジェクトを取得する
		/// </summary>
		protected GameObject shortRangeAttackAreaObject
		{
			get
			{
				if (!_shortRangeAttackAreaObject)
				{

					foreach (GameObject child in gameObject.GetChildren(true))
					{
						if (child.tag == TagName.AttackArea)
						{
							_shortRangeAttackAreaObject = child;
						}
					}

					_shortRangeAttackAreaObject.GetSafeComponent<AttackArea>();

					if (!_shortRangeAttackAreaObject)
					{
						Debug.LogError("攻撃判定用のゲームオブジェクトが見つかりませんでした。");
					}
				}

				return _shortRangeAttackAreaObject;
			}
		}

		protected virtual void Start()
		{
			cameraController = GameObject.FindGameObjectWithTag(TagName.CameraController).GetComponent<CameraController>();
		}

		protected virtual void Update()
		{
			// カメラとの距離に応じて描画状態を切り替える
			Vector3 cameraToVector = cameraController.cameraTransform.position - transform.position;
			if (cameraToVector.magnitude < sight)
			{
				isShow = false;
			}
			else
			{
				isShow = true;
			}
		}

		/// <summary>
		/// 近接攻撃
		/// </summary>
		abstract public IEnumerator ShortRangeAttack();
	}
}