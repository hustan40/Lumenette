using UnityEngine;

//Кристалл, который создается при попадании луча в какой-либо объект и уничтожается через 10 секунды
public class HitCrystal : MonoBehaviour
{
    void Awake()
    {
        Destroy(gameObject, 10f);
    }
}
