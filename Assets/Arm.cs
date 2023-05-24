using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MiniGame1 miniGame = FindObjectOfType<MiniGame1>();
            if (miniGame != null)
            {
                miniGame.RespawnPlayer();
            }
            else
            {
                Debug.LogError("MiniGame1 not found!");
            }
        }         
    }
}
