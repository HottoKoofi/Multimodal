using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class MandoR : MonoBehaviour
{
    public XRNode mandoR;
    public GameObject pivote;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(mandoR);
        transform.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(mandoR);
        if (Input.GetAxis("TriggerLeft") != 0) {
            Impulso(Input.GetAxis("TriggerLeft"));
        }
    }

    void Impulso(double acceleration) {
        Rigidbody rb = pivote.GetComponent<Rigidbody>();
        rb.AddForce(0, (float)acceleration * 10000 ,0,ForceMode.Force);
        Debug.Log("Aceleracion: "+(float)acceleration * 100);
        //Debug.Log(rb);
    }
}
