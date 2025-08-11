using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BossBullet : MonoBehaviour
{
    public Vector3 target;
    int damage;
    private GameObject player;
    Animator animator;
    bool isNearToPLayer = false;
    float timer = 0;
    [SerializeField]float explosionTime = 2;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        damage = 10;
    }

    // Update is called once per frame
    void Update()
    {
        float step = 8 * Time.deltaTime;
        timer += Time.deltaTime;

      
        

        if (timer > explosionTime)
        {
            damage = 25;
            StartCoroutine(dissapear());
            animator.SetBool("explosion", true);
            isNearToPLayer = true;
            //this.gameObject.GetComponent<CircleCollider2D>().enabled = true;
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, step);
        }


    }

    public void SetPosition(Vector3 postion) 
    {
        target = postion;
    }
    IEnumerator dissapear()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(this.gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.tag == "Player")
        {
            collision.GetComponent<IDamageable>().TakeDamage(damage);
            timer = int.MaxValue;
        }

    }
}
