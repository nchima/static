using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResShifter : MonoBehaviour {

    RenderTexture rendTex;
    RenderTexture newTex;

    Vector2 originalRes;

    FloatRange greatestResDifferenceRange = new FloatRange(0f, 1f);
    FloatRange baseResDifferenceRange = new FloatRange(0f, 0.1f);
    FloatRange currentResDifferenceRange = new FloatRange(0f, 0f);
    float returnToMinimumSpeed = 0.01f;

    [SerializeField] Material rendTexMaterial;

	void Start ()
    {
        rendTex = GetComponent<Camera>().targetTexture;

        originalRes = new Vector2(rendTex.width, rendTex.height);
	}
	
	void Update ()
    {
        currentResDifferenceRange = new FloatRange(
            Mathf.Clamp(currentResDifferenceRange.min, greatestResDifferenceRange.min, greatestResDifferenceRange.max),
            Mathf.Clamp(currentResDifferenceRange.max, greatestResDifferenceRange.min, greatestResDifferenceRange.max));

        //rendTex.width = originalRes.x + Mathf.RoundToInt(Random.Range(-currentResDifference, currentResDifference));
        //rendTex.height = originalRes.y + Mathf.RoundToInt(Random.Range(-currentResDifference, currentResDifference));

        if (Mathf.Abs(currentResDifferenceRange.min - baseResDifferenceRange.min) > 0.01f)
        {
            currentResDifferenceRange.min = Mathf.Lerp(currentResDifferenceRange.min, baseResDifferenceRange.min, returnToMinimumSpeed);
            currentResDifferenceRange.max = Mathf.Lerp(currentResDifferenceRange.max, baseResDifferenceRange.max, returnToMinimumSpeed);
        }

        else
        {
            currentResDifferenceRange.min = baseResDifferenceRange.min;
            currentResDifferenceRange.max = baseResDifferenceRange.max;
        }

        float rand = currentResDifferenceRange.Random;

        newTex = new RenderTexture(
            Mathf.RoundToInt(originalRes.x - originalRes.x * rand),
            Mathf.RoundToInt(originalRes.y - originalRes.y * rand),
            16);

        //Debug.Log(currentResDifferenceRange.min + ", " + currentResDifferenceRange.max);
        //Debug.Log(newTex.width + ", " + newTex.height);

        newTex.filterMode = FilterMode.Point;
        newTex.anisoLevel = 0;
        newTex.antiAliasing = 1;

        if (rendTex != null) rendTex.Release();
        GetComponent<Camera>().targetTexture = newTex;
        rendTexMaterial.mainTexture = newTex;
    }

    public void IncreaseResShift(float increaseAmt)
    {
        currentResDifferenceRange.min += increaseAmt;
        currentResDifferenceRange.max = currentResDifferenceRange.min * 1.5f;
    }
}
