using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame1 : MonoBehaviour
{
    public void RespawnPlayer()
    {
        CVirtPlayerController playerController = FindObjectOfType<CVirtPlayerController>();
        if (playerController != null)
        {
            playerController.RespawnPlayer();
            DeathZone deathZone = FindObjectOfType<DeathZone>();
            if (deathZone != null)
            {
                deathZone.ResetPlayerIsDead();
            }
        }
        else
        {
            Debug.LogError("CVirtPlayerController not found!");
        }
    }
}
