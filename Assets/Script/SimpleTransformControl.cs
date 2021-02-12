#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;

public class SimpleTransformControl : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float lerpValue = 0f;
    [SerializeField] private AnimationCurve behaviorCurve = default;
    [SerializeField] private Transform pointA = default;
    [SerializeField] private Transform pointB = default;

    [SerializeField] private Rigidbody platform = default;

    private float lastLerpValue = default;

    private void FixedUpdate()
    {
        if (Math.Abs(lastLerpValue - lerpValue) > .001f)
        {
            platform.MovePosition(Vector3.LerpUnclamped(pointA.position, pointB.position, behaviorCurve.Evaluate(lerpValue)));
            platform.MoveRotation(Quaternion.LerpUnclamped(pointA.rotation, pointB.rotation, behaviorCurve.Evaluate(lerpValue)));
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.blue;

        if (pointA != null)
        {
            Handles.DrawSolidDisc(pointA.position, pointA.up, .5f);
            Handles.DrawWireDisc(pointA.position, pointA.up, 1f);
            Handles.ArrowHandleCap(0, pointA.position, Quaternion.LookRotation(pointA.up), 1f, EventType.Repaint);
        }
           
        if (pointB != null)
        {
            Handles.DrawSolidDisc(pointB.position, pointB.up, .5f);
            Handles.DrawWireDisc(pointB.position, pointB.up, 1f);
            Handles.ArrowHandleCap(0, pointB.position, Quaternion.LookRotation(pointB.up), 1f, EventType.Repaint);
        }
        
        Handles.color = Color.red;
        
        if (pointA != null && pointB != null)
        {
            Handles.DrawLine(pointA.position, pointB.position);

            if (platform != null && behaviorCurve != null)
            {
                Handles.DrawWireDisc(Vector3.LerpUnclamped(pointA.position, pointB.position, behaviorCurve.Evaluate(lerpValue)), Vector3.LerpUnclamped(pointA.up, pointB.up, behaviorCurve.Evaluate(lerpValue)), .5f);
                var pos = Vector3.LerpUnclamped(pointA.position, pointB.position, behaviorCurve.Evaluate(lerpValue));
                var quat = Quaternion.LookRotation(Vector3.LerpUnclamped(pointA.up, pointB.up, behaviorCurve.Evaluate(lerpValue)));
                Handles.ArrowHandleCap(0, pos, quat, 1f, EventType.Repaint);
                //Handles.DrawLine(Vector3.LerpUnclamped(pointA.position, pointB.position, behaviorCurve.Evaluate(lerpValue)), Vector3.LerpUnclamped(pointA.position, pointB.position, behaviorCurve.Evaluate(lerpValue)) + Vector3.LerpUnclamped(pointA.up, pointB.up, behaviorCurve.Evaluate(lerpValue)));
            }
        }
    }
#endif
    
}
