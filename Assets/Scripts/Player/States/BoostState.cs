using UnityEngine;

public class BoostState : PlayerState
{
    public BoostState(PlayerController player) : base(player) { }

    public override bool CanEnter()
    {
        return !player.IsHurt && player.IsBoosting;
    }

    public override void Enter()
    {
        Debug.Log("BOOST STATE");

        player.trail.enabled = true;
        player.Sprite.material = player.outlineShader;
        player.windParticles.Play();
    }

    public override void Exit()
    {
        base.Exit();

        player.trail.enabled = false;
        player.Sprite.material = player.litShader;
        player.windParticles.Stop();
    }
}
