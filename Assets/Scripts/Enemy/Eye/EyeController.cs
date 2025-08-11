using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeController : MonoBehaviour, IDamageable
{
    public enum EnemyState
    {
        Idle,
        Moving,
        Attacking,
        Hurt,
        Death
    }

    public EnemyData enemyData;
    public GameObject projectilePrefab;
    public float attackRange = 12f;
    public float stopDistance = 5f;
    public float pathUpdateRate = 0.5f;
    public float attackCooldown = 2f;

    private float health;
    private bool canAttack = true;
    private Transform player;
    private EnemyState currentState;
    private Vector2 lastMoveDir;

    private List<Vector3> path;
    private int currentPathIndex;
    private float lastPathUpdateTime;

    private void Awake()
    {
        health = enemyData.maxHealth;
        currentState = EnemyState.Idle;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        StartCoroutine(RegisterEnemyDelayed());
    }

    private void Update()
    {
        if (player == null || currentState == EnemyState.Death)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToPlayer <= attackRange)
                    ChangeState(EnemyState.Moving);
                break;

            case EnemyState.Moving:
                UpdatePath();
                MoveAlongPath();

                if (distanceToPlayer <= attackRange && canAttack)
                    ChangeState(EnemyState.Attacking);
                break;

            case EnemyState.Attacking:
                if (distanceToPlayer > attackRange)
                    ChangeState(EnemyState.Moving);
                else if (canAttack)
                    AttackPlayer();
                break;

            case EnemyState.Hurt:
                break;
        }
    }

    private void UpdatePath()
    {
        if (Time.time - lastPathUpdateTime > pathUpdateRate)
        {
            lastPathUpdateTime = Time.time;
            path = Pathfinding.Instance.FindPath(transform.position, player.position);
            currentPathIndex = 0;

            if (path != null && path.Count > 1)
                path.RemoveAt(0); 
        }
    }

    private void MoveAlongPath()
    {
        if (currentState == EnemyState.Hurt) return;
        if (path == null || currentPathIndex >= path.Count)
        {
            return;
        }

        Vector3 targetPos = path[currentPathIndex];
        Vector3 directionToTarget = (targetPos - transform.position).normalized;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= stopDistance)
        {
            return;
        }

        transform.position += directionToTarget * enemyData.speed * Time.deltaTime;

        lastMoveDir = new Vector2(directionToTarget.x, directionToTarget.y);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            currentPathIndex++;
    }

    private void AttackPlayer()
    {
        canAttack = false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        ShootProjectile();
        StartCoroutine(ResetAttackCooldown());
    }

    public void ShootProjectile()
    {
        if (projectilePrefab != null)
        {            
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float spawnDistance = 1.5f;
            Vector3 spawnPos = transform.position + dirToPlayer * spawnDistance;
            
            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            EnemyBullet projectile = proj.GetComponent<EnemyBullet>();

            if (projectile != null)
            {
                projectile.SetDirection(dirToPlayer);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        Debug.Log("aaa");
        health -= amount;
        ChangeState(EnemyState.Hurt);

        if (health <= 0)
        {
            EnemyManager.Instance.UnregisterEnemy(this);
            Destroy(this);
        }
            
            
        else
            StartCoroutine(RecoverFromHurt());
    }

    private void Die()
    {
        //ChangeState(EnemyState.Death);
        EnemyManager.Instance.UnregisterEnemy(this);
        Destroy(gameObject);
    }

    private IEnumerator RecoverFromHurt()
    {
        yield return new WaitForSeconds(0.4f);
        if (currentState == EnemyState.Hurt && health > 0)
            ChangeState(EnemyState.Moving);
    }

    private void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }

    private IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator RegisterEnemyDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.RegisterEnemy(this);
        else
            Debug.LogError("EnemyManager instance not found");
    }
}
