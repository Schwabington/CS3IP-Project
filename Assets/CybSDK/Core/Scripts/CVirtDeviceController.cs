/************************************************************************************

Filename    :   CVirtDeviceController.cs
Content     :   DeviceController establishes and maintains the connection to the VirtDevice.
Created     :   August 8, 2014
Last Updated:	December 5, 2019
Authors     :   Lukas Pfeifhofer
				Stefan Radlwimmer

Copyright   :   Copyright 2019 Cyberith GmbH

Licensed under the AssetStore Free License and the AssetStore Commercial License respectively.

************************************************************************************/

// The underlying class of UVirtDevice is dependent on the platform used
#if UNITY_ANDROID || PLATFORM_ANDROID
#undef UNITY_EDITOR_WIN // To avoid problems with intellisense
using UVirtDevice = CybSDK.CBleVirtDevice;
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using UVirtDevice = CybSDK.IVirtDevice;
#endif

// Use this define for multi player games
//#define CVirtDeviceController_Networking

#if CVirtDeviceController_Networking
using UnityEngine.Networking;
#endif

using System;
using UnityEngine;
using CybSDK;


public enum VirtDeviceType
{
	Automatic = 0,
	NativeVirtualizer = 1,
	XInput = 2,
	Keyboard = 3,
	Custom = 4,
}

public enum DecouplingType
{
	Automatic = 0,
	ForceCoupled = 1,
	ForceDecoupled = 2,
}

#if UNITY_ANDROID || PLATFORM_ANDROID
[RequireComponent(typeof(CBleVirtDevice))]
#endif
#if CVirtDeviceController_Networking
public class CVirtDeviceController : NetworkBehaviour
#else
public class CVirtDeviceController : MonoBehaviour
#endif
{
	private UVirtDevice virtDevice;
	//
	public VirtDeviceType deviceType = VirtDeviceType.Automatic;
	public DecouplingType decouplingType = DecouplingType.Automatic;
	public bool activateHaptic = true;
	public bool calibrateOnConnect = false;

	private bool _isDecoupled = true;
	//

	public enum CVirtDeviceControllerCallbackType
	{
		Connect,
		Disconnect
	}
	public delegate void CVirtDeviceControllerCallback(UVirtDevice virtDevice, CVirtDeviceController.CVirtDeviceControllerCallbackType callbackType);
	public event CVirtDeviceControllerCallback OnCVirtDeviceControllerCallback = null;

	// Use this for initialization
	void Awake()
	{
#if CVirtDeviceController_Networking
		if (!isLocalPlayer)
		{
			CLogger.Log("Virtualizer not initialized on non-local player!");
			return;
		}
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		virtDevice = InitVirtualizer(deviceType);

		if (virtDevice == null)
		{
			CLogger.LogError("Virtualizer device not found...");
			return;
		}
		CLogger.Log("Virtualizer device found, connecting...");

		if (!virtDevice.Open())
		{
			CLogger.LogError("Failed to connect to Virtualizer device.");
			return;
		}

		CLogger.Log("Successfully connected to Virtualizer device.");

		if (calibrateOnConnect)
		{
			//Wait for 25ms to let the Device gather data before resetting it
			this.Invoke(Calibrate, 0.025f);
		}
#elif UNITY_ANDROID
		virtDevice = gameObject.transform.Find("BLEConnectionPanel").GetComponent<CBleVirtDevice>();
		if (virtDevice == null)
		{
			CLogger.LogError(String.Format("CVirtDeviceController requires a BLEConnectionPanel gameobject to be attached to gameobject '{0}'.", gameObject.name));
			return;
		}

		virtDevice.OnCBleVirtDeviceCallback += OnCBleVirtDeviceCallback;
#endif
    }

#if UNITY_ANDROID
	private void OnCBleVirtDeviceCallback(CBleVirtDevice.States state)
	{
		if (OnCVirtDeviceControllerCallback != null && state == CBleVirtDevice.States.ACTIVE)
		{
			virtDevice.Open();
			OnCVirtDeviceControllerCallback(virtDevice, CVirtDeviceControllerCallbackType.Connect);
		}
	}
#endif

    void Start()
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		if (virtDevice == null || !virtDevice.IsOpen())
			return;

		if (OnCVirtDeviceControllerCallback != null)
			OnCVirtDeviceControllerCallback(virtDevice, CVirtDeviceControllerCallbackType.Connect);
#endif
	}

	private void Calibrate()
	{
		//Reset PlayerOrientation and PlayerHeight
		virtDevice.ResetPlayerOrientation();
		virtDevice.ResetPlayerHeight();
	}

	private UVirtDevice InitVirtualizer(VirtDeviceType type)
	{
		switch (type)
		{
			case VirtDeviceType.Automatic:
				for (VirtDeviceType i = VirtDeviceType.NativeVirtualizer; i <= VirtDeviceType.Custom; ++i)
				{
					UVirtDevice device = InitVirtualizer(i);
					if (device != null)
						return device;
				}
				return null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            case VirtDeviceType.NativeVirtualizer:
                CLogger.Log("Initializing Standard Virtualizer Device");
                SetIsDecoupling_Internal(true);
                return Virt.FindDevice();
            case VirtDeviceType.XInput:
                CLogger.Log("Initializing XInput Mockup");
                SetIsDecoupling_Internal(false);
                return Virt.CreateDeviceMockupXInput();
            case VirtDeviceType.Keyboard:
                CLogger.Log("Initializing Keyboard Mockup");
                SetIsDecoupling_Internal(false);
                return Virt.CreateDeviceMockupKeyboard();
#endif
			case VirtDeviceType.Custom:
				CLogger.Log("Initializing Custom Mockup");
				SetIsDecoupling_Internal(true);
				//ToDo: Add your custom IVirtDevice implementation here
				return null;
			default:
				CLogger.LogError("No IVirtDevice implementation available, fallback not possible.");
				return null;
		}
	}

	private void SetIsDecoupling_Internal(bool isDecoupled)
	{
		switch (decouplingType)
		{
			case DecouplingType.Automatic:
				_isDecoupled = isDecoupled;
				break;
			case DecouplingType.ForceCoupled:
				_isDecoupled = false;
				break;
			case DecouplingType.ForceDecoupled:
				_isDecoupled = true;
				break;
		}
	}

	public bool IsHapticEnabled()
	{
		return activateHaptic;
	}

	/// <summary>
	/// Returns the Virtualizer device managed by the CVirtDeviceController.
	/// </summary>
	public UVirtDevice GetDevice()
	{
		return virtDevice;
	}

	/// <summary>
	/// Returns true if the IVirtDevice does not support decoupled movement, otherwise false.
	/// </summary>
	public bool IsCoupled()
	{
		return !IsDecoupled();
	}

	/// <summary>
	/// Returns true if the IVirtDevice supports decoupled movement, otherwise false.
	/// </summary>
	public bool IsDecoupled()
	{
		return _isDecoupled;
	}

	//Cleanup if the object gets unloaded. This is important to prevent connection failures.
	void OnDestroy()
	{
		if (virtDevice == null)
			return;

		// Issue callback before closing
		if (OnCVirtDeviceControllerCallback != null)
			OnCVirtDeviceControllerCallback(virtDevice, CVirtDeviceControllerCallbackType.Disconnect);

		virtDevice.Close();
		CLogger.Log("Automatically disconnected from Virtualizer device.");
		virtDevice = null;
	}
}
