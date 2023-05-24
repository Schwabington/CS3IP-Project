using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Android;
using CybSDK;

namespace CybSDK
{
    public class CBleVirtDevice : MonoBehaviour
    {
        //****************************************************************************************
        //*
        //* Virtualizer Device Interface
        //* 
        //****************************************************************************************
        //****************************************************************************************
        //* Connection
        //****************************************************************************************

        /// <summary>
        /// Opens the connection to the Virtualizer device.
        /// </summary>
        /// <returns>Always true no matter the eventual result, due to the asynchronous nature of BLE.</returns>
        public bool Open() { return androidInstance.Call<bool>("Open"); }

        /// <summary>
		/// Checks if the BLE connection is active and the usb connection was opened before.
		/// </summary>
        public bool IsOpen() { return state == States.ACTIVE && androidInstance.Call<bool>("IsOpen"); }

        /// <summary>
		/// Closes the connection to the Virtualizer device.
		/// </summary>
        /// <returns>Always true no matter the eventual result, due to the asynchronous nature of BLE.</returns>
        public bool Close() { return androidInstance.Call<bool>("Close"); }

        //****************************************************************************************
        //* Device Infos
        //****************************************************************************************

        // Struct adapted from the C# CybSDK library class VirtDeviceInfo
        public struct VirtDeviceInfo
        {
            /// <summary>
            /// The major firmware version.
            /// </summary
            public byte MajorVersion;
            /// <summary>
            /// The minor firmware version.
            /// </summary>
            public byte MinorVersion;
            /// <summary>
            /// The USB vendor id.
            /// </summary>
            public ushort VendorId;
            /// <summary>
            /// The USB vendor name.
            /// </summary>
            public string VendorName;
            /// <summary>
            /// The USB product id.
            /// </summary>
            public ushort ProductId;
            /// <summary>
            /// The USB product name.
            /// </summary>
            public string ProductName;
        }

        /// <summary>
        /// Returns the USB infos of this device.
        /// </summary>
        public VirtDeviceInfo GetDeviceInfo()
        {
            VirtDeviceInfo virtDeviceInfo;

            virtDeviceInfo.MajorVersion = androidInstance.Call<byte[]>("getMajorVersion")[0];

            virtDeviceInfo.MinorVersion = androidInstance.Call<byte[]>("getMinorVersion")[0];

            virtDeviceInfo.VendorId = BitConverter.ToUInt16(androidInstance.Call<byte[]>("getVendorId"), 0);

            byte[] vendorNameBytes = androidInstance.Call<byte[]>("getVendorName");
            virtDeviceInfo.VendorName = System.Text.Encoding.UTF32.GetString(vendorNameBytes, 0, vendorNameBytes.Length);

            virtDeviceInfo.ProductId = BitConverter.ToUInt16(androidInstance.Call<byte[]>("getProductId"), 0);

            byte[] productNameBytes = androidInstance.Call<byte[]>("getProductName");
            virtDeviceInfo.ProductName = System.Text.Encoding.UTF32.GetString(productNameBytes, 0, productNameBytes.Length);

            return virtDeviceInfo;
        }

        //****************************************************************************************
        //* Virtualizer Game Data
        //****************************************************************************************

        /// <summary>
        /// Returns the current player height relative to the default height.
        /// </summary>
        /// <remarks>The default height is set by the ResetPlayerHeight method.</remarks>
        /// <remarks>height &lt; -threshold: crouching</remarks>
        /// <remarks>height >  threshold: jumping</remarks>
        /// <returns>1.00f = 1cm.</returns>
        public float GetPlayerHeight() { return androidInstance.Call<float>("GetPlayerHeight"); }

        /// <summary>
        /// Assigns the current height to the default height.
        /// </summary>
        /// <remarks>This method should be called while the player is asked to stand upright.</remarks>
        public void ResetPlayerHeight() { androidInstance.Call("ResetPlayerHeight"); }

        /// <summary>
        /// Returns the orientation of the player as an absolute value.
        /// </summary>
        /// <remarks>The origin is set by the ResetPlayerOrientation method and increases clockwise.</remarks>
        /// <returns>logical: 0.00f to 1.00f (= physical: 0° to 360°).</returns>
        public float GetPlayerOrientation() { return androidInstance.Call<float>("GetPlayerOrientation"); }

