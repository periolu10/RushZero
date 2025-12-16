using UnityEngine;

public class EventRouter : MonoBehaviour
{
    PlayerController playerController;

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    // Audio Events
    public void PlayFootstepSFX()
    {
        AudioManager.Instance.PlaySFX("footstep_wood");
    }

    // Attack

    //public void Attack()
    //{
    //    playerController.OnAttackHit();
    //}

    //public void EndAttack()
    //{
    //    playerController.OnAttackEnd();
    //}

    //public void PlayAttackVFX()
    //{
    //    playerController.PlayAttackVFX();
    //}
}
