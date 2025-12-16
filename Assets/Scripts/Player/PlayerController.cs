using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerSettings;

public class PlayerController : MonoBehaviour
{
    // Determine the stage type
    public LevelData.ControlType controlType;
    LevelManager levelManager;

    // Determine current player state/action
    private List<PlayerState> states;
    private PlayerState currentState;

    public PlayerState CurrentState => currentState;

    [Header("Stats")] public PlayerStats stats;

    [Header("Hub Movement Settings")] public MovementSettings hubMovement;

    [Header("Action Movement Settings")] public MovementSettings actionMovement;

    [Header("Jump Settings")] public JumpSettings jumpSettings;
    float jumpBufferCounter = 0f;

    [Header("Player Actions")] public PlayerActions playerActions;

    float moveSpeed;
    float acceleration;
    float deceleration;

    bool isPushing;

    // Seperate vars to indicate when the player is in a trigger, in order to prevent missed inputs.
    [Header("Collision Physics Input Dependencies")]
    private GameObject currentPushable;

    private bool inDoorTrigger = false;
    private SceneLoader currentDoorLoader = null;

    private bool inNPCTrigger = false;
    private DialogueTrigger currentDialogueTrigger = null;

    int playerLayer = 6;
    int enemyLayer = 7;
    int obstacleLayer = 31;

