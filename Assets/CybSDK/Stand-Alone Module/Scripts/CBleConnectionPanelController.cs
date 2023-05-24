using UnityEngine;
using UnityEngine.UI;
using CybSDK;

    [RequireComponent(typeof(CBleVirtDevice))]
    public class CBleConnectionPanelController : MonoBehaviour
    {
        public Button PairedDeviceButtonPrefab;

        private GameObject ConnectionObjectsHolder;
        private GameObject CalibrationObjectsHolder;
        private GameObject SelectionObjectsHolder;
        private GameObject ConnectionFailedObjectsHolder;
        private Button ReconnectBtn;

        private CBleVirtDevice virtDevice;

        private Text StatusText;
        private Text SelectionTitleText;
        private Text ConnectionTitleText;
        private Text ConnectionFailedTitleText;

        private string savedVirtSamName;

        private bool _calibrationRequired = false;

        private void Awake()
        {
            virtDevice = GetComponent<CBleVirtDevice>();
            virtDevice.OnCBleVirtDeviceCallback += OnCBleVirtDeviceCallback;

            ConnectionObjectsHolder = gameObject.transform.Find("ConnectionObjectsHolder").gameObject;
            CalibrationObjectsHolder = gameObject.transform.Find("CalibrationInstructionsObjectsHolder").gameObject;
            SelectionObjectsHolder = gameObject.transform.Find("SelectionObjectsHolder").gameObject;
            ConnectionFailedObjectsHolder = gameObject.transform.Find("ConnectionFailedObjectsHolder").gameObject;
            ReconnectBtn = ConnectionFailedObjectsHolder.transform.Find("ReconnectBtn").GetComponent<Button>();
            ReconnectBtn.onClick.AddListener(() => virtDevice.ConnectToSavedVirt());

            StatusText = ConnectionObjectsHolder.transform.Find("StatusText").GetComponent<Text>();
            SelectionTitleText = SelectionObjectsHolder.transform.Find("TitleText").GetComponent<Text>();
            ConnectionTitleText = ConnectionObjectsHolder.transform.Find("TitleText").GetComponent<Text>();
            ConnectionFailedTitleText = ConnectionFailedObjectsHolder.transform.Find("TitleText").GetComponent<Text>();
        }

        private void Start()
        {
            ConnectionObjectsHolder.SetActive(false);
            CalibrationObjectsHolder.SetActive(false);
            SelectionObjectsHolder.SetActive(false);
            ConnectionFailedObjectsHolder.SetActive(true);
        }

        private void OnCBleVirtDeviceCallback(CBleVirtDevice.States state)
        {
            switch (state)
            {
                case CBleVirtDevice.States.UNKNOWN:
                    StatusText.text = "Unknown";
                    break;
                case CBleVirtDevice.States.SELECTING:
                    // If the saved Virtualizer bluetooth name is not found in the list of paired devices,
                    // the user can select a new name from the list of paired bluetooth devices.
                    ConnectionObjectsHolder.SetActive(false);
                    CalibrationObjectsHolder.SetActive(false);
                    SelectionObjectsHolder.SetActive(true);
                    ConnectionFailedObjectsHolder.SetActive(false);

                    savedVirtSamName = virtDevice.GetSavedVirtSamName();
                    string[] pairedDevices = virtDevice.GetPairedDevices();
                    if (pairedDevices.Length == 1)
                    {
                        SelectionTitleText.text = "This headset is not paired with any device. Please, pair first with the Virtualizer and restart this app";
                    }
                    else
                    {
                        SelectionTitleText.text = "The VirtSAM with the name '" + savedVirtSamName + "' could not be found in your paired devices. Please, select another device or pair again:";
                    }

                    // Generate the interactable list of paired bluetooth devices
                    for (int i = 1; i < pairedDevices.Length; i++)
                    {
                        Button newPairedDeviceButton = Instantiate(PairedDeviceButtonPrefab) as Button;
                        newPairedDeviceButton.transform.SetParent(SelectionObjectsHolder.transform, false);
                        newPairedDeviceButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 50 - (i - 1) * 70, 0);
                        newPairedDeviceButton.GetComponentInChildren<Text>().text = pairedDevices[i];
                        int index = i;
                        newPairedDeviceButton.onClick.AddListener(delegate { virtDevice.ConnectToAndSave(pairedDevices[index]); });
                    }
                    break;
                case CBleVirtDevice.States.CONNECTING:
                    ConnectionObjectsHolder.SetActive(true);
                    CalibrationObjectsHolder.SetActive(false);
                    SelectionObjectsHolder.SetActive(false);
                    ConnectionFailedObjectsHolder.SetActive(false);
                    StatusText.text = "Connecting";

                    savedVirtSamName = virtDevice.GetSavedVirtSamName();
                    ConnectionTitleText.text = "Setting up Virtualizer connection with '" + savedVirtSamName + "', please wait...";
                    break;
                case CBleVirtDevice.States.CONNECTION_FAILED:
                    gameObject.SetActive(true);
                    ConnectionObjectsHolder.SetActive(false);
                    CalibrationObjectsHolder.SetActive(false);
                    SelectionObjectsHolder.SetActive(false);
                    ConnectionFailedObjectsHolder.SetActive(true);
                    ConnectionFailedTitleText.text = "Connection with '" + savedVirtSamName + "' failed!";
                    break;
                case CBleVirtDevice.States.CONNECTED:
                    StatusText.text = "Connected";
                    break;
                case CBleVirtDevice.States.INITIALIZING_DATA:
                    // The HMD is retrieving and subscribing to the data from the Virtualizer connected by Bluetooth.
                    StatusText.text = "Initializing Data";
                    break;
                case CBleVirtDevice.States.CALIBRATING:
                    StatusText.text = "Setup Successful";
                    ConnectionObjectsHolder.SetActive(false);
                    SelectionObjectsHolder.SetActive(false);
                    ConnectionFailedObjectsHolder.SetActive(false);
                    if (_calibrationRequired)
                        CalibrationObjectsHolder.SetActive(true);
                    else // If not required, the calibration state can be skipped
                        FinishSetup();
                    break;
            }
        }

        /// <summary>
        /// Sets if calibration is required after a successful connection has been made to the Virtualizer.
        /// </summary>
        public void SetCalibrationRequired(bool calibrationRequired)
        {
            _calibrationRequired = calibrationRequired;
        }

        /// <summary>
        /// Calibrates the forward direction of the HMD by resetting the player orientation.
        /// Only needed when the HMD can autonomously redefine the forward direction without conscious user/developer input (e.g., Oculus Quest) or calibration needs to be done during run-time.
        /// </summary>
        public void Calibrate()
        {
            if (virtDevice == null)
                return;
            if (virtDevice.GetState() == CBleVirtDevice.States.CALIBRATING)
            {
                FinishSetup();
                virtDevice.ResetPlayerOrientation();
            } // If calibration was not done in a correct manner, or if the forward direction was (accidentally) redefined, it is possible to recalibrate
            else if (virtDevice.GetState() == CBleVirtDevice.States.ACTIVE)
                virtDevice.ResetPlayerOrientation();
            CLogger.Log("Recalibrated");
        }

        private void FinishSetup()
        {
            gameObject.GetComponent<Canvas>().enabled = false;
            virtDevice.FinishSetup();
        }
    }
