using UnityEngine;

public class HurtState : PlayerState
{
    public HurtState(PlayerController player) : base(player) { }

    public override bool CanEnter()
    {
        return player.IsHurt;
    }

    public override void Enter()
    {
        player.Animator.Play("Hurt");
    }
}
