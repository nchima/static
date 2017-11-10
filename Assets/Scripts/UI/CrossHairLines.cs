using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHairLines : MonoBehaviour {

	[SerializeField] GameObject leftLine, rightLine;
    [SerializeField] float MAX_GAP = 2f;
    [SerializeField] float MIN_GAP = 0.2f;
    [SerializeField] GameObject crosshairCircle;
    Color currentColor;
    [SerializeField] Color noTargetColor;
    [SerializeField] Color targetColor;
    public bool targetAllGoodAndStuff;


    private void Update()
    {
        if (targetAllGoodAndStuff)
        {
            currentColor = targetColor;
            //crosshairCircle.SetActive(true);
        }

        else
        {
            currentColor = noTargetColor;
            //crosshairCircle.SetActive(false);
        }

        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>()) mr.material.SetColor("_Tint", currentColor);
        crosshairCircle.GetComponent<MeshRenderer>().material.SetColor("_Tint", currentColor);
        leftLine.GetComponent<MeshRenderer>().material.mainTexture = GameManager.instance.noiseGenerator.noiseTex;
        rightLine.GetComponent<MeshRenderer>().material.mainTexture = GameManager.instance.noiseGenerator.noiseTex;
        float currentGap = MyMath.Map(GameManager.instance.currentSine, -1f, 1f, MAX_GAP, MIN_GAP);
        leftLine.transform.localPosition = new Vector3(-currentGap / 2, 0f, 0f);
        rightLine.transform.localPosition = new Vector3(currentGap / 2, 0f, 0f);
    }
}
