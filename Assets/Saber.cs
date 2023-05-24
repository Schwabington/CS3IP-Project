using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saber : MonoBehaviour
{
    public LayerMask layer;
    public Color saberColor;
    private Vector3 previousPos;
    private TriggerPlatform triggerPlatform;

    // Start is called before the first frame update
    void Start()
    {
        triggerPlatform = FindObjectOfType<TriggerPlatform>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, 1, layer))
        {
            if(Vector3.Angle(transform.position-previousPos, hit.transform.up)>130)
            {
                Cube cube = hit.transform.GetComponent<Cube>();
                if (cube && cube.cubeColor == saberColor)
                {
                    Destroy(hit.transform.gameObject);
                    triggerPlatform.AddHit();
                }
            }
        }
        previousPos = transform.position;
    }
}
