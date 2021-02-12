using UnityEngine;

public class OnGroundBehaviour : BaseStateBehaviour
{
    [SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
    [SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        velocity = physicData.Body.velocity;
        
        desiredVelocity = new Vector3(inputData.MoveInput.x, 0f, inputData.MoveInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.fixedDeltaTime;
        
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        
        if (inputData.JumpInput)
        {
            inputData.JumpInput = false;
            Jump();
        }
        
        physicData.Body.velocity = velocity;
        
        animator.SetBool(hashGround, physicData.OnGround);
    }

    void Jump()
    {
        if(physicData.OnGround) velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
    }
}
