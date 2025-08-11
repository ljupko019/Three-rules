using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [SerializeField] private List<OrcController> allEnemies = new List<OrcController>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterEnemy(OrcController enemy)
    {
        if (!allEnemies.Contains(enemy))
            allEnemies.Add(enemy);
    }

    public void UnregisterEnemy(OrcController enemy)
    {
        if (allEnemies.Contains(enemy))
            allEnemies.Remove(enemy);
    }

    public OrcController GetClosestEnemy(Vector3 position, float range)
    {
        OrcController closest = null;
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


