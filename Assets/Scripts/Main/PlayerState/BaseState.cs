using System.Collections;

namespace Uxtuno
{
	namespace PlayerState
	{
		public abstract class BaseState
		{
			/// <summary>
			/// 初期化
			/// </summary>
			public abstract void Initialize(Player player);

			/// <summary>
			/// 状態ごとの動作
			/// </summary>
			public abstract IEnumerator Move(Player player);
		}
	}
}