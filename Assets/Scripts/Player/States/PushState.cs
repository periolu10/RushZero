using UnityEngine;

public class PushState : PlayerState
{
    public PushState(PlayerController player) : base(player) { }

    public override bool CanEnter()
    {
        return player.IsGrounded && player.IsPushing && !player.IsHurt;
    }

    public override void Enter()
    {
        player.Animator.Play("Push");
    }
}
