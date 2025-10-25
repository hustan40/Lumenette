using UnityEngine;

/// Метательное оружие с подсветкой, которое можно бросать в цель
public class ThrowWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private Item _item; // Предмет-оружие для получения характеристик
    [SerializeField] private float _forceThrow = 10f; // Сила броска
    [SerializeField] private LightInRadius _lightInRadius; // Компонент урона в радиусе света
    [SerializeField] private Light _light; // Источник света
    [SerializeField] private float _kinematicDelay = 5f; // Время до остановки физики
    [SerializeField] private float _lightDuration = 15f; // Время свечения после остановки
    
    private Rigidbody _rb; // Компонент физики
    private Camera _mainCamera; // Кэшированная ссылка на главную камеру
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _mainCamera = Camera.main;
        
        if (_rb == null)
        {
            Debug.LogError("Rigidbody не найден");
        }
        
        if (_mainCamera == null)
        {
            Debug.LogError("Камера не найдена");
        }
    }
    
    /// Прекращает использование оружия (не применимо для метательного оружия)
    public void StopUse()
    {
        // Метательное оружие не может быть остановлено после броска
    }

    /// Использует оружие (бросает его в направлении прицела)
    public void Use()
    {
        if (_rb == null || _mainCamera == null) return;
        
        // Создаем луч из центра экрана
        Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        
        // Настраиваем урон если есть компонент урона в радиусе
        if (_lightInRadius != null && _item is MainWeapon weapon)
        {
            _lightInRadius.Damage = weapon.Damage;
        }
        
        // Включаем свет
        if (_light != null)
        {
            _light.enabled = true;
        }
        
        // Настраиваем физику для броска
        _rb.isKinematic = false;
        transform.SetParent(null);
        
        // Бросаем в направлении луча
        Vector3 throwDirection = ray.direction;
        _rb.AddForce(throwDirection * _forceThrow);
        
        // Планируем остановку физики
        Invoke(nameof(SetKinematic), _kinematicDelay);
    }

    /// Останавливает физику объекта и планирует выключение света
    private void SetKinematic()
    {
        if (_rb != null)
        {
            _rb.isKinematic = true;
        }
        
        Invoke(nameof(SetDark), _lightDuration);
    }
    /// Выключает свет объекта
    private void SetDark()
    {
        if (_light != null)
        {
            _light.enabled = false;
        }
    }
    
    /// Очистка при выключении объекта
    private void OnDisable()
    {
        // Отменяем все отложенные вызовы
        CancelInvoke();
    }
}
