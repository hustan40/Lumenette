using UnityEngine;

public interface IDamageable //Интерфейс получения урона
{
    abstract void TakeDamage(float damage, Vector3 hitPosition, Vector3 hitDirection);
}