    [Header("Ground Check")]
    [SerializeField] bool isGrounded = false;
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] LayerMask groundLayer;


    private float boostBufferCounter = 0f;
    private float boostTimer = 0;
    private bool isBoosting = false;
    private bool boostOnCooldown = false;


    [SerializeField] LayerMask enemyLayerMask;

    bool isHurt;
    bool IFRAMES;

    [Header("References")]
    private Rigidbody2D playerRB;
    private SpriteRenderer sprite;
    private Animator animator;
    private HUDManager hudManager;
    public CinemachineImpulseSource boostImpulse;

    [Header("Effects")]
    public TrailRenderer trail;
    public ParticleSystem chargeParticles;
    public ParticleSystem windParticles;
    public Material litShader;
    public Material outlineShader;
    public ParticleSystem boostCircle;
    bool boostBegin = false;

    //-- Input Actions --//
    InputActions controls;
    Vector2 moveInput;
    bool upPressed;
    bool downPressed;
    bool downAttackPerformed = false;

    [Header("Input")]
    //UI
    public bool confirmPressed;
    public bool cancelPressed;

    //-- DEBUG --//
    public TMP_Text stateText;

    #region [PUBLIC GETTERS]
    public Rigidbody2D PlayerRB => playerRB;
    public Vector2 MoveInput => moveInput;
    public bool IsPushing => isPushing;
    public bool IsBoosting => isBoosting;
    public Animator Animator => animator;
    public bool IsGrounded => isGrounded;
    public bool IsHurt => isHurt;

    public SpriteRenderer Sprite => sprite;

    public bool inJump;

    public DialogueTrigger CurrentDialogueTrigger => currentDialogueTrigger;
    #endregion

    private void Awake()
    {
        controls = new InputActions();

        BindInputs();

        // Debug
        controls.Player.MagicDebugKey.performed += ctx => SceneController.Instance.LoadScene(Scenes.Scene.hub_Village);
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    private void Start()
    {
        // SET THIS CONTROLLER AS VAR IN GAME MANAGER
        GameManager.Instance.playerController = this;

        // Setup states
        states = new List<PlayerState>()
        {
            //new AttackState(this),
            //new BoostState(this),
            new HurtState(this),
            new PushState(this),
            new JumpState(this),
            new FallState(this),
            new MoveState(this),
            new IdleState(this),
        };

        // Get Level Data
        levelManager = FindAnyObjectByType<LevelManager>();
        controlType = levelManager.levelData.controlType;

        // Get References
        playerRB = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        hudManager = FindAnyObjectByType<HUDManager>();

        SetupController();

        // Setup Stats
        stats.currentHealth = stats.maxHealth;
        playerRB.gravityScale = jumpSettings.gravityScale;

        // Other setup
        windParticles.Stop();
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        HandleJump();

        SpriteFlip();

        #region Trigger/Collision Events
        // UpPress
        // Scene/Room Loading
        if (inDoorTrigger && upPressed && currentDoorLoader != null)
        {
            if (currentDoorLoader.loadType == SceneLoader.LoadType.Scene && currentDoorLoader.canOpenHud)
            {
                currentDoorLoader.canOpenHud = false;
                currentDoorLoader.OpenStageHUD();
            }
            else if (currentDoorLoader.loadType == SceneLoader.LoadType.Room)
            {
                currentDoorLoader.LoadScene();
            }
        }
        // NPC - Dialogue
        else if (inNPCTrigger && upPressed && currentDialogueTrigger != null)
        {
            DialogueManager dialogueManager = FindAnyObjectByType<DialogueManager>();

            if (dialogueManager.canOpenDialogue)
            {
                dialogueManager.canOpenDialogue = false;
                currentDialogueTrigger.BeginDialogue();
            }
        }

        // Push
        if (currentPushable != null)
        {
            Vector3 directionToTarget = currentPushable.transform.position - transform.position;
            directionToTarget.Normalize();

            float moveDirection = MoveInput.x;

            isPushing = (moveDirection > 0 && directionToTarget.x > 0) ||
                        (moveDirection < 0 && directionToTarget.x < 0);

            isPushing &= moveDirection != 0;
        }
        else
        {
            isPushing = false;
        }
        #endregion

        // -- STATE CONTROL -- //
        foreach (var state in states)
        {
            if (state.CanEnter() && state != currentState)
            {
                currentState?.Exit();
                currentState = state;
                currentState.Enter();
                break;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!playerActions.move) return;

        float targetSpeed = moveInput.x * moveSpeed;

        if (controlType == LevelData.ControlType.Runner && Mathf.Approximately(moveInput.x, 0f))
        {
            targetSpeed = -7;
        }

        float speedDiff = targetSpeed - playerRB.linearVelocityX;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = speedDiff * accelRate;
        float newVelocityX = playerRB.linearVelocityX + movement * Time.fixedDeltaTime;

        if (controlType != LevelData.ControlType.Runner && Mathf.Abs(newVelocityX) < 1f)
            newVelocityX = 0f;

        float directionMultiplier = sprite.flipX ? -1 : 1;

        if (isBoosting)
        {
            if (Mathf.Abs(playerRB.linearVelocityX) < 0.5f && boostBegin)
            {
                isBoosting = false;
                return;
            }

            playerRB.linearVelocity = new Vector2((actionMovement.moveSpeed + actionMovement.boostBonus) * directionMultiplier, playerRB.linearVelocityY);
            Boost(true);
        }
        else
        {
            playerRB.linearVelocity = new Vector2(newVelocityX, playerRB.linearVelocityY);
            Boost(false);
        }

        float absVel = Mathf.Abs(playerRB.linearVelocityX);
        if (absVel >= 8 || controlType == LevelData.ControlType.Runner) animator.SetFloat("Speed", 1);
        else if (absVel >= 5) animator.SetFloat("Speed", 0.5f);
        else if (absVel >= 1) animator.SetFloat("Speed", 0);
        else animator.SetFloat("Speed", 0);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            collision.gameObject.GetComponent<EnemyController>().TakeDamage(200);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pushable"))
        {
            currentPushable = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == currentPushable)
        {
            currentPushable = null;
            isPushing = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("RoomChange"))
        {
            inDoorTrigger = true;
            currentDoorLoader = collision.GetComponent<SceneLoader>();
        }
        else if (collision.CompareTag("NPC"))
        {
            inNPCTrigger = true;
            currentDialogueTrigger = collision.GetComponent<DialogueTrigger>();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("RoomChange"))
        {
            inDoorTrigger = false;
            currentDoorLoader = null;
        }
        else if (collision.CompareTag("NPC"))
        {
            inNPCTrigger = false;
            currentDialogueTrigger = null;
        }
    }


    #region |-- PLAYER METHODS --|


    void SpriteFlip()
    {
        if (playerRB.linearVelocityX >= 0.01f && moveInput.x > 0.01f)
        {
            sprite.flipX = false;

            // VFX
            windParticles.transform.SetLocalPositionAndRotation(new Vector2(2, 0), Quaternion.Euler(0, 0, 0));
            trail.transform.localPosition = new Vector2(0.8f, 0);
        }
        else if (playerRB.linearVelocityX <= -0.01f && moveInput.x < -0.01f && controlType != LevelData.ControlType.Runner) // Do NOT flip sprite on runner stages.
        {
            sprite.flipX = true;

            // VFX
            windParticles.transform.SetLocalPositionAndRotation(new Vector2(-2, 0), Quaternion.Euler(0, 180, 0));
            trail.transform.localPosition = new Vector2(-0.8f, 0);
        }
    }

    public void SpriteFlipX()
    {
        sprite.flipX = false;
    }

    void SetupController()
    {
        if (controlType == LevelData.ControlType.Cutscene)
        {
            DisableAllControl();
            return;
        }

        playerActions.move = true;
        playerActions.jump = true;

        if (controlType == LevelData.ControlType.Hub)
        {
            playerActions.attack = false;
            playerActions.boost = false;

            moveSpeed = hubMovement.moveSpeed;
            acceleration = hubMovement.acceleration;
            deceleration = hubMovement.deceleration;

        }
        else if (controlType == LevelData.ControlType.Action || controlType == LevelData.ControlType.Runner)
        {
            // Check for unlockable mechanics
            CheckMechanics();

            playerActions.attack = true;
            playerActions.boost = true;

            moveSpeed = actionMovement.moveSpeed;
            acceleration = actionMovement.acceleration;
            deceleration = actionMovement.deceleration;
        }
    }

    /// <summary>
    /// Used by enemies to deal damage to the player.
    /// </summary>
    /// <param name="damage">Damage amount to be taken off.</param>
    public void TakeDamage(float damage, Transform sourceTransform)
    {
        if (IFRAMES) return;

        if (stats.currentHealth - damage >= 0) stats.currentHealth -= damage;
        else if (stats.currentHealth < 0)
        {
            stats.currentHealth = 0;
        }

        if (stats.currentHealth <= 0)
        {
            Death();
        }

        isHurt = true;
        IFRAMES = true;
        StartCoroutine(HitIFrames());

        UpdateHUD();

        // Knockback
        float knockForce;
        Vector2 knockbackDir = (transform.position - sourceTransform.position).normalized;
        if (isGrounded) { knockbackDir.y = 0; knockForce = 10; }
        else knockForce = 10;
        playerRB.linearVelocity = Vector2.zero;
        playerRB.AddForce(knockbackDir * knockForce, ForceMode2D.Impulse);

        // Sound
        AudioManager.Instance.PlaySFX("player_hit");
    }

    /// <summary>
    /// Used by items/interactables to heal the player.
    /// </summary>
    /// <param name="amount">Heal amount to apply.</param>
    public void Heal(float amount)
    {
        if (stats.currentHealth + amount <= stats.maxHealth) stats.currentHealth += amount;
        else if (stats.currentHealth + amount > stats.maxHealth) stats.currentHealth = stats.maxHealth;
    }

    IEnumerator HitIFrames()
    {
        Time.timeScale = 0.3f;
        DisableAllControl();
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        yield return new WaitForSeconds(0.2f);
        EnableControls();
        isHurt = false;
        Time.timeScale = 1f;

        float invulTime = 1.5f;
        float flashInterval = 0.15f;
        float elapsed = 0f;

        while (elapsed < invulTime)
        {
            // Toggle color
            sprite.color = Color.gray5;
            yield return new WaitForSeconds(flashInterval);

            sprite.color = Color.white;
            yield return new WaitForSeconds(flashInterval);

            elapsed += flashInterval * 2f;
        }

        IFRAMES = false;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        sprite.color = Color.white;
    }

    void CheckMechanics()
    {

    }

    private void Boost(bool isBoosting)
    {
        if (isBoosting)
        {
            if (!boostBegin)
            {
                boostImpulse.GenerateImpulse();
                boostBegin = true;
                trail.emitting = true;
                Sprite.material = outlineShader;
                windParticles.Play();
                boostCircle.Play();

                AudioManager.Instance.SetParameter("Muffle", 1, 0.1f);
                AudioManager.Instance.PlaySFX("player_boost");
                AudioManager.Instance.PlayBoostLoop();
            }
        }
        else
        {
            if (boostBegin)
            {
                trail.emitting = false;
                Sprite.material = litShader;
                windParticles.Stop();

                boostBegin = false;

                AudioManager.Instance.StopBoostLoop(true);
                AudioManager.Instance.SetParameter("Muffle", 0, 0.1f);
            }
        }
    }

    public void Death()
    {
        if (GameManager.Instance.gameData.playerLives > 0)
        {
            GameManager.Instance.ChangeLives(-1);
            StartCoroutine(Restart());
        }
        else
        {
            // GAME OVAH
        }

        // Restart
    }

    IEnumerator Restart()
    {
        DisableAllControl();
        sprite.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(1);
        yield return StartCoroutine(SceneController.Instance.FadeOut(SceneController.Instance.fadeCanvasGroup, 1));

        sprite.enabled = true;
        GetComponent<Collider2D>().enabled = true;
        transform.position = levelManager.startPoint.position;

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(SceneController.Instance.FadeIn(SceneController.Instance.fadeCanvasGroup, 0.5f));

        EnableControls();
    }
    #endregion

    #region Update Methods
    private void HandleJump()
    {
        // Jump
        if (jumpBufferCounter > 0)
        {
            inJump = false;

            jumpBufferCounter -= Time.deltaTime;

            if (isGrounded && playerActions.jump)
            {
                playerRB.linearVelocity = new Vector2(playerRB.linearVelocityX, jumpSettings.jumpForce);
                jumpBufferCounter = 0f;

                inJump = true;

                // Audio
                AudioManager.Instance.PlaySFX("player_jump");
            }
        }
    }
    #endregion

    #region |-- HUD --|
    void UpdateHUD()
    {
        hudManager.UpdateHealth(stats.currentHealth, stats.maxHealth);
        hudManager.ShakeHealthBar();
    }
    #endregion

    #region PLAYER CONTROLS
    public void DisableAllControl()
    {
        playerRB.linearVelocity = Vector2.zero;

        playerActions.move = false;
        playerActions.jump = false;
        playerActions.attack = false;
        playerActions.boost = false;

        // Force stop boost.
        Boost(false);
    }

    public void EnableControls()
    {
        SetupController();
    }
    #endregion

    #region | -- VFX -- |
    //public void PlayAttackVFX()
    //{
    //    attackVFX.SetActive(true);

    //    if (downAttackPerformed)
    //    {
    //        attackVFX.transform.localPosition = new Vector2(0f, -1);
    //        attackVFX.transform.localRotation = Quaternion.Euler(0, 0, -90f);

    //        if (!sprite.flipX)
    //        {
    //            attackVFX.GetComponent<SpriteRenderer>().flipY = false;
    //        }
    //        else if (sprite.flipX)
    //        {
    //            attackVFX.GetComponent<SpriteRenderer>().flipY = true;
    //        }
    //    }
    //    else
    //    {
    //        attackVFX.transform.localRotation = Quaternion.Euler(0, 0, 0f);

    //        if (!sprite.flipX)
    //        {
    //            attackVFX.transform.localPosition = new Vector2(1f, 0);
    //            attackVFX.GetComponent<SpriteRenderer>().flipX = false;
    //        }
    //        else if (sprite.flipX)
    //        {
    //            attackVFX.transform.localPosition = new Vector2(-1f, 0);
    //            attackVFX.GetComponent<SpriteRenderer>().flipX = true;
    //        }
    //    }

    //        attackVFX.GetComponent<Animator>().Play("Slash1", 0, 0);
    //}

    //public void StopAttackVFX()
    //{
    //    attackVFX.SetActive(false);
    //}
    #endregion

    #region Input
    private void BindInputs()
    {
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => jumpBufferCounter = jumpSettings.jumpBufferTime;
        controls.Player.Jump.canceled += ctx =>
        {
            if (playerRB.linearVelocityY > 0)
                playerRB.linearVelocity = new Vector2(playerRB.linearVelocityX, playerRB.linearVelocityY * jumpSettings.jumpCutMultiplier);
        };

        controls.Player.Action.performed += ctx =>
        {
            if (controlType == LevelData.ControlType.Hub) return;

            isBoosting = true;
        };
        controls.Player.Action.canceled += ctx => isBoosting = false;

        controls.Player.Up.performed += ctx => upPressed = true;
        controls.Player.Up.canceled += ctx => upPressed = false;

        controls.Player.Down.performed += ctx => downPressed = true;
        controls.Player.Down.canceled += ctx => downPressed = false;

        controls.UI.Submit.performed += ctx => confirmPressed = true;
        controls.UI.Submit.canceled += ctx => confirmPressed = false;

        controls.UI.Cancel.performed += ctx => cancelPressed = true;
        controls.UI.Cancel.canceled += ctx => cancelPressed = false;
    }
    #endregion

    #region |-- DEBUG --|
    //-- DEBUG --//
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);

        //Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
    #endregion
}
