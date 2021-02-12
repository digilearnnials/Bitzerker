#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public class TransformInterpolator : MonoBehaviour {

    [SerializeField] Rigidbody body = default;
	
    [SerializeField] Transform from = default, to = default;
	
    public void Interpolate (float t) {
        body.MovePosition(Vector3.LerpUnclamped(from.position, to.position, t));
        body.MoveRotation(Quaternion.LerpUnclamped(from.rotation, to.rotation, t));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (from)
        {
            Handles.color = Color.blue;
            Handles.DrawWireDisc(from.position, from.up, .5f);
            Handles.ArrowHandleCap(0, from.position, Quaternion.LookRotation(from.forward), 1f, EventType.Repaint);
            
            Handles.color = Color.green;
            Handles.ArrowHandleCap(0, from.position, Quaternion.LookRotation(from.up), 1f, EventType.Repaint);
        }
        
        if (to)
        {
            Handles.color = Color.blue;
            Handles.DrawWireDisc(to.position, to.up, .5f);
            Handles.ArrowHandleCap(0, to.position, Quaternion.LookRotation(to.forward), 1f, EventType.Repaint);
            
            Handles.color = Color.green;
            Handles.ArrowHandleCap(0, to.position, Quaternion.LookRotation(to.up), 1f, EventType.Repaint);
        }

        if (from && to)
        {
            Handles.DrawDottedLine(from.position, to.position, 5f);
        }
    }
#endif
}
