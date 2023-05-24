using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnSystem : MonoBehaviour
{
    public static RespawnSystem Instance { get; private set; }
    private CVirtPlayerController playerController;
    public List<Checkpoint> checkpoints = new List<Checkpoint>();


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public Checkpoint GetCheckpoint(int index)
    {
        if (index < 0 || index >= checkpoints.Count)
        {
            return null;
        }
        return checkpoints[index];
    }

    void Start()
    {
        // Find the GameObject that has the player controller script attached to it
        GameObject playerObject = GameObject.FindWithTag("Player");

        // Get the player controller component from the player GameObject
        playerController = playerObject.GetComponent<CVirtPlayerController>();

        // Find all GameObjects with the "Checkpoint" tag and add them to the checkpoints list
        GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("Checkpoint");
        foreach (GameObject checkpointObject in checkpointObjects)
        {
            Checkpoint checkpoint = checkpointObject.GetComponent<Checkpoint>();
            if (checkpoint != null)
            {
                checkpoints.Add(checkpoint);
            }
            else
            {
                Debug.LogWarning("GameObject with Checkpoint tag does not have a Checkpoint component!");
            }
        }
    }

    void Update()
    {
        if (playerController == null)
        {
            Debug.LogError("Player controller reference is null!");
            return;
        }  
    }
}



