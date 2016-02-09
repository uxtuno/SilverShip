using UnityEngine;
using System.Collections;

namespace Uxtuno
{
	public class Rotate : MyMonoBehaviour
	{
		public enum Axis
		{
			Forward,
			Up,
			Left,
		}

		[SerializeField]
		private Axis rotateAxis = Axis.Forward; // 回転の軸、定数
		private Vector3 axis; // 回転の軸

		[SerializeField]
		private float speed = 1.0f; // 回転速度

		void Start()
		{
			switch (rotateAxis)
			{
				case Axis.Forward:
					axis = Vector3.forward;
					break;
				case Axis.Up:
					axis = Vector3.up;
					break;
				case Axis.Left:
					axis = Vector3.left;
					break;
			}
		}

		// Update is called once per frame
		void Update()
		{
			transform.Rotate(axis, speed * Time.deltaTime);
		}
	}
}