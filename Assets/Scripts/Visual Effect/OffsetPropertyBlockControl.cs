using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetPropertyBlockControl : PropertyBlockControl {

    [HideInInspector] public Vector2 Offset = Vector2.zero;

    void LateUpdate() {
        // Get the current value of the material properties in the renderer.
        foreach (Renderer _renderer in _renderers) {
            _renderer.GetPropertyBlock(_propBlock);

            _renderer.material.SetVector("_TextureOffset", new Vector4(Offset.x, Offset.y, 0f, 0f));

            // Apply the edited values to the renderer.
            _renderer.SetPropertyBlock(_propBlock);
        }
    }

}
