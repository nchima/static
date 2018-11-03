using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletTrail : MonoBehaviour {

    float lingerDuration = 0.4f;
    float lingerTimer = 0f;

    static ObjectPooler meshPooler;
    List<Segment> segments = new List<Segment>();
    GameObject trailPiece;

    public float thickness;
    public Color m_Color;



    private void Awake() {
        // Get a reference to the bullet mesh pooler.
        if (meshPooler == null) {
            if (!GameObject.Find("Pooled Bullet Meshes").GetComponent<ObjectPooler>()) {
                Debug.LogError("Could not find bullet mesh pooler!");
                Debug.Break();
                return;
            } else {
                meshPooler = GameObject.Find("Pooled Bullet Meshes").GetComponent<ObjectPooler>();
            }
        }

        // Grab a trail piece from the pooler.
        if (trailPiece == null) trailPiece = meshPooler.GrabObject();
    }


    private void Update() {
        // Check to see if any segments need to be deleted.
        for (int i = 0; i < segments.Count; i++) {
            if (Time.time >= segments[i].deleteTime) {
                segments.Remove(segments[i]);
                RedrawTrail();
            }
        }
    }


    void RedrawTrail() {
        if (segments.Count == 0) {
            segments.Clear();
            trailPiece.SetActive(false);
            return;
        }

        trailPiece.GetComponent<PlayerBulletMesh>().SetTransformByEndPoints(segments[segments.Count-1].beginningPoint, segments[0].endPoint, thickness);
        trailPiece.GetComponent<PlayerBulletMesh>().SetColor(m_Color);
    }


    public void AddSegment(Vector3 beginning, Vector3 end, Vector3 velocity) {
        Debug.DrawRay(beginning, Vector3.up, Color.yellow);
        Debug.DrawRay(end, Vector3.up, Color.yellow);
        trailPiece.SetActive(true);
        trailPiece.GetComponent<PlayerBulletMesh>().screenTime = lingerDuration;
        segments.Add(new Segment(beginning, end, Time.time + lingerDuration));
        foreach(Segment segment in segments) {
            Debug.DrawLine(segment.beginningPoint, segment.endPoint, Color.red);
        }
        RedrawTrail();
    }


    class Segment {
        public Vector3 beginningPoint;
        public Vector3 endPoint;
        public float deleteTime;

        public Segment(Vector3 beginningPoint, Vector3 endPoint, float deleteTime) {
            this.beginningPoint = beginningPoint;
            this.endPoint = endPoint;
            this.deleteTime = deleteTime;
        }
    }
}
