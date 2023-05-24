using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CybSDK
{
    public class Bullet : MonoBehaviour
    {
        [HideInInspector]
        public PlayerHitHapticEmitter hapticPlayerHitScript;

        private Collider playerHitCollider;

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
                
                Destroy(gameObject);
            }         
        }
    }
}

