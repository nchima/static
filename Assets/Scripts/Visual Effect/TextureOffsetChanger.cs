using UnityEngine;
using System.Collections;

public class TextureOffsetChanger : MonoBehaviour {

	Material mat;

	public float relativeSpeed = 0.01f;

    public bool timeIndependent = false;

	void Start() {
		mat = GetComponent<MeshRenderer> ().material;
	}

	void Update () {
        //if (GetComponent<OffsetPropertyBlockControl>()) {

        if (!timeIndependent) {
            mat.mainTextureOffset = new Vector2(
                mat.mainTextureOffset.x + MyMath.Map(Input.GetAxis("Mouse X"), -1f, 1f, -relativeSpeed, relativeSpeed),
                MyMath.Map(Mathf.Sin(Time.time), -1, 1, -20, 20)
            );
        }
        //}
        else {
            mat.mainTextureOffset = new Vector2(Random.Range(-20f, 20f), Random.Range(-20f, 20f));
        }
	}
}
