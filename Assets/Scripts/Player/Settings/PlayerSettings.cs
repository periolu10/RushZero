using UnityEngine;

public class PlayerSettings
{
    [System.Serializable]
    public class PlayerStats
    {
        public float currentHealth;
        public float maxHealth = 100;
    }

    [System.Serializable]
    public class MovementSettings
    {
        public float moveSpeed = 5f;
        public float acceleration = 10f;
        public float deceleration = 15f;
        public float boostBonus = 20;
    }

    [System.Serializable]
    public class JumpSettings
    {
        public float jumpForce = 5f;
        public float jumpBufferTime = 0.1f;
        public float jumpCutMultiplier = 0.5f;
        public float gravityScale = 3f;
    }

    [System.Serializable]
    public class BoostSettings
    {
        public int charges = 0;
        public int maxCharges = 3;
        public float rushEnergy = 0f;
        public float refillAmount = 5f;
        public float maxRefill = 15f;
        public float power = 20f;
        public float duration = 1f;
        public float bufferTime = 0.15f;
        public float cooldown = 0.5f;
    }

    [System.Serializable]
    public class PlayerActions
    {
        public bool move;
        public bool jump;
        public bool attack;
        public bool boost;

        public bool interact;
        public bool roomChange;
    }

}
