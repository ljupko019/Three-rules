using UnityEngine;

[CreateAssetMenu(fileName = "OrcData", menuName = "OrcData")]
public class OrcData : ScriptableObject
{
    public string enemyName = "Enemy";
    public float maxHealth = 100f;
    public float speed = 3f;
    public float damage = 10f;
}
