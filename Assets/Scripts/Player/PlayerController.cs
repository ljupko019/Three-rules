using System.Collections;
using UnityEngine;
using UnityEngine.Diagnostics;

public class PlayerController : MonoBehaviour, IDamageable
{
    private float speed = 10f;

    private enum State 
    {
        Normal,
        Dashing,
        Attacking,
    }

    private Rigidbody2D rb;
    private Vector3 moveDir;
    private Vector3 lastMoveDir;
    [SerializeField]private State state;

    public bool isSpear = false;
    public bool isGun = true;

    [SerializeField] GameObject bulletPrefab;

    private Vector3 dashDir;
    private float dashSpeed;

    private bool isInvulnerable = false;
    private float dodgeCooldown = 1.0f;
    private float lastDodgeTime = -10f;

    Animator animator;

    private bool isAttacking = false;
    private float damageSpear = 10f;
    public float health = 100f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {

        switch (state) 
        {
            case State.Normal:
                HandleMovement();
                if (Input.GetKey(KeyCode.Space) && Time.time - lastDodgeTime > dodgeCooldown)
                {
                    //koristimo lastMoveDir umesto moveDir
                    //ukoliko se igrac ne krece kada se dash-uje
                    //dash-ovace se u smeru u kom se zadnji put kretao
                    dashDir = lastMoveDir;
                    dashSpeed = 50;
                    state = State.Dashing;
                }
                if (isSpear)
                {
                    HandleSpearAttack();
                }
                else if (isGun) 
                {
                    HandleGunAttack();
                }
                break;
            case State.Dashing:
                HandleDash();
                break;
            case State.Attacking:   
                break;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isSpear = true;
            isGun = false;
            animator.SetTrigger("Spear");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            isSpear = false;
            isGun = true;
        }


    }

    private void FixedUpdate()
    {
        switch (state) {
            case State.Normal:
                rb.linearVelocity = moveDir * speed;
                break;
            case State.Dashing:
                rb.linearVelocity = dashDir * dashSpeed;
                break;
            case State.Attacking:
                //kako se igrac ne bi kretao dok napada
                rb.linearVelocity = Vector3.zero;
                break;
        }
    }
    
    private void HandleMovement() 
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W))
        {
            moveY = +1f;

        }
        if (Input.GetKey(KeyCode.S))
        {
            moveY = -1f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveX = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveX = +1f;
        }
        moveDir = new Vector3(moveX, moveY).normalized;

        //ukoliko se igrac krece pamti se smer njegovog kretanja, zbog dash-a
        if (moveX != 0 || moveY != 0)
        {
            lastMoveDir = moveDir;
        }

        SetRunAnim(moveDir);
    }

    private void HandleSpearAttack() 
    {
        if (Input.GetMouseButtonDown(0) && state != State.Attacking)
        {
            Vector3 mousePosition = GetMousePostion();
            Vector3 mouseDir = (mousePosition - transform.position).normalized;
            SetSpearAttackAnim(mouseDir);
            
            float attckOffset = 1.5f;
            Vector3 attackPostition = transform.position + mouseDir * attckOffset;
            float attackRange = 1;
            var targetEnemy = EnemyManager.Instance.GetClosestEnemy(attackPostition, attackRange);
            if (targetEnemy != null)
            {
                targetEnemy.TakeDamage(damageSpear);
            }
            state = State.Attacking;
            
            animator.SetBool("isAttacking", true);
           
            StartCoroutine(SpearAttackCoolDown());
        }

    }

    private void HandleGunAttack() 
    {
        if (Input.GetMouseButtonDown(0) && state != State.Attacking)
        {
            Vector3 mousePosition = GetMousePostion();
            Vector3 mouseDir = (mousePosition - transform.position).normalized;
            //SetSpearAttackAnim(mouseDir);

            Instantiate(bulletPrefab);
            state = State.Attacking;

            float spawnDistance = 1.5f;
            Vector3 spawnPos = transform.position + mouseDir * spawnDistance;

            GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                Debug.Log("okej");
                bullet.SetDirection(mouseDir);
            }

            animator.SetBool("isAttacking", true);

            StartCoroutine(GunAttackCoolDown());
        }
    }

    private void HandleDash() 
    {
        float dashSpeedDropMultiplier = 5f;
        dashSpeed -= dashSpeed * dashSpeedDropMultiplier * Time.deltaTime;
        SetDashAnim(lastMoveDir);
        animator.SetBool("isDash", true);

        float dashSpeedMin = 15f;
        if (dashSpeed < dashSpeedMin)
        {
            animator.SetBool("isDash", false);
            state = State.Normal;
            isInvulnerable = false;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isInvulnerable)
            return;

        health -= amount;
        if (health <= 0) 
        {
            Death();
        }
    }

    private void Death() 
    {

    }
    private void SetDashAnim(Vector3 direction)
    {
        animator.SetFloat("AnimLastMoveX", direction.x);
        animator.SetFloat("AnimLastMoveY", direction.y);
    }

    private void SetRunAnim(Vector3 direction)
    {
        animator.SetFloat("AnimMoveX", direction.x);
        animator.SetFloat("AnimMoveY", direction.y);
    }

    private void SetSpearAttackAnim(Vector3 direction)
    {
        animator.SetFloat("AnimAttackDirX", direction.x);
        animator.SetFloat("AnimAttackDirY", direction.y);
    }

    private Vector3 GetMousePostion()
    {
        Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        vector.z = 0f;
        return vector;
    }

    IEnumerator SpearAttackCoolDown() 
    {
        //posle 0.3s se zavrsava animacija
        float attackDuration = 0.66f / 2;
        yield return new WaitForSeconds(attackDuration);      
        animator.SetBool("isAttacking", false);
        

        yield return new WaitForSeconds(0.05f);
        state = State.Normal;
    }

    IEnumerator GunAttackCoolDown()
    {
        //posle 0.3s se zavrsava animacija
        float attackDuration = 0.66f / 2;
        yield return new WaitForSeconds(attackDuration);
        animator.SetBool("isAttacking", false);


        yield return new WaitForSeconds(0.05f);
        state = State.Normal;
    }
}
