using UnityEngine;
using System.Diagnostics;

namespace Uxtuno
{
	public class Debug
	{
		[Conditional("Uxtuno")]
		public static void Log(object message)
		{
			UnityEngine.Debug.Log(message);
		}

		[Conditional("Uxtuno")]
		public static void Log(object message, Object context)
		{
			UnityEngine.Debug.Log(message, context);
		}

		[Conditional("Uxtuno")]
		public static void LogError(object message)
		{
			UnityEngine.Debug.LogError(message);
		}

		[Conditional("Uxtuno")]
		public static void LogError(object message, Object context)
		{
			UnityEngine.Debug.LogError(message, context);
		}

		[Conditional("Uxtuno")]
		public static void LogWarning(object message)
		{
			UnityEngine.Debug.LogWarning(message);
		}

		[Conditional("Uxtuno")]
		public static void LogWarning(object message, Object context)
		{
			UnityEngine.Debug.LogWarning(message, context);
		}

		[Conditional("Uxtuno")]
		public static void DrawLine(Vector3 start, Vector3 end)
		{
			UnityEngine.Debug.DrawLine(start, end);
		}

		[Conditional("Uxtuno")]
		public static void DrawLine(Vector3 start, Vector3 end, Color color)
		{
			UnityEngine.Debug.DrawLine(start, end, color);
		}

		[Conditional("Uxtuno")]
		public static void DrawRay(Vector3 start, Vector3 dir)
		{
			UnityEngine.Debug.DrawRay(start, dir);
		}

		[Conditional("Uxtuno")]
		public static void DrawRay(Vector3 start, Vector3 dir, Color color)
		{
			UnityEngine.Debug.DrawRay(start, dir, color);
		}
	}
}

namespace Kuvo
{
	public class Debug
	{
		[Conditional("Kuvo")]
		public static void Log(object message)
		{
			UnityEngine.Debug.Log(message);
		}

		[Conditional("Kuvo")]
		public static void Log(object message, Object context)
		{
			UnityEngine.Debug.Log(message, context);
		}

		[Conditional("Kuvo")]
		public static void LogError(object message)
		{
			UnityEngine.Debug.LogError(message);
		}

		[Conditional("Kuvo")]
		public static void LogError(object message, Object context)
		{
			UnityEngine.Debug.LogError(message, context);
		}

		[Conditional("Kuvo")]
		public static void LogWarning(object message)
		{
			UnityEngine.Debug.LogWarning(message);
		}

		[Conditional("Kuvo")]
		public static void LogWarning(object message, Object context)
		{
			UnityEngine.Debug.LogWarning(message, context);
		}

		[Conditional("Kuvo")]
		public static void DrawLine(Vector3 start, Vector3 end)
		{
			UnityEngine.Debug.DrawLine(start, end);
		}

		[Conditional("Kuvo")]
		public static void DrawLine(Vector3 start, Vector3 end, Color color)
		{
			UnityEngine.Debug.DrawLine(start, end, color);
		}

		[Conditional("Kuvo")]
		public static void DrawRay(Vector3 start, Vector3 dir)
		{
			UnityEngine.Debug.DrawRay(start, dir);
		}

		[Conditional("Kuvo")]
		public static void DrawRay(Vector3 start, Vector3 dir, Color color)
		{
			UnityEngine.Debug.DrawRay(start, dir, color);
		}
	}
}