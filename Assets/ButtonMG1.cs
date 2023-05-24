using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonMG1 : MonoBehaviour
{
    public GameObject SpinnyVortex;
    public GameObject Canons;

    void Start()
    {
        SpinnyVortex.SetActive(false);
        Canons.SetActive(false);
    }
    
    void OnMouseDown()
    {
        SpinnyVortex.SetActive(true);
        Canons.SetActive(true);
    }
}
