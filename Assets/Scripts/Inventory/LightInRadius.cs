using UnityEngine;

public class LightInRadius : MonoBehaviour
{
    private float _damage; // Урон в радиусе
    public float Damage
    {
        get { return _damage; }
        set { _damage = value; }
    }
    void OnTriggerStay(Collider other) //Пока объект в сцене, наносить ему урон
    {
        if (other.GetComponent<IDamageable>() != null)
        {
            other.GetComponent<IDamageable>().TakeDamage(_damage, this.transform.position, this.transform.position);
        }
    }
}
