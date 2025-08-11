using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcController : MonoBehaviour, IDamageable
{
    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Hurt,
        Death,
    }


    public EnemyData enemyData;

    private float health;
    private float damage;
    [SerializeField] private EnemyState currentState;

    private List<Vector3> path;
    private int currentPathIndex;


    private Transform player;

    public float chaseRange = 10f;
    public float attackRange = 1.5f;
    public float pathUpdateRate = 0.5f;

    private float lastPathUpdateTime;
    private float lastAttackTime;
    private float attackCooldown = 2.0f;
    public bool canAttack = true;

    Animator animator;
    private Vector2 lastMoveDir;
    private bool hasDealtDamageThisAttack = false;

    [SerializeField] private Collider2D attackHitbox;

    private Vector3 hitboxOffsetRight = new Vector3(1, 0, 0);
    private Vector3 hitboxOffsetLeft = new Vector3(-1, 0, 0);
    private Vector3 hitboxOffsetUp = new Vector3(0, 1, 0);
    private Vector3 hitboxOffsetDown = new Vector3(0, -1, 0);

    private void Awake()
    {
        animator = GetComponent<Animator>();
        health = enemyData.maxHealth;
        currentState = EnemyState.Idle;
   
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        StartCoroutine(RegisterEnemyDelayed());
    }

    private void Update()
    {
        if (player == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToPlayer <= chaseRange)
                {
                    ChangeState(EnemyState.Chasing);
                }
                break;

            case EnemyState.Chasing:
                if (distanceToPlayer <= attackRange)
                {
                    ChangeState(EnemyState.Attacking);
                }
                //else if (distanceToPlayer > chaseRange)
                //{
                //    ChangeState(EnemyState.Idle);
                //}
                else
                {
                    UpdatePath();
                    MoveAlongPath();
                }
                break;

            case EnemyState.Attacking:
                if (distanceToPlayer > attackRange)
                {
                    ChangeState(EnemyState.Chasing);
                }
                else
                {                    
                    if (canAttack)
                    {
                        AttackPlayer();
                    }
                }
                break;

            case EnemyState.Hurt:
                break;
        }
    }

    private void ChangeState(EnemyState newState)
    {
        currentState = newState;
        currentPathIndex = 0;
        path = null;
       
    }

    private void UpdatePath()
    {
        if (Time.time - lastPathUpdateTime > pathUpdateRate)
        {
            lastPathUpdateTime = Time.time;
            path = Pathfinding.Instance.FindPath(transform.position, player.position);
            currentPathIndex = 0;
            if (path != null && path.Count > 1)
                path.RemoveAt(0); // ukloni trenutno mesto (poziciju gde je enemy)
        }
    }

    private void MoveAlongPath()
    {
        if (currentState == EnemyState.Hurt) return; // ne pomera se dok je hurt

        if (path == null || currentPathIndex >= path.Count) return;

        Vector3 targetPos = path[currentPathIndex];
        Vector3 moveDir = (targetPos - transform.position).normalized;

        lastMoveDir = new Vector2(moveDir.x, moveDir.y);

        RunningAnim();

        transform.position += moveDir * enemyData.speed * Time.deltaTime;
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            currentPathIndex++;
        }
    }

    private void AttackPlayer()
    {
        //if (!canAttack)
        //{
        //    return;
        //}
        hasDealtDamageThisAttack = false; //resetovanje pre napada
        canAttack = false;
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        AttackAnim(dirToPlayer);
        StartCoroutine(ResetAttackCooldown());
        Debug.Log($"{enemyData.enemyName} is attacking the player!");
    }
    public void OnAttackStart()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        ActivateHitbox(dirToPlayer);
        hasDealtDamageThisAttack = false; // resetuj flag za nanošenje štete
    }

    public void OnAttackEnd()
    {
        attackHitbox.enabled = false;
    }

    //private void DealDamageToPlayer() 
    //{
    //    if (hasDealtDamageThisAttack)
    //        return; //imamo 3 animation event-a za napad, da se ne bi 3 puta prizvao napad


    //    Vector3 dirToPlayer = (player.position - transform.position).normalized;

        
    //    float angle = Vector3.Angle(lastMoveDir, dirToPlayer);

    //    float attackAngleThreshold = 60f;

    //    if (angle > attackAngleThreshold)
    //    {
    //        Debug.Log($"Attack missed, player not in front (angle: {angle})");
    //        return;
    //    }
    //    hasDealtDamageThisAttack = true;

    //    player.GetComponent<PlayerController>().TakeDamage(10);
    //}
    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log($"Orc took {amount} damage, health left: {health}");

        HurtAnim();
        ChangeState(EnemyState.Hurt);
      
        if (health <= 0)
            Die();

        StartCoroutine(RecoverFromHurt());
    }


    private void Die()
    {
        ChangeState(EnemyState.Death);
        animator.SetTrigger("Death");
        EnemyManager.Instance.UnregisterEnemy(this);
        Destroy(gameObject, 1f); //unistava se nakon 1s, da bi se zavrsila animacija
    }

    private void RunningAnim() 
    {
        animator.SetFloat("moveX", lastMoveDir.x);
        animator.SetFloat("moveY", lastMoveDir.y);
        animator.SetBool("isRunning", true);
    }

    private void AttackAnim(Vector3 dirToPlayer) 
    {
        animator.SetBool("isRunning", false);
        animator.SetFloat("moveX", dirToPlayer.x);
        animator.SetFloat("moveY", dirToPlayer.y);
        animator.SetTrigger("Attack");
    }

    private void HurtAnim() 
    {
        animator.SetTrigger("Hurt");
    }

    private void ActivateHitbox(Vector2 attackDirection)
    {
        if (Mathf.Abs(attackDirection.x) > Mathf.Abs(attackDirection.y))
        {
            if (attackDirection.x > 0)
                attackHitbox.transform.localPosition = hitboxOffsetRight;
            else
                attackHitbox.transform.localPosition = hitboxOffsetLeft;
        }
        else
        {
            if (attackDirection.y > 0)
                attackHitbox.transform.localPosition = hitboxOffsetUp;
            else
                attackHitbox.transform.localPosition = hitboxOffsetDown;
        }

        attackHitbox.enabled = true;
        //canDealDamage = true;
    }

    private IEnumerator RegisterEnemyDelayed()
    {
        yield return new WaitForSeconds(0.5f);

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RegisterEnemy(this);
        }
        else
        {
            Debug.LogError("EnemyManager instance not found! Uveri se da EnemyManager postoji u sceni.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!attackHitbox.enabled) 
        {
            Debug.Log("aaaa");
            return;
        }
        

        if (collision.CompareTag("Player") && !hasDealtDamageThisAttack)
        {
            hasDealtDamageThisAttack = true;
            collision.GetComponent<PlayerController>().TakeDamage(enemyData.damage);
            Debug.Log($"{enemyData.enemyName} dealt damage to player!");
        }
    }

    private IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator RecoverFromHurt()
    {
        yield return new WaitForSeconds(0.4f);
        if (currentState == EnemyState.Hurt && health > 0)
            ChangeState(EnemyState.Chasing);
    }
}
