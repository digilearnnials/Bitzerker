#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DetectionZone : MonoBehaviour
{
    [SerializeField] UnityEvent onFirstEnter = default, onLastExit = default;
    
    List<Collider> colliders = new List<Collider>();
    
    void Awake () {
        enabled = false;
    }
    
    void FixedUpdate () {
        for (int i = 0; i < colliders.Count; i++) {
            Collider collider = colliders[i];
            if (!collider || !collider.gameObject.activeInHierarchy) {
                colliders.RemoveAt(i--);
                if (colliders.Count == 0) {
                    onLastExit.Invoke();
                    enabled = false;
                }
            }
        }
    }
    
    void OnTriggerEnter (Collider other) {
        if (colliders.Count == 0) {
            onFirstEnter?.Invoke();
            enabled = true;
        }
        colliders.Add(other);
    }

    void OnTriggerExit (Collider other) {
        if (colliders.Remove(other) && colliders.Count == 0) {
            onLastExit?.Invoke();
            enabled = false;
        }
    }
    
    void OnDisable () {
#if UNITY_EDITOR
        if (enabled && gameObject.activeInHierarchy) {
            return;
        }
#endif
        if (colliders.Count > 0) {
            colliders.Clear();
            onLastExit.Invoke();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.richText = true;
        style.normal.textColor = Color.white;
        
        Handles.color = Color.green;
        Handles.SphereHandleCap(0, transform.position, Quaternion.identity, .5f, EventType.Repaint);
        
        if (onFirstEnter.GetPersistentEventCount() > 0)
        {
            Handles.color = Color.blue;
            
            for (int i = 0; i < onFirstEnter.GetPersistentEventCount(); i++)
            {
                var obj = onFirstEnter.GetPersistentTarget(i);

                if (obj != null)
                {
                    Vector3 start = Vector3.zero;
                    Vector3 end = Vector3.zero;

                    if (obj is Component)
                    {
                        var comp = (Component)obj;
                        end = comp.transform.position - transform.right;
                    }
                    else
                    {
                        var go = (GameObject)obj;
                        end = go.transform.position - transform.right;
                    }
                    
                    start = transform.position - transform.right;
                    
                    
                    Handles.SphereHandleCap(0, start, Quaternion.identity, .25f, EventType.Repaint);
                    Handles.SphereHandleCap(0, end, Quaternion.identity, .25f, EventType.Repaint);
                    Handles.DrawBezier(start, end, start + Vector3.up, end + Vector3.up, Color.blue, null, 2f);
                }
            }
        }
        
        if (onLastExit.GetPersistentEventCount() > 0)
        {
            Handles.color = Color.red;
            
            for (int i = 0; i < onLastExit.GetPersistentEventCount(); i++)
            {
                var obj = onLastExit.GetPersistentTarget(i);

                if (obj != null)
                {
                    Vector3 start = Vector3.zero;
                    Vector3 end = Vector3.zero;

                    if (obj is Component)
                    {
                        var comp = (Component)obj;
                        end = comp.transform.position + transform.right;
                    }
                    else
                    {
                        var go = (GameObject)obj;
                        end = go.transform.position + transform.right;
                    }

                    start = transform.position + transform.right;

                    Handles.SphereHandleCap(0, start, Quaternion.identity, .25f, EventType.Repaint);
                    Handles.SphereHandleCap(0, end, Quaternion.identity, .25f, EventType.Repaint);
                    Handles.DrawBezier(start, end, start + Vector3.up, end + Vector3.up, Color.red, null, 2f);
                }
            }
        }
    }
#endif
}
