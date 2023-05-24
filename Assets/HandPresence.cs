using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandPresence : MonoBehaviour 
{ 
    public bool showController = false; 
    public InputDeviceCharacteristics controllerCharacteristics; 
    public List<GameObject> controllerPrefabs; 
    private InputDevice targetDevice; 
    private GameObject spawnedController; 

    // Start is called before the first frame update
    void Start() 
    { 
        TryInitialize(); 
    } 
    void TryInitialize() 
    { 
        List<InputDevice> devices = new List<InputDevice>(); 
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices); 

        foreach (var item in devices) 
        { 
            Debug.Log(item.name + item.characteristics); 
        } 
        if (devices.Count > 0) 
        { 
            targetDevice = devices[0]; 
            GameObject prefab = controllerPrefabs.Find(controller => controller.name == targetDevice.name); 
            spawnedController = Instantiate(controllerPrefabs[0], transform); 
        } 
    } 

    // Update is called once per frame
    void Update() 
    { 
        if (!targetDevice.isValid) 
        { 
            TryInitialize(); 
        } 
        else 
        { 
            if (showController) 
            { 
                spawnedController.SetActive(true); 
            } 
            else 
            { 
                spawnedController.SetActive(false); 
            } 
        } 
    } 
}