using UnityEngine;

public interface IDamageable
{
    Transform transform { get; }
    public void TakeDamage(float amount);
}
