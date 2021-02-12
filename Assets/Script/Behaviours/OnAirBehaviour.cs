using UnityEngine;

public class OnAirBehaviour : BaseStateBehaviour
{
    [SerializeField, Range(0, 5)] int maxAirJumps = 0;
    [SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
    [SerializeField, Range(0f, 100f)] float maxAirAcceleration = 1f;
    
    private int jumpPhase = 0;

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        velocity = physicData.Body.velocity;
        
        desiredVelocity = new Vector3(inputData.MoveInput.x, 0f, inputData.MoveInput.y) * maxSpeed;
        float maxSpeedChange = maxAirAcceleration * Time.fixedDeltaTime;
        
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        
        if(inputData.JumpInput) AirJump();
        
        physicData.Body.velocity = velocity;
        
        animator.SetBool(hashGround, physicData.OnGround);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        jumpPhase = 0;
    }

    void AirJump()
    {
        if (jumpPhase < maxAirJumps) {
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            
            if (velocity.y > 0f) {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            
            velocity.y += jumpSpeed;
        }
    }
}
