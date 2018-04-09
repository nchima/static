using UnityEngine;
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]

public class ScreenBlurHelper : MonoBehaviour {

    public Material DMmat;
    public float target;


    void Start() {
        this.GetComponent<Camera>().depthTextureMode = DepthTextureMode.MotionVectors;

    }


    private void Update() {
        //Shader.SetGlobalInt("_Button", Input.GetButton("Fire1") ? 1 : 0);
        //		if (Input.GetMouseButton (0)) {
        //			target = Mathf.Lerp (target, 2048, Time.deltaTime/3);
        //		} else {
        //			target = Mathf.Lerp (target, 0, Time.deltaTime);
        //
        //		}

        //Shader.SetGlobalInt ("_Button", target);

    }


    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(src, dest, DMmat);
    }
}