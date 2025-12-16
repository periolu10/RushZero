using UnityEngine;

public class FallState : PlayerState
{
    public FallState(PlayerController player) : base(player) { }

    public override bool CanEnter()
    {
        return !player.IsGrounded &&
            player.PlayerRB.linearVelocityY <= -0.01f &&
            !player.IsHurt;
    }

    public override void Enter()
    {
        player.Animator.Play("AirSpin");
    }

}
