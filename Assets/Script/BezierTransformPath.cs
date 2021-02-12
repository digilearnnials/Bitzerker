using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BezierTransformPath : MonoBehaviour
{
    [SerializeField, Range(8, 64)] private int resolution = 8;
    [SerializeField, Range(0, 1)] private float lerpValue = 0f;
    
    public Transform anchor0 = default;
    public Transform anchor1 = default;

    private void OnValidate()
    {
        if (anchor0 == null)
        {
            anchor0 = new GameObject("Anchor0").transform;
            anchor0.position = transform.position;
            anchor0.rotation = transform.rotation;
            anchor0.parent = transform;
        }
        
        if (anchor1 == null)
        {
            anchor1 = new GameObject("Anchor1").transform;
            anchor1.position = transform.position + transform.forward;
            anchor1.rotation = transform.rotation;
            anchor1.parent = transform;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (anchor0 != null && anchor1 != null)
        {
            Vector3[] points = new Vector3[resolution];
            
            for (int i = 0; i < points.Length; i++)
            {
                var t = (float)i / (points.Length - 1);
                points[i] = Vector3.LerpUnclamped(anchor0.position, anchor1.position, t);
                var pForward = Vector3.LerpUnclamped(anchor0.forward, anchor1.forward, t);
                var pUp = Vector3.LerpUnclamped(anchor0.up, anchor1.up, t);

                Handles.color = Handles.yAxisColor;
                Handles.ArrowHandleCap(0, points[i], Quaternion.LookRotation(pUp), .5f, EventType.Repaint);
                Handles.color = Handles.zAxisColor;
                Handles.ArrowHandleCap(0, points[i], Quaternion.LookRotation(pForward), .5f, EventType.Repaint);
                
                Handles.DrawWireDisc(points[i], pUp, .25f);
            }
            
            Handles.color = Color.yellow;
            Handles.DrawPolyLine(points);
            
            Handles.color = Color.red;
            Handles.DrawWireDisc(Vector3.LerpUnclamped(anchor0.position, anchor1.position, lerpValue), Vector3.LerpUnclamped(anchor0.up, anchor1.up, lerpValue), .25f);
            
            Handles.color = Handles.yAxisColor;

            var pos = Vector3.LerpUnclamped(anchor0.position, anchor1.position, lerpValue);
            var upQuat = Quaternion.LookRotation(Vector3.LerpUnclamped(anchor0.up, anchor1.up, lerpValue));
            
            var forwardQuat = Quaternion.LookRotation(Vector3.LerpUnclamped(anchor0.forward, anchor1.forward, lerpValue));
            
            Handles.ArrowHandleCap(0, pos, upQuat, 1f, EventType.Repaint);
            
            Handles.color = Handles.zAxisColor;

            Handles.ArrowHandleCap(0, pos, forwardQuat, 1f, EventType.Repaint);
        }
        
        if (anchor0 != null)
        {
            Handles.color = Color.blue;
            Handles.DrawSolidDisc(anchor0.position, anchor0.up, .25f);
            Handles.DrawWireDisc(anchor0.position, anchor0.up, .3f);
            
            Handles.color = Handles.yAxisColor;
            Handles.ArrowHandleCap(0, anchor0.position, Quaternion.LookRotation(anchor0.up), 1f, EventType.Repaint);
            Handles.color = Handles.zAxisColor;
            Handles.ArrowHandleCap(0, anchor0.position, Quaternion.LookRotation(anchor0.forward), 1f, EventType.Repaint);
        }
        
        if (anchor1 != null)
        {
            Handles.color = Color.blue;
            Handles.DrawSolidDisc(anchor1.position, anchor1.up, .25f);
            Handles.DrawWireDisc(anchor1.position, anchor1.up, .3f);
            
            Handles.color = Handles.yAxisColor;
            Handles.ArrowHandleCap(0, anchor1.position, Quaternion.LookRotation(anchor1.up), 1f, EventType.Repaint);
            Handles.color = Handles.zAxisColor;
            Handles.ArrowHandleCap(0, anchor1.position, Quaternion.LookRotation(anchor1.forward), 1f, EventType.Repaint);
        }
    }
#endif
}
