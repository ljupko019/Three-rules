using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boss : MonoBehaviour
{
    [SerializeField] float health = 300;

    [SerializeField] GameObject bullet;
    private GameObject player;
    private float coolDown = 4;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(CrabShoot());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (health <= 0)
        {
            SceneManager.LoadScene(5);
            Destroy(this.gameObject);
        }

        if (health < 50) 
        {
            coolDown = 2;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BulletPlayer")
        {
            health-= collision.gameObject.GetComponent<PlayerController>().GetDamage();
            Debug.Log("Hit !!");
        }

    }

    IEnumerator CrabShoot()
    {      
        Instantiate(bullet, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(3f);
        StartCoroutine(CrabShoot());
    }
}
