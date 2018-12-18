using UnityEngine;
using System.Collections;

public class TextureOffsetChanger : MonoBehaviour {

	Material mat;
	public float relativeSpeed = 0.01f;
    public bool timeIndependent = false;
    [SerializeField] bool useMouseMovement = true;

	void Start() {
		mat = GetComponent<MeshRenderer> ().material;
	}

	void Update () {
        //if (GetComponent<OffsetPropertyBlockControl>()) {

        if (!timeIndependent) {
            float input = Input.GetAxis("Mouse X");
            if (InputManager.inputMode == InputManager.InputMode.Controller && (!Services.gameManager.isGameStarted || GameManager.isGamePaused)) {
                input = InputManager.movementAxis.y;
            }
            mat.mainTextureOffset = new Vector2(
                mat.mainTextureOffset.x + MyMath.Map(input * MyMath.BoolToInt(useMouseMovement), -1f, 1f, -relativeSpeed, relativeSpeed),
                MyMath.Map(Mathf.Sin(Time.time), -1, 1, -20, 20)
            );
        }
        //}
        else {
            mat.mainTextureOffset = new Vector2(Random.Range(-20f, 20f), Random.Range(-20f, 20f));
        }
	}
}
