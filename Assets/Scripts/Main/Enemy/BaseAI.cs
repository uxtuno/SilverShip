using UnityEngine;
using System.Collections;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// AIとしての共通動作を規定した抽象クラス
	/// </summary>
	[RequireComponent(typeof(Enemy))]
	abstract public class BaseAI : MyMonoBehaviour
	{
		/// <summary>
		/// AIレベル一覧
		/// </summary>
		protected enum AILevel
		{
			None,
			Fool,
			Nomal,
			Smart,
		}

		[Tooltip("AIのレベル"), SerializeField]
		protected AILevel aILevel = AILevel.None;       // AIのレベル

		private Player _player;		// playerプロパティの実体

		/// <summary>
		/// プレイヤーの参照を格納する
		/// </summary>
		protected Player player
		{
			get
			{
				if(!_player)
				{
					_player = GameManager.instance.player;
				}

				return _player;
			}
		}

		protected virtual void Update()
		{
			// レベルが設定されていない場合何もしない
			if (aILevel == AILevel.None)
			{
				return;
			}
		}
	}
}