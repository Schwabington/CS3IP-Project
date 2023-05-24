using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    private bool hasStarted = false;
    private bool hasFinished = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("StartLine"))
        {
            // Player has passed the StartLine
            hasStarted = true;
            Debug.Log("Player has passed the start line");
        }
        else if (other.gameObject.CompareTag("FinishLine"))
        {
            // Player has passed the FinishLine
            hasFinished = true;
            Debug.Log("Player has passed the finish line");
        }
    }

    public bool HasStarted()
    {
        return hasStarted;
    }

    public bool HasFinished()
    {
        return hasFinished;
    }
}
