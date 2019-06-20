using UnityEngine;
using System.Collections;
[RequireComponent(typeof(LineRenderer))]
public class Line : MonoBehaviour {

    public LineRenderer lr;
    public Transform p0;
    public Transform p1;
    public int layerOrder = 0;
    void Start() {
        lr.SetVertexCount(2);
        lr.sortingLayerID = layerOrder;
    }
    void Update() {
        lr.SetPosition(0,p0.position);
        lr.SetPosition(1,p1.position);
    }

}
