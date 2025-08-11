using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "Enemy";
    public float maxHealth = 100f;
    public float speed = 3f;
    public float damage = 10f;
}
