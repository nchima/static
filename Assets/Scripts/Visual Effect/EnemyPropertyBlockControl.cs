using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPropertyBlockControl : PropertyBlockControl {

    public Color MainColor = Color.black;
    public Color EmissionColor = Color.black;
    public float Explode = 0f;

    void LateUpdate() {
        // Get the current value of the material properties in the renderer.
        foreach (Renderer _renderer in _renderers) {
            _renderer.GetPropertyBlock(_propBlock);

            // Assign our new values. For this demo, I'm just looping the explode value from 0 to 1 over time, and oscillating the colors using a Sin wave. You can do whatever.
            //_propBlock.SetColor("_Color", Color.Lerp(Color.red, Color.yellow, (Mathf.Sin(Time.time * 2f + _seed) + 1) / 2f));
            //_propBlock.SetColor("_Emissive", Color.Lerp(Color.green, Color.magenta, (Mathf.Sin(Time.time * 3f + _seed) + 1) / 2f));
            //_propBlock.SetFloat("_Explode", Mathf.Clamp01(Mathf.Repeat(Time.time * 0.2f + _seed, 1.5f) - 0.5f));

            // Apply the edited values to the renderer.
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}
