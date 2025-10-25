using UnityEngine;
[CreateAssetMenu(fileName = "Weapon", menuName = "Objects/Weapon")]
public class MainWeapon : Item //Объект главного оружия
{
    public GameObject Weapon;
    public float Damage;
    public float Cooldown;
}
