using System;
using UnityEngine;

public class PhysicData : MonoBehaviour
{
    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 25f;

    private Rigidbody body = default;
    [SerializeField] private bool onGround = default;

    public Rigidbody Body => body;
    public bool OnGround => onGround;
    
    float minGroundDotProduct;

    void OnValidate () {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    private void FixedUpdate()
    {
        onGround = false;
    }

    private void OnCollisionStay(Collision other)
    {
        EvaluateCollision(other);
    }

    private void OnCollisionExit(Collision other)
    {
        EvaluateCollision(other);
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++) {
            Vector3 normal = collision.GetContact(i).normal;
            onGround |= normal.y >= minGroundDotProduct;
        }
    }
}
