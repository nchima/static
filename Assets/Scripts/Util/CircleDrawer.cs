using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleDrawer {

    public static void Draw(LineRenderer lineRenderer, float xRadius, float yRadius, int segments, float jitter) {
        float x;
        float y;
        float z = 0f;

        float angle = 20f;

        if (lineRenderer.positionCount != segments + 1) { lineRenderer.positionCount = segments + 1; }

        for (int i = 0; i < (segments + 1); i++) {
            x = (Mathf.Sin(Mathf.Deg2Rad * angle) * xRadius) + Random.Range(-jitter, jitter);
            y = 0f;
            z = (Mathf.Cos(Mathf.Deg2Rad * angle) * yRadius) + Random.Range(-jitter, jitter);

            lineRenderer.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / segments);
        }
    }


    public static void DrawCrosshair(LineRenderer lineRenderer, float xRadius, float yRadius, int segments, float jitter) {
        float x;
        float y;
        float z = 0f;

        float angle = 20f;

        if (lineRenderer.positionCount != segments + 1) { lineRenderer.positionCount = segments + 1; }

        for (int i = 0; i < (segments + 1); i++) {
            float xDeath = 0;
            if (i % 2 == 0) { xDeath = GunValueManager.MapTo(0, xRadius); }
            x = (Mathf.Sin(Mathf.Deg2Rad * angle) * (xRadius - xDeath * Random.Range(0.9f, 1.1f)) + Random.Range(-jitter, jitter));
            y = 0f;
            z = (Mathf.Cos(Mathf.Deg2Rad * angle) * (yRadius - xDeath * Random.Range(0.9f, 1.1f)) + Random.Range(-jitter, jitter));

            lineRenderer.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / segments);
        }
    }
}
