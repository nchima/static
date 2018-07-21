using UnityEngine;
using System.Collections;

public class MyMath {

	public static float Map(float x, float in_min, float in_max, float out_min, float out_max) {
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}

    public static float Wrap(float x, float min, float max) {
        return (((x - min) % (max - min)) + (max - min)) % (max - min) + min;
    }

    public static float Wrap01(float x) {
        return Wrap(x, 0f, 1f);
    }

    public static float Either1orNegative1 {
        get {
            float rand = Random.value;
            if (rand < 0.5f) { return -1f; } else { return 1f; }
        }
    }

    public static int BoolToInt(bool input) {
        if (input) { return 1; }
        else { return 0; }
    }
}