        /// <summary>
        /// Assigns the current orientation to the default vector.
        /// </summary>
        /// <remarks>This method should be called while the player is asked to look forward.</remarks>
        /// <remarks>This orientation should be used to calibrate the HMD.</remarks>
        public void ResetPlayerOrientation() { androidInstance.Call("ResetPlayerOrientation"); }

        /// <summary>
        /// Returns the current movement speed in meters per second.
        /// </summary>
        /// <returns>1.00f = 1m/s</returns>
        public float GetMovementSpeed() { return androidInstance.Call<float>("GetMovementSpeed"); }

        /// <summary>
        /// Returns the movement direction relative to the current player orientation.
        /// </summary>
        /// <remarks>The origin is the GetPlayerOrientation method and increases clockwise.</remarks>
        /// <returns>logical: -1.00f to 1.00f (= physical: -180° to 180°).</returns>
        public float GetMovementDirection() { return androidInstance.Call<float>("GetMovementDirection"); }

        //****************************************************************************************
        //* Haptic
        //****************************************************************************************

        /// <summary>
        /// Checks if the Virtualizer device supports haptic feedback.
        /// </summary>
        public bool HasHaptic() { return androidInstance.Call<bool>("HasHaptic"); }

        /// <summary>
        /// Play a signal on the haptic unit.
        /// </summary>
        public void HapticPlay() { androidInstance.Call("HapticPlay"); }

        /// <summary>
        /// Stop the haptic unit.
        /// </summary>
        public void HapticStop() { androidInstance.Call("HapticStop"); }

        /// <summary>
        /// Set the gain (dB) level of the haptic unit.
        /// </summary>
        /// <param name="gain">The value can be 0, 1, 2 or 3.</param>
        public void HapticSetGain(int gain) { mGain = gain; }

        /// <summary>
        /// Set the frequency (Hz) of a sine wave on the haptic unit.
        /// </summary>
        /// <param name="frequency">The value is valid between 0Hz and 80Hz.</param>
        public void HapticSetFrequency(int frequency) { mFrequency = frequency; }

        /// <summary>
        /// Sets the haptic feedback (change of amplitude) in the baseplate.
        /// </summary>
        /// <param name="volume">The value is valid between 0 (no feedback) and 100 (full feedback).</param>
        public void HapticSetVolume(int volume) { mVolume = volume; }

        //****************************************************************************************
        //*
        //* BLE Connection
        //* 
        //****************************************************************************************

        public enum States
        {
            UNKNOWN,
            SELECTING,
            CONNECTING,
            CONNECTION_FAILED,
            CONNECTED,
            INITIALIZING_DATA,
            CALIBRATING,
            ACTIVE,
        }

        private States state = States.UNKNOWN;

        public delegate void CBleVirtDeviceCallback(States state);
        public event CBleVirtDeviceCallback OnCBleVirtDeviceCallback = null;

        private static AndroidJavaObject androidInstance = null;

        private string[] pairedDevices;
        private string savedVirtSamName = "VirtSAM1"; // Standard name used only the first time the app is launched

        [Tooltip("Defines the minimum interval time for haptic calls, which is needed for the Bluetooth write operation bottleneck. If the haptic feedback starts being out of sync/delayed, increase this value.")]
        public float hapticPeriod = 0.09f;
        private float lastHapticTime = 0.0f;
        
        private const string setStateMessage = "SetState";
        private const string errorMessage = "Error";
        private const string warningMessage = "Warning";

        private int mGain = 0;
        private int mFrequency = 0;
        private int mVolume = 0;
        private int lastGain = -1;
        private int lastFrequency = -1;
        private int lastVolume = -1;

        private void SetState(States newState)
        {
            state = newState;

            if (OnCBleVirtDeviceCallback != null)
            {
                OnCBleVirtDeviceCallback(state);
            }
        }

        /// <summary>
        /// Returns the current state of CBleVirtDevice.
        /// </summary>
        public States GetState()
        {
            return state;
        }

