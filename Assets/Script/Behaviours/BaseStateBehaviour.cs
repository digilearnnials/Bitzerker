using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseStateBehaviour : StateMachineBehaviour
{
    [SerializeField, Range(0f, 100f)] protected float maxSpeed = 10f;
    protected Vector3 velocity, desiredVelocity = default;
    
    protected PhysicData physicData = default;
    protected InputData inputData = default;

    protected int hashGround = Animator.StringToHash("Ground");
    
    
    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!inputData)
        {
            inputData = animator.GetComponent<InputData>();
        }
        
        if (!physicData)
        {
            physicData = animator.GetComponent<PhysicData>();
        }
    }
}
