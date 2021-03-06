using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorScroller : MonoBehaviour {

    Material material;
    [SerializeField] float scrollRate = 0.01f;


    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }


    void Update()
    {
        Vector2 newOffset = material.mainTextureOffset;
        newOffset.y += scrollRate;
        GetComponent<MeshRenderer>().material.mainTextureOffset = newOffset;
    }
}