        private void Start()
        {
            // Required when using Bluetooth functionality of Android
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                Permission.RequestUserPermission(Permission.FineLocation);

            if (androidInstance == null)
            {
                AndroidJavaClass javaClass = new AndroidJavaClass("com.cyberith.unityandroidblevirtdevicelib.AndroidBLEVirtDevice");
                androidInstance = javaClass.CallStatic<AndroidJavaObject>("getInstance");
            }

            if (androidInstance != null)
            {
                // The name of the GameObject that this script component is attached to is used to send messages from the Android instance
                // In case, the name is changed by the developer the Android instance will still have the correct name
                string parentGameObjectName = gameObject.name;
                androidInstance.Call("Initialize", parentGameObjectName);

                string savedBluetoothDeviceName = CDataManager.ReadFile();
                if (savedBluetoothDeviceName != null)
                    savedVirtSamName = savedBluetoothDeviceName;

                ConnectToSavedVirt();
            } else
            {
                CLogger.LogError("Android instance in CBleVirtDevice is null");
            }
        }


        /// <summary>
        /// Sets the state to ACTIVE. Should be called when the BLE connection process is finished and, if required, calibration was done.
        /// </summary>
        public void FinishSetup()
        {
            SetState(States.ACTIVE);
        }

        /// <summary>
        /// Returns the list of bluetooth devices that the HMD has been paired to.
        /// </summary>
        public string[] GetPairedDevices()
        {
            return pairedDevices;
        }

        /// <summary>
        /// Returns the Bluetooth name of the last connected Virtualizer.
        /// </summary>
        public string GetSavedVirtSamName()
        {
            return savedVirtSamName;
        }

        /// <summary>
        /// Saves the name and tries (asynchronously) to establish an BLE connection using that name.
        /// </summary>
        public void ConnectToAndSave(string name)
        {
            savedVirtSamName = name;
            CDataManager.WriteToFile(name);
            androidInstance.Call<string>("FindAndConnect", name);
        }

        /// <summary>
        /// Tries (asynchronously) to establish a BLE connection using the saved bluetooth name.
        /// </summary>
        public void ConnectToSavedVirt()
        {
            // A string is returned that contains all paired devices that were checked, plus the result at the beginning
            // The Android function stops either when it has checked all paired devices, or when it has found the corresponding name
            string pairedDevicesStr = androidInstance.Call<string>("FindAndConnect", savedVirtSamName);
            char[] delim = new char[] { '~' };
            pairedDevices = pairedDevicesStr.Split(delim);

            if (pairedDevices[0] == "false") // Did not find the paired virtualizer, so the user should select
                SetState(States.SELECTING);
        }

        // Callback function for Android messages
        private void OnAndroidMessage(string message)
        {
            if (message != null)
            {
                char[] delim = new char[] { '~' };
                string[] parts = message.Split(delim);

                if (message.Length >= setStateMessage.Length && message.Substring(0, setStateMessage.Length) == setStateMessage)
                {
                    Enum.TryParse(parts[1], out States newState);
                    SetState(newState);
                } else if (message.Length >= errorMessage.Length && message.Substring(0, errorMessage.Length) == errorMessage)
                {
                    CLogger.LogError("[Android BLE] " + parts[1]);
                }
                else if (message.Length >= warningMessage.Length && message.Substring(0, warningMessage.Length) == warningMessage)
                {
                    CLogger.LogWarning("[Android BLE] " + parts[1]);
                }
            }
        }
        
        private void Update()
        {
            if (state == States.ACTIVE)
            {
                // Reduces the number of BLE operations by only setting the haptic values once every period and when one of the values are changed
                lastHapticTime += Time.deltaTime;
                if (mGain != lastGain || mFrequency != lastFrequency || mVolume != lastVolume)
                {
                    if (lastHapticTime >= hapticPeriod)
                    {
                        androidInstance.Call("HapticSetValues", mGain, mFrequency, mVolume);
                        lastHapticTime = 0;
                        lastGain = mGain;
                        lastFrequency = mFrequency;
                        lastVolume = mVolume;
                    }
                }
            }
        }

        // Closing Bluetooth Gatt Object in the Android instance when the app is exited
        private void OnApplicationQuit()
        {
            androidInstance.Call("OnApplicationQuit");
        }
    }
}