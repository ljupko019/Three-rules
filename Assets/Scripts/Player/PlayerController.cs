using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamageable
{
    

    private enum State 
    {
        Normal,
        Dashing,
        Attacking,
        Death,
    }
    private float speed = 10f;
    private float startrSpeed = 10f;
    private Rigidbody2D rb;
    private Vector3 moveDir;
    private Vector3 lastMoveDir;
    [SerializeField]private State state;

    public bool isSpear = true;
    public bool isGun = false;
    private float ammo = 20; 

    [SerializeField] GameObject bulletPrefab;

    private Vector3 dashDir;
    private float dashSpeed;

    private bool isInvulnerable = false;
    private float dodgeCooldown = 1.0f;
    private float lastDodgeTime = -10f;

    Animator animator;

    private bool isAttacking = false;
    private float damageSpear = 15f;
    private float damageGun = 10f;
    private float health = 100;

    public float timeRemaining = 60;

    private bool canHeal = true;
    private bool canUseGun = true;
    private bool isOneHitDeath = false;
    private bool isTimeLimited = false;
    private bool canDash = true;
    private bool isVulnerable = false;

    private bool isVampire = false;
    private bool isInfiniteAmmo = false;

    [SerializeField] Transform startPosition;
    [SerializeField] GameObject winObject;
    [SerializeField] TMP_Text ammoText;
    [SerializeField] Slider slider;

    private bool isPositionSet = false;
    private bool isWinObjSet = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        DontDestroyOnLoad(this);        
    }

    private void Update()
    {
        if (startPosition == null)
        {
            startPosition = GameObject.FindGameObjectWithTag("Start position").transform;
        }
        else if(!isPositionSet)
        {
            transform.position = startPosition.position;
            isPositionSet = true;
        }

        if (winObject == null)
        {
            winObject = GameObject.FindGameObjectWithTag("Scene Loader");
        }
        else if (!isWinObjSet)
        {
            isWinObjSet = true;
            winObject.SetActive(false);
        }

        switch (state) 
        {
            case State.Normal:
                HandleMovement();
                if (Input.GetKey(KeyCode.Space) && Time.time - lastDodgeTime > dodgeCooldown && canDash)
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
            case State.Death:
                break;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isSpear = true;
            isGun = false;
            animator.SetTrigger("Spear");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && canUseGun)
        {
            animator.SetTrigger("Gun");
            isSpear = false;
            isGun = true;
        }

        if (Input.GetKeyDown(KeyCode.P)) 
        {
            Heal(10);
        }
        if (isTimeLimited) 
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0) 
            {
                Death();
            }
        }

        if (slider == null)
        {
            slider = GameObject.Find("Slider").GetComponent<Slider>();

        }
        else 
        {
            slider.value = health;
        }

        if (ammoText == null)
        {
            ammoText = GameObject.Find("Ammo Text").GetComponent<TMP_Text>();
        }
        else
        {
            ammoText.text = "Ammo: " + ammo;
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
            case State.Death:
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

    public void Win() 
    {
        winObject.SetActive(true);
    }
    public void UnsetUI() 
    {
        slider = null;
        ammoText = null;
    }
    private void HandleSpearAttack() 
    {
        if (Input.GetMouseButtonDown(0) && state != State.Attacking)
        {
            Vector3 mousePosition = GetMousePostion();
            Vector3 mouseDir = (mousePosition - transform.position).normalized;
            SetAttackAnim(mouseDir);
            
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
        if (!canUseGun) 
        {
            isSpear = true; 
            return;
        }
        if (Input.GetMouseButtonDown(0) && state != State.Attacking && (ammo > 0 || isInfiniteAmmo))
        {
            if (!isInfiniteAmmo) 
            {
                ammo--;
            }
            Vector3 mousePosition = GetMousePostion();
            Vector3 mouseDir = (mousePosition - transform.position).normalized;
            SetAttackAnim(mouseDir);
            state = State.Attacking;

            float spawnDistance = 1.5f;
            Vector3 spawnPos = transform.position + mouseDir * spawnDistance;

            GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetDirection(mouseDir);
                bullet.Setdamage(damageGun);
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
        if (isOneHitDeath)
        {
            Death();
        }
        else
        {
            if (isInvulnerable)
                return;

            if (isVulnerable) 
            {
                amount *= 2f;
            }

            health -= amount;
            if (health <= 0)
            {
                Death();
            }
        }
    }
    public void Heal(int heal) 
    {
        if (canHeal)
        {
            health += heal;
            if (health > 100)
            {
                health = 100;
            }
        }
    }

    public void VampireHeal() 
    {
        health += 5;
        if (health > 100)
        {
            health = 100;
        }
    }
    public void SetCanHeal(bool heal) 
    {
        canHeal = heal;
    }

    public void SetCanUseGun(bool gunUse) 
    {
        canUseGun = gunUse;
    }

    public void SetOneHitDeath(bool oneHitDeath) 
    {
        isOneHitDeath = oneHitDeath;
    }

    public void SetIsTimeLimited(bool timeLimited) 
    {
        isTimeLimited = timeLimited;
    }
    public void SetSlowerSpeed() 
    {
        speed = speed * 0.7f;
    }
    public void SetNormalSpeed() 
    {
        speed = startrSpeed;
    }
    public void SetOffDash(bool disableDash)
    {
        canDash = disableDash;
    }
    public void SetVulnerable(bool vulnerable) 
    {
        isVulnerable = vulnerable;
    }

    public void SetDoubleDamage() 
    {
        damageSpear *= 2;
        damageGun *= 2;
    }

    public void UnsetDoubleDamage()
    {
        damageSpear /= 2;
        damageGun /= 2;
    }

    public void SetVampireTouch(bool vampire) 
    {
        isVampire = vampire;
    }
    public void SetSpeedIncrease() 
    {
        speed *= 1.7f;
    }
    public void UnetSpeedIncrease()
    {
        speed = startrSpeed;
    }
    public void SetInfiniteAmmo(bool ammoInfinite) 
    {
        isInfiniteAmmo = ammoInfinite;
    }
    private void Death() 
    {
        SceneManager.LoadScene(4);
        animator.SetTrigger("Death");
        Destroy(this, 2);
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

    private void SetAttackAnim(Vector3 direction)
    {
        animator.SetFloat("AnimAttackDirX", direction.x);
        animator.SetFloat("AnimAttackDirY", direction.y);
    }

    public float GetDamage() 
    {
        if (isSpear)
        {
            return damageSpear;
        }
        else 
        {
            return damageGun;
        }
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
