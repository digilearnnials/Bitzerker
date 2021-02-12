using UnityEditor;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    
#if UNITY_EDITOR
    [Header("Gizmo Options")]
    [SerializeField] private float gizmoHeight = 1f;
    [SerializeField] private Color discColor = Color.cyan;
    [SerializeField] private Color forwardLineColor = Color.blue;
    [SerializeField] private Color dirLineLineColor = Color.red;
    [SerializeField] private float discRadius = 1;
    [Space]
#endif

    [Header("Class parameters"), Space]
    
    [SerializeField] Transform playerInputSpace = default;

    [SerializeField, Range(0f, 100f)] private float maxSpeed = 10f, maxClimbSpeed = 4f, maxSwimSpeed = 5f;
    
    [SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f, maxAirAcceleration = 1f, maxClimbAcceleration = 20f, maxSwimAcceleration = 5f;
    
    [SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
    
    [SerializeField, Range(0, 5)] private int maxAirJumps = default;
    
    [SerializeField, Range(0f, 90f)] private float maxGroundAngle = 25f, maxStairsAngle = 50f;
    
    [SerializeField, Range(90, 180)] float maxClimbAngle = 140f;
    
    [SerializeField, Range(0f, 100f)] float maxSnapSpeed = 100f;
    
    [SerializeField, Min(0f)] float probeDistance = 1f;
    
    [SerializeField] LayerMask probeMask = -1, stairsMask = -1, climbMask = -1, waterMask = 0;
    
    [SerializeField] float submergenceOffset = 0.5f;

    [SerializeField, Min(0.1f)] float submergenceRange = 1f;
    
    [SerializeField, Range(0f, 10f)] float waterDrag = 1f;
    
    [SerializeField, Min(0f)] float buoyancy = 1f;
    
    [SerializeField, Range(0.01f, 1f)] float swimThreshold = 0.5f;
    
    [SerializeField] private bool lockDir = default;

    private float minGroundDotProduct, minStairsDotProduct, minClimbDotProduct;
    
    Vector3 playerInput;
    
    private Vector3 velocity, connectionVelocity;
    
    private Vector3 contactNormal, steepNormal, climbNormal, lastClimbNormal;
    
    private int jumpPhase, groundContactCount, steepContactCount, climbContactCount;
    
    private Vector3 upAxis, rightAxis, forwardAxis, dirAxis;

    private Vector3 connectionWorldPosition, connectionLocalPosition;

    private bool desiredJump, desiresClimbing;
    
    int stepsSinceLastGrounded, stepsSinceLastJump;

    private bool OnGround => groundContactCount > 0;

    private bool OnSteep => steepContactCount > 0;
    
    private bool Climbing => climbContactCount > 0 && stepsSinceLastJump > 2;

    private bool InWater => submergence > 0f;

    private bool Swimming => submergence >= swimThreshold;

    private float submergence = 0;

    private Rigidbody body, connectedBody, previousConnectedBody;

    private Animator animator;

    void OnValidate () {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
    }
    
    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;

        dirAxis = transform.forward;

        animator = GetComponent<Animator>();
        
        OnValidate();
    }

    private void Update(){
        
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput.z = Swimming ? Input.GetAxis("UpDown") : 0f;
        playerInput = Vector3.ClampMagnitude(playerInput, 1f);

        MoveToDir(playerInput, Input.GetButtonDown("Jump"), Input.GetButton("Climb"));
    }

    private void FixedUpdate()
    {
        // _upAxis = -Physics.gravity.normalized;

        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
        
        UpdateState();

        if (InWater)
        {
            velocity *= 1f - waterDrag * submergence * Time.deltaTime;
        }
        
        AdjustVelocity();

        if (desiredJump) {
            desiredJump = false;
            Jump(gravity);
        }

        if (Climbing)
        {
            velocity -= contactNormal * (maxClimbAcceleration * 0.8f * Time.fixedDeltaTime);
        }else if (InWater) {
            velocity += gravity * ((1f - buoyancy * submergence) * Time.deltaTime);
        }
        else if (OnGround && velocity.sqrMagnitude < 0.01f) {
            velocity +=
                contactNormal *
                (Vector3.Dot(gravity, contactNormal) * Time.deltaTime);
        }
        else if (desiresClimbing && OnGround) {
            velocity += (gravity - contactNormal * (maxClimbAcceleration * 0.8f)) * Time.fixedDeltaTime;
        }
        else
        {
            velocity += gravity * Time.fixedDeltaTime;
        }

        body.velocity = velocity;

        ///////// TODO hacer esto en las BehaviourScripts
        animator.SetBool("TargetLock", lockDir);
        
        if (body.velocity.sqrMagnitude / maxSpeed > 0.02)
        {
            if (!lockDir)
            {
                maxSpeed = 10;
                animator.speed = 1;
                animator.SetFloat("VelocityY", body.velocity.magnitude / maxSpeed);
                animator.SetFloat("VelocityX", 0f);
            }
            else
            {
                maxSpeed = 5;
                animator.speed = 1.5f;
                animator.SetFloat("VelocityY", body.velocity.magnitude / maxSpeed);
                animator.SetFloat("VelocityX", body.velocity.magnitude / maxSpeed);
            }
        }
        else
        {
            animator.SetFloat("VelocityY", 0f);
            animator.SetFloat("VelocityX", 0f);
        }
        //////////////////////////////////////////
        
        
        AlignBody();
        
        ClearState();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var gizmosPos = transform.position + (transform.up * gizmoHeight);
        Handles.color = discColor;
        Handles.DrawWireDisc(gizmosPos, transform.up, discRadius);
    
        Handles.color = forwardLineColor;
        Handles.ArrowHandleCap(0, gizmosPos, Quaternion.LookRotation(transform.forward), discRadius, EventType.Repaint);

        Handles.color = dirLineLineColor;
        Handles.ArrowHandleCap(0, gizmosPos, Quaternion.LookRotation(velocity.normalized), discRadius * (velocity.magnitude / maxSpeed), EventType.Repaint);
        Handles.DrawWireDisc(gizmosPos, transform.up, discRadius * (velocity.magnitude / maxSpeed));
    
        Handles.Label(gizmosPos + transform.right * discRadius, $"Velocity \n{velocity}");
    }
#endif

    public void MoveToDir(Vector2 direction, bool jump, bool climb)
    {
        if (playerInputSpace)
        {
            rightAxis = Vector3.ProjectOnPlane(playerInputSpace.right, upAxis);
            forwardAxis = Vector3.ProjectOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = Vector3.ProjectOnPlane(Vector3.right, upAxis);
            forwardAxis = Vector3.ProjectOnPlane(Vector3.forward, upAxis);
        }
        
        if(direction.magnitude > .01f) 
            dirAxis = Vector3.ProjectOnPlane(playerInputSpace.TransformDirection(new Vector3(direction.x, 0f, direction.y)), upAxis);

        if (Swimming)
        {
            desiresClimbing = false;
        }
        else
        {
            desiredJump |= jump;
            desiresClimbing = climb;
        }
    }

    private void ClearState()
    {
        groundContactCount = steepContactCount = climbContactCount = 0;
        contactNormal = steepNormal = connectionVelocity = climbNormal = Vector3.zero;
        previousConnectedBody = connectedBody;
        connectedBody = null;
        submergence = 0f;
    }

    private void UpdateState () {
        
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        
        velocity = body.velocity;
        if (CheckClimbing() || CheckSwimming() || OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }
            
            if(groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = upAxis;
        }

        if (connectedBody)
        {
            if (connectedBody.isKinematic || connectedBody.mass >= body.mass)
            {
                UpdateConnectionState();
            }
        }
    }

    void UpdateConnectionState()
    {
        if (connectedBody == previousConnectedBody)
        {
            Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
        }
        connectionWorldPosition = body.position;

        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
    }

    private void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;
        
        if (OnGround) {
            jumpDirection = contactNormal;
        }
        else if (OnSteep) {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps) {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else {
            return;
        }
        
        stepsSinceLastJump = 0;
        jumpPhase += 1;
        
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);

        if (InWater)
        {
            jumpSpeed *= Mathf.Max(0f, 1f - submergence / swimThreshold);
        }
        
        jumpDirection = (jumpDirection + upAxis).normalized;
        
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        
        if (alignedSpeed > 0f) {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        
        velocity += jumpDirection * jumpSpeed;
    }
    
    public void PreventSnapToGround () {
        stepsSinceLastJump = -1;
    }

    private void AdjustVelocity ()
    {
        float acceleration, speed;
        
        Vector3 xAxis, zAxis;
        
        if (Climbing)
        {
            float swimFactor = Mathf.Min(1f, submergence / swimThreshold);
            acceleration = Mathf.LerpUnclamped(OnGround ? maxAcceleration : maxAirAcceleration, maxSwimAcceleration, swimFactor);
            speed = Mathf.LerpUnclamped(maxAcceleration, maxSwimAcceleration, swimFactor);
            xAxis = Vector3.Cross(contactNormal, upAxis);
            zAxis = upAxis;
        }else if (InWater) {
            acceleration = maxSwimAcceleration;
            speed = maxSwimSpeed;
            xAxis = rightAxis;
            zAxis = forwardAxis;
        }
        else {
            acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            speed = OnGround && desiresClimbing ? maxClimbSpeed : maxSpeed;
            xAxis = rightAxis;
            zAxis = forwardAxis;
        }
        
        xAxis = Vector3.ProjectOnPlane(xAxis, contactNormal);
        
        zAxis = Vector3.ProjectOnPlane(zAxis, contactNormal);

        Vector3 relativeVelocity = velocity - connectionVelocity;
        
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);
        
        float maxSpeedChange = acceleration * Time.fixedDeltaTime;

        float newX = Mathf.MoveTowards(currentX, playerInput.x * speed, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, playerInput.y * speed, maxSpeedChange);
        
        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);

        if (Swimming)
        {
            float currentY = Vector3.Dot(relativeVelocity, upAxis);
            float newY = Mathf.MoveTowards(currentY, playerInput.z * speed, maxSpeedChange);
            velocity += upAxis * (newY - currentY);
        }
    }
    
    void OnTriggerEnter (Collider other) {
        if ((waterMask & (1 << other.gameObject.layer)) != 0) {
            EvaluateSubmergence(other);
        }
    }

    void OnTriggerStay (Collider other) {
        if ((waterMask & (1 << other.gameObject.layer)) != 0) {
            EvaluateSubmergence(other);
        }
    }
    
    void EvaluateSubmergence (Collider collider) {
        if (Physics.Raycast(body.position + upAxis * submergenceOffset, -upAxis, out RaycastHit hit, submergenceRange + 1f, waterMask, QueryTriggerInteraction.Collide))
        {
            submergence = 1f - hit.distance / submergenceRange;
        }
        else
        {
            submergence = 1f;
        }
        
        if (Swimming)
        {
            connectedBody = collider.attachedRigidbody;
        }
    }
    
    bool CheckSwimming () {
        if (Swimming) {
            groundContactCount = 0;
            contactNormal = upAxis;
            return true;
        }
        return false;
    }
    
    void OnCollisionEnter (Collision collision) {
        EvaluateCollision(collision);
    }

    void OnCollisionStay (Collision collision) {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        if (Swimming)
        {
            return;
        }
        
        int layer = collision.gameObject.layer;
        float minDot = GetMinDot(layer);
        
        for (int i = 0; i < collision.contactCount; i++) {
            Vector3 normal = collision.GetContact(i).normal;

            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
                connectedBody = collision.rigidbody;
            }
            else
            {
                if (upDot > -0.01f)
                {
                    steepContactCount += 1;
                    steepNormal += normal;
                    if (groundContactCount == 0)
                    {
                        connectedBody = collision.rigidbody;
                    }
                }
                
                if (desiresClimbing && upDot >= minClimbDotProduct && (climbMask & (1 << layer)) != 0) {
                    climbContactCount += 1;
                    climbNormal += normal;
                    lastClimbNormal = normal;
                    connectedBody = collision.rigidbody;
                }
            }
        }
    }
    
    bool SnapToGround () {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2) {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed) {
            return false;
        }
        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask, QueryTriggerInteraction.Ignore)) {
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer)) {
            return false;
        }
        
        groundContactCount = 1;
        contactNormal = hit.normal;
        
        float dot = Vector3.Dot(velocity, hit.normal);
        
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }

        connectedBody = hit.rigidbody;
        return true;
    }
    
    float GetMinDot (int layer) {
        return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
    }
    
    bool CheckSteepContacts () {
        if (steepContactCount > 1) {
            steepNormal.Normalize();

            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct) {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }
    
    bool CheckClimbing () {
        if (Climbing) {
            if (climbContactCount > 1) {
                climbNormal.Normalize();
                float upDot = Vector3.Dot(upAxis, climbNormal);
                if (upDot >= minGroundDotProduct) {
                    climbNormal = lastClimbNormal;
                }
            }
            groundContactCount = 1;
            contactNormal = climbNormal;
            return true;
        }
        return false;
    }
    
    private void AlignBody()
    {
        if (!Climbing)
        {
            // Alinear al personaje con referencia a la direccion de movimiento
            if (!lockDir)
            {
                body.rotation = Quaternion.SlerpUnclamped(body.rotation,Quaternion.LookRotation(dirAxis, upAxis), Time.fixedDeltaTime * 5);
            }
            else
            {
                dirAxis = forwardAxis;
                body.rotation = Quaternion.SlerpUnclamped(body.rotation,Quaternion.LookRotation(dirAxis, upAxis), Time.fixedDeltaTime * 10);
            }
        }
        else
        {
            dirAxis = -climbNormal;
            body.rotation = Quaternion.SlerpUnclamped(body.rotation,Quaternion.LookRotation(dirAxis, upAxis), Time.fixedDeltaTime * 10);
        }

        // Alinear al personaje con la direcion de la gravedad
        body.rotation = Quaternion.SlerpUnclamped(body.rotation,Quaternion.FromToRotation(body.transform.up, upAxis) * body.rotation, Time.fixedDeltaTime * 10);
    }
}
