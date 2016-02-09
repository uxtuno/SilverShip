namespace Uxtuno
{
	namespace PlayerState
	{
		public abstract class BaseState
		{
			protected Player player;
			public BaseState(Player player)
			{
				this.player = player;
			}

			/// <summary>
			/// 状態ごとの動作
			/// </summary>
			public abstract void Move();
		}
	}
}