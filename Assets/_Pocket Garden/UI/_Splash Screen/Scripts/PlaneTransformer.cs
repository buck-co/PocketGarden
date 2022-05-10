using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneTransformer : MonoBehaviour
{
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        var scale = transform.localScale;
        scale.z = scale.x * cam.aspect;
        transform.localScale = scale;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
