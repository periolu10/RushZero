using UnityEngine;

public class AttackState : PlayerState
{
    public AttackState(PlayerController player) : base(player) { }

    public override bool CanEnter()
    {
        return !player.IsHurt;
    }

    public override void Enter()
    {
        Debug.Log("ATTACK STATE");

        if (!player.IsGrounded)
        {
            player.Animator.Play("Attack_Jump");
        }
        else
        {
            player.Animator.Play("Attack");
        }
    }
}
