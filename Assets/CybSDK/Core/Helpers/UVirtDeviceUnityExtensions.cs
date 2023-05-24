// The underlying class of UVirtDevice is dependent on the platform used
#if UNITY_ANDROID || PLATFORM_ANDROID
using UVirtDevice = CybSDK.CBleVirtDevice;
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using UVirtDevice = CybSDK.IVirtDevice;
#endif

using UnityEngine;

namespace CybSDK
{
	/// <summary>
	/// 
	/// </summary>
	public static class UVirtDeviceUnityExtensions
	{
		/// <summary>
		/// Returns the movement direction as a speed scaled vector relative to the current player orientation.
		/// </summary>
		public static Vector3 GetMovementVector(this UVirtDevice device)
		{
			return device.GetMovementDirectionVector() * device.GetMovementSpeed();
		}

		/// <summary>
		/// Returns the movement direction as vector relative to the current player orientation.
		/// </summary>
		/// <remarks>The origin is the GetPlayerOrientation method and increases clockwise.</remarks>
		public static Vector3 GetMovementDirectionVector(this UVirtDevice device)
		{
			float movementDirection = device.GetMovementDirection() * Mathf.PI;
			return new Vector3(
				Mathf.Sin(movementDirection),
				0.0f,
				Mathf.Cos(movementDirection)).normalized;
		}

		/// <summary>
		/// Returns the orientation of the player as vector.
		/// </summary>
		/// <remarks>The origin is set by the ResetPlayerOrientation method and increases clockwise.</remarks>
		public static Vector3 GetPlayerOrientationVector(this UVirtDevice device)
		{
			float playerOrientation = device.GetPlayerOrientation() * 2.0f * Mathf.PI;
			return new Vector3(
				Mathf.Sin(playerOrientation),
				0.0f,
				Mathf.Cos(playerOrientation)).normalized;
		}

		/// <summary>
		/// Returns the orientation of the player as quaternion.
		/// </summary>
		/// <remarks>The origin is set by the ResetPlayerOrientation method and increases clockwise.</remarks>
		public static Quaternion GetPlayerOrientationQuaternion(this UVirtDevice device)
		{
			float playerOrientation = device.GetPlayerOrientation() * 360.0f;
			return Quaternion.Euler(0.0f, playerOrientation, 0.0f);
		}
	}
}
