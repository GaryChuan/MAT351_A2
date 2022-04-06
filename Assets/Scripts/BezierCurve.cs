using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    [SerializeField] float _lineWidth;
    [SerializeField] List<Vector3> _points;

    void Start()
    {
        var lineRenderer = gameObject.GetComponent<LineRenderer>();

        for(int i = 0; i < _points.Count; ++i)
        {
            _points[i] = transform.TransformPoint(_points[i]);
        }

        lineRenderer.startWidth = _lineWidth;
        lineRenderer.endWidth = _lineWidth;
        lineRenderer.positionCount = _points.Count;
        lineRenderer.SetPositions(_points.ToArray());
    }

    void Update()
    {
        
    }
}
