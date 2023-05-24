/************************************************************************************

Filename    :   CVirtPlayerController.cs
Content     :   PlayerController takes input from a VirtDevice and moves the player respectively.
Created     :   August 8, 2014
Last Updated:	December 5, 2019
Authors     :   Lukas Pfeifhofer
				Stefan Radlwimmer

Copyright   :   Copyright 2019 Cyberith GmbH

Licensed under the AssetStore Free License and the AssetStore Commercial License respectively.

************************************************************************************/

// The underlying class of UVirtDevice is dependent on the platform used
#if UNITY_ANDROID || PLATFORM_ANDROID
using UVirtDevice = CybSDK.CBleVirtDevice;
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using UVirtDevice = CybSDK.IVirtDevice;
#endif

using UnityEngine;
using CybSDK;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CVirtDeviceController))]
public class CVirtPlayerController : MonoBehaviour
{
	private CVirtDeviceController deviceController;
	private CharacterController characterController;

	//[Tooltip("Reference to a GameObject that will be rotated according to the player�s orientation in the device. If not set will search for 'ForwardDirection' attached to GameObject.")];
	public Transform forwardDirection;

	[Tooltip("Movement Speed Multiplier, to fine tune the players speed.")]
	[Range(0, 10)]

	public float movementSpeedMultiplier = 1.2f;
	public int lastCheckpointIndex = -1; // Start with no checkpoints passed
	public Vector3 MotionVector { get; private set; }
	public AudioSource death;

	// Use this for initialization
	void Start()
	{
		// Find the forward direction        
		if (forwardDirection == null)
		{
			forwardDirection = transform.Find("ForwardDirection");
		}

		//Check if this object has a CVirtDeviceController attached
		deviceController = GetComponent<CVirtDeviceController>();
		if (deviceController == null)
		{
			CLogger.LogError(string.Format("CVirtPlayerController requires a CVirtDeviceController attached to gameobject '{0}'.", gameObject.name));
			enabled = false;
			return;
		}

		//Check if this object has a CharacterController attached
		characterController = GetComponent<CharacterController>();
		if (characterController == null)
		{
			CLogger.LogError(string.Format("CVirtPlayerController requires a CharacterController attached to gameobject '{0}'.", gameObject.name));
			enabled = false;
			return;
		}
	}

	// Update is called once per frame
	void Update()
	{
		UVirtDevice device = deviceController.GetDevice();

		if (device == null || !device.IsOpen())
			return;

		// MOVE
		///////////
		Vector3 movement = device.GetMovementVector() * movementSpeedMultiplier;

		// Check if the player is moving forward
		if (movement.z > 0)
		{
			// Check if the audio source is playing
			if (!GetComponent<AudioSource>().isPlaying)
			{
				// If the audio source is not playing, play it
				GetComponent<AudioSource>().Play();
			}
		}
		else
		{
			// If the player is not moving, pause the audio source
			GetComponent<AudioSource>().Pause();
		}

		// ROTATION
		///////////
		Quaternion localOrientation = device.GetPlayerOrientationQuaternion();

		// Determine global orientation for characterController Movement
		Quaternion globalOrientation;

		// For decoupled movement we do not rotate the pawn --> HMD does that
		if (deviceController.IsDecoupled())
		{
			if (forwardDirection != null)
			{
				forwardDirection.transform.localRotation = localOrientation;
				globalOrientation = forwardDirection.transform.rotation;
			}
			else
			{
				// Quaternions are applied right to left
				globalOrientation = localOrientation * gameObject.transform.rotation;
			}
		}
		// For coupled movement we rotate the pawn and HMD
		else
		{
			gameObject.transform.rotation = localOrientation;
			globalOrientation = localOrientation;
		}

		Vector3 motionVector = globalOrientation * movement;
		MotionVector = motionVector;
		characterController.SimpleMove(motionVector);

	}

void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            Debug.Log("Player entered trigger: " + other.gameObject.name);
			// Update the last checkpoint passed by the player
            Checkpoint checkpoint = other.GetComponent<Checkpoint>();
            lastCheckpointIndex = checkpoint.checkpointIndex;
        }
		else if (other.CompareTag("Bullet"))
        {
			RespawnPlayer();
		}
		else if (other.CompareTag("Arm"))
        {
			RespawnPlayer();
		}
		else if (other.CompareTag("DeathZone"))
        {
			RespawnPlayer();
		}
    }

	public void RespawnPlayer()
	{
		Debug.Log("Player Dead");
		death.Play();        

		if (lastCheckpointIndex != -1)
        {
            Checkpoint lastCheckpoint = FindObjectOfType<RespawnSystem>().GetCheckpoint(lastCheckpointIndex);
            if (lastCheckpoint != null)
            {
                // Set the player's position to the position of the last checkpoint passed
                transform.position = lastCheckpoint.transform.position;
            }
            else
            {
                Debug.LogError("Last checkpoint not found!");
            }
        }
        else
        {
            Debug.LogError("No checkpoints passed!");
        }
	}
}