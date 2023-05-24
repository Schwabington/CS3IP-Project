using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;

    // Start is called before the first frame update
    void Start()
    {
        checkpointIndex = transform.GetSiblingIndex();
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Record the index of the last checkpoint passed by the player
            CVirtPlayerController player = other.GetComponent<CVirtPlayerController>();
            player.lastCheckpointIndex = checkpointIndex;
        }
    }
}
