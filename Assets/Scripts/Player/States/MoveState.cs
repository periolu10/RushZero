using UnityEngine;
using static LevelData;

public class MoveState : PlayerState
{
    public MoveState(PlayerController player) : base(player) { }

    public override bool CanEnter()
    {
        if (!player.IsGrounded || player.IsHurt || player.IsPushing)
            return false;

        // Runner stage always allow entering
        if (player.controlType == ControlType.Runner)
            return true;

        return Mathf.Abs(player.PlayerRB.linearVelocityX) >= 0.01f;
    }

    public override void Enter()
    {
        if (player.controlType == ControlType.Hub)
        {
            player.Animator.Play("RunMid");
        }
        else
        {
            player.Animator.Play("Run");
        }
    }
}
