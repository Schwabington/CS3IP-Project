using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
     public float rotateSpeed; //set it in the  inspector

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update () {
        rotate();
    }
 
 
    void rotate() {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

}