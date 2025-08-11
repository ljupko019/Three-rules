using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] int HP = 3;

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
        if (HP <= 0)
        {
            Destroy(this.gameObject);
        }

        if (HP < 50) 
        {
            coolDown = 2;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BulletPlayer")
        {
            HP -= 1;
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
