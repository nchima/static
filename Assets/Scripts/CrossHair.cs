using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour {

    //private void Update()
    //{
    //    GetComponent<MeshRenderer>().material.mainTexture = GameManager.instance.noiseGenerator.noiseTex;

    //    float newScale = MyMath.Map(GameManager.instance.currentSine, -1f, 1f, 3f, 0.6696f);
    //    transform.localScale = new Vector3(newScale, transform.localScale.y, newScale);
    //}

    public int segments;
    [SerializeField] float maxRadius = 1.25f;
    [SerializeField] float minRadius = 0.34f;
    float xradius;
    float yradius;
    LineRenderer line;


    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();

        line.positionCount = segments + 1;
        line.useWorldSpace = false;
        CreatePoints();
    }


    private void Update()
    {
        xradius = MyMath.Map(GameManager.instance.currentSine, -1f, 1f, maxRadius, minRadius);
        yradius = MyMath.Map(GameManager.instance.currentSine, -1f, 1f, maxRadius, minRadius);
        transform.Rotate(new Vector3(0f, Random.Range(-180f, 180f), 0f));
        CreatePoints();
    }


    void CreatePoints()
    {
        float x;
        float y;
        float z = 0f;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            y = 0f;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            line.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / segments);
        }
    }

}
