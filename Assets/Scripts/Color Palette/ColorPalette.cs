using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Color Palettes/Color Palette")]
public class ColorPalette : ScriptableObject {
    public Color wallColor;
    public Color floorColor;
    public Color obstacleColor;
    public Color perspectiveColor1 = Color.cyan;
    public Color perspectiveColor2 = Color.magenta;
    public Color perspectiveColor3 = Color.yellow;
    public Color orthoColor1 = Color.red;
    public Color orthoColor2 = Color.green;
    public Color orthoColor3 = Color.blue;
}
