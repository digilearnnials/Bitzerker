using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AccelerationZone : MonoBehaviour
{
    [SerializeField, Min(0f)] private float acceleration = 0f, speed = 10f;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body)
        {
            Accelerate(body);    
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body)
        {
            Accelerate(body);
        }
    }

    void Accelerate(Rigidbody body)
    {
        Vector3 velocity = transform.InverseTransformDirection(body.velocity);

        if (acceleration > 0f)
        {
            velocity = Vector3.MoveTowards(velocity, transform.up * speed, acceleration * Time.deltaTime) ;
        }
        else
        {
            velocity = transform.up * speed;
        }
        
        body.velocity = velocity;

        if (body.TryGetComponent(out MovingSphere sphere))
        {
            sphere. PreventSnapToGround();
        }
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.green;
        
        Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(transform.up), 1f, EventType.Repaint);
    }
}
