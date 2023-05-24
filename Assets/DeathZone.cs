using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private bool playerIsDead = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerIsDead)
        {
            MiniGame1 miniGame = FindObjectOfType<MiniGame1>();
            if (miniGame != null)
            {
                miniGame.RespawnPlayer();
                playerIsDead = true;
            }
            else
            {
                Debug.LogError("MiniGame1 not found!");
            }
        }         
    }

    public void ResetPlayerIsDead()
    {
        playerIsDead = false;
    }
}
