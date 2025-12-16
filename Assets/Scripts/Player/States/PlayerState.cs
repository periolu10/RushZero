using UnityEngine;

public class PlayerState
{
    protected PlayerController player;

    public PlayerState(PlayerController player)
    {
        this.player = player;
    }

    public virtual bool CanEnter() => true;
    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
