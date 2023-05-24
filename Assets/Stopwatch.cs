using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Stopwatch : MonoBehaviour
{
    public GameObject startLine;
    public GameObject finishLine;
    public TextMeshProUGUI timeText;

    private float startTime;
    private float finalTime;

    private bool started = false;

    void Update()
    {
        if (started)
        {
            float currentTime = Time.time - startTime;
            timeText.text = "Time: " + currentTime.ToString("F2");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("StartLine"))
        {
            Debug.Log("Player entered start line trigger");
            started = true;
            startTime = Time.time;
        }

        if (other.gameObject.CompareTag("FinishLine"))
        {
            started = false;
            finalTime = Time.time - startTime;
            timeText.text = "Final Time: " + finalTime.ToString("F2");
            Invoke("HideTimeText", 5f);
        }
    }

    void HideTimeText()
    {
        timeText.text = "";
    }
}
