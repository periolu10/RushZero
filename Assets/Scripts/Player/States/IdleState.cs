using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerController player) : base(player) { }

    public override bool CanEnter()
    {
        return player.IsGrounded && 
            !player.IsHurt &&
            !player.IsPushing &&
            player.PlayerRB.linearVelocityX == 0 &&
            player.controlType != LevelData.ControlType.Runner;
    }

    public override void Enter()
    {
        player.Animator.Play("Idle");
    }

}
