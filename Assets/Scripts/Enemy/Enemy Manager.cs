using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [SerializeField] public List<IDamageable> allEnemies = new List<IDamageable>();
    PlayerController player;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        Debug.Log(allEnemies.Count);   
    }

    public void RegisterEnemy(IDamageable enemy)
    {
        if (!allEnemies.Contains(enemy))
            allEnemies.Add(enemy);
    }

    public void UnregisterEnemy(IDamageable enemy)
    {
        if (allEnemies.Contains(enemy))
        {
            player.VampireHeal();
            allEnemies.Remove(enemy);
        }
        if (allEnemies.Count < 1) 
        {
            player.Win();
        }
    }

    public IDamageable GetClosestEnemy(Vector3 position, float range)
    {
        IDamageable closest = null;
        float closestDist = float.MaxValue;

        foreach (var enemy in allEnemies)
        {
            float dist = Vector3.Distance(position, enemy.transform.position);
            if (dist <= range && dist < closestDist)
            {
                closest = enemy;
                closestDist = dist;
            }
        }
        return closest;
    }
}


