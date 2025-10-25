using UnityEngine;
using System.Collections;

//Тестовый код двери, который срабатывает при зарядки кристала
public class DoorInteract : MonoBehaviour, IInteractable
{
    private float _angle = -0.7f; // угол открытия
    public void Interact()
    {
        StartCoroutine(OpenDoor()); //Начало корутины
    }
    private IEnumerator OpenDoor()
    {
        while (transform.rotation.y > _angle)
        {
            transform.Rotate(0, -0.5f, 0);
            yield return new WaitForEndOfFrame();
        }
    }
}
