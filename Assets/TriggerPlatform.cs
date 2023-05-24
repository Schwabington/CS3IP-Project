using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPlatform : MonoBehaviour
{
    public GameObject Spawner;
    public float spawnerDuration = 10f;
    private bool spawnerActivated = false;
    private int hitCount = 0;
    private int cubeCount = 0;

    void Start()
    {
        Spawner.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !spawnerActivated)
        {
            Debug.Log("Passed");
            spawnerActivated = true;
            StartCoroutine(ActivateSpawner());
        }
    }

    IEnumerator ActivateSpawner()
    {
        Debug.Log("Spawner activated");
        Spawner.SetActive(true);
        while (spawnerDuration > 0)
        {
            yield return new WaitForSeconds(1f);
            spawnerDuration--;
        }
        DeactivateSpawner();
    }

    void DeactivateSpawner()
    {
        Debug.Log("Spawner deactivated");
        Spawner.SetActive(false);
        Debug.Log("Cubes spawned: " + cubeCount);
        Debug.Log("Cubes hit: " + hitCount);
    }

    public void AddHit()
    {
        hitCount++;
    }

    public void AddCube()
    {
        cubeCount++;
    }
}