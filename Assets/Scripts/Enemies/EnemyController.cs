using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] EnemyData enemyData; // MAKE SURE TO DRAGnDROP THE ENEMYDATA S.O. HERE

    float currentHealth;
    SpriteRenderer sprite;
    Animator animator;
    Rigidbody2D rb;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public int currentPointIndex = 0;
    Vector2 lastPos;

    [Header("Chase Settings")]
    public float chaseSpeed = 3f;
    public float detectionRange = 5f;
    public float attackRange = 1f;

    [Header("Attack Settings")]
    public float attackCooldown = 1f;
    private float lastAttackTime;
    public Transform attackPoint;
    public float attackRadius;
    public LayerMask playerLayerMask;
    bool isAttacking;

    [Header("Idle Settings")]
    public float idleTime = 1f;
    bool isIdle = false;

    private Transform player;
    private Vector3 initialPosition;
    bool isHit;
    float hitStun = 0.3f;

    private enum State { Patrolling, Chasing, Attacking, Returning, Idle }
    private State currentState = State.Patrolling;

    [Header("HUD References")]
    [SerializeField] GameObject hudGO;
    [SerializeField] TMP_Text enemyNameText;
    [SerializeField] Image healthBarImage;

    [Header("VFX")]
    [SerializeField] ParticleSystem hitParticles;

    private Camera uiCamera;
    bool hudEnabled;

    [Header("What is this???")]
    public GameObject punFunnyHaha;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        initialPosition = transform.position;

        hudGO.SetActive(false);
        if (enemyData.enemyType == EnemyData.EnemyType.Dummy)
        {
            punFunnyHaha.SetActive(false);
        }
        hudEnabled = false;
        currentHealth = enemyData.enemyMaxHealth;
        SetupHUD();

        currentPointIndex = Random.Range(0, 2);
    }

    private void Update()
    {
        if (hitStun > 0)
        {
            hitStun -= Time.deltaTime;
        }
        else
        {
            hitStun = 0;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2f);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Ground"))
            {
                rb.constraints = RigidbodyConstraints2D.FreezePositionY;
            }
        }
    }

    private void FixedUpdate()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (isHit) return;
        if (hitStun > 0) return;
        if (currentHealth <= 0) return;

        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                SetAnimation(patrolPoints[currentPointIndex].position);
                if (distanceToPlayer <= detectionRange)
                    currentState = State.Chasing;
                break;

            case State.Chasing:
                Chase();
                if (distanceToPlayer <= attackRange)
                {
                    currentState = State.Attacking;
                    rb.MovePosition(rb.position);
                }
                else if (distanceToPlayer > detectionRange)
                {
                    currentState = State.Returning;
                }
                else
                {
                    Chase();
                    SetAnimation(player.position);
                }
                break;

            case State.Attacking:
                Attack();
                break;

            case State.Returning:
                ReturnToPatrol();
                SetAnimation(initialPosition);
                if (distanceToPlayer <= detectionRange)
                    currentState = State.Chasing;
                break;
            case State.Idle:
                Idle(distanceToPlayer);
                animator.Play("Idle");
                if (distanceToPlayer <= detectionRange && !isIdle)
                    currentState = State.Chasing;
                break;
        }

        Debug.Log(currentState);
    }

    private void SetAnimation(Vector2 target)
    {
        float distance = Vector2.Distance(rb.position, target);

        if (distance > 0.05f) // still moving
            animator.Play("Run");
        else
            animator.Play("Idle");
    }

    #region Logic Methods
    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPointIndex];
        MoveTowards(targetPoint.position, patrolSpeed);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            currentState = State.Idle;
        }
    }

    void ReturnToPatrol()
    {
        MoveTowards(initialPosition, patrolSpeed);
        if (Vector2.Distance(transform.position, initialPosition) < 0.5f)
        {
            currentState = State.Patrolling;
        }
    }

    void Idle(float distanceToPlayer)
    {
        if (isIdle) return;
        StartCoroutine(IdleWait(distanceToPlayer));
    }

    void Chase()
    {
        MoveTowards(player.position, chaseSpeed);
    }

    void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Debug.Log("Enemy attacks player!");
            lastAttackTime = Time.time;
            rb.MovePosition(rb.position);
            animator.Play("Attack");
        }
    }

    public void DoDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayerMask);

        foreach (var enemy in enemies)
        {
            // Apply damage
            enemy.GetComponent<PlayerController>().TakeDamage(enemyData.enemyDamage, transform);

            // Sound
        }
    }

    public void OnAttackEnd()
    {
        currentState = State.Idle;
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        if (target.x > transform.position.x)
        {
            sprite.flipX = true;
            attackPoint.localPosition = new Vector2(0.3f, -0.1f);
        }
        else if (target.x < transform.position.x)
        {
            sprite.flipX = false;
            attackPoint.localPosition = new Vector2(-0.3f, -0.1f);
        }

        Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.deltaTime);
        rb.MovePosition(newPos);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

    IEnumerator IdleWait(float distanceToPlayer)
    {
        isIdle = true;
        float seconds = Random.Range(0.5f, 1.5f);
        yield return new WaitForSeconds(seconds);
        if (distanceToPlayer <= attackRange)
            currentState = State.Attacking;
        else if (distanceToPlayer <= detectionRange)
            currentState = State.Chasing;
        else if (distanceToPlayer > detectionRange)
            currentState = State.Returning;
        isIdle = false;
    }
    #endregion


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.GetComponent<PlayerController>().IsBoosting) return;

            collision.gameObject.GetComponent<PlayerController>().TakeDamage(enemyData.enemyDamage, transform);
        }
    }

    public void TakeDamage(float damage)
    {
        if (!hudEnabled && GameManager.Instance.gameSettings.showEnemyHealthBars)
        {
            hudGO.SetActive(true);
            hudEnabled = true;

            if (enemyData.enemyType == EnemyData.EnemyType.Dummy)
            {
                punFunnyHaha.SetActive(true);
                AudioManager.Instance.PlaySFX("funnyhaha");
            }
        }

        if (currentHealth - damage >= 0)
        {
            currentHealth -= damage;
        }
        else if (currentHealth - damage < 0)
        {
            currentHealth = 0;
        }

        StartCoroutine(DamageFlash());
        hitParticles.Play();
        hitStun = 0.3f;

        // Knockback
        rb.linearVelocity = Vector2.zero;

        Vector2 forceDir = player.GetComponent<PlayerController>().Sprite.flipX
            ? new Vector2(-1, 0)
            : new Vector2(1, 0);

        float knockbackForce = 30f * rb.mass;
        rb.AddForce(forceDir * knockbackForce, ForceMode2D.Impulse);

        if (currentHealth == 0)
        {
            Death();
        }

        UpdateHealthBar();
    }

    IEnumerator DamageFlash()
    {
        sprite.color = Color.red;
        isHit = true;
        yield return new WaitForSeconds(0.1f);
        isHit = false;
        if (currentHealth != 0) sprite.color = Color.white;
    }

    void Death()
    {
        // Animation
        sprite.color = Color.red;
        animator.Play("Die");

        AudioManager.Instance.PlaySFX("enemy_death");
        
        HUDManager hudManager = FindAnyObjectByType<HUDManager>();
        hudManager.AddScore(150);

        // Disable Colliders
        Collider2D collider = GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(collider, FindAnyObjectByType<PlayerController>().GetComponent<Collider2D>());

        // Destroy after N seconds
        StartCoroutine(DestroyCountdown(2f));
    }

    IEnumerator DestroyCountdown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }

    // -- HUD -- //

    void SetupHUD()
    {
        enemyNameText.text = enemyData.enemyName;
        UpdateHealthBar();

        uiCamera = GameObject.FindWithTag("UI_Camera").GetComponent<Camera>();
        hudGO.GetComponent<Canvas>().worldCamera = uiCamera;
    }

    void UpdateHealthBar()
    {
        healthBarImage.fillAmount = currentHealth / enemyData.enemyMaxHealth;
    }
}
