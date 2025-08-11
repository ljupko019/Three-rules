using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 100f;
    public float lifeTime = 3f;
    private Vector2 moveDirection;

    private Rigidbody2D rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {

    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * speed;
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(10);
            Destroy(gameObject);
        }
        if (!collision.gameObject.CompareTag("Player")) 
        {
            Destroy(gameObject);
        }
    }
}
