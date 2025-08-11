using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 100f;
    public float lifeTime = 3f;
    public float damage = 3f;
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
        Debug.Log($"MoveDirection: {moveDirection}, Speed: {speed}");
        rb.linearVelocity = moveDirection * speed;
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<IDamageable>().TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
