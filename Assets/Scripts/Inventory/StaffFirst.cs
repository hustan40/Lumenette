using UnityEngine;

/// Первый посох - оружие дальнего боя с лазерным лучом и перезарядкой
public class StaffFirst : MonoBehaviour, IWeapon
{
    [SerializeField] private Item _item; // Предмет-оружие для получения характеристик
    [SerializeField] private Transform _shootPoint; // Точка выстрела
    [SerializeField] private LineRenderer _lineRenderer; // Визуализация лазерного луча
    [SerializeField] private float _maxDistance = 100f; // Максимальная дистанция выстрела
    [SerializeField] private float _timeRay = 0.3f; // Время отображения луча
    [SerializeField] private LayerMask _hitLayer = ~0; // Слои для попадания
    [SerializeField] private ParticleSystem _shootEffect; // Эффект выстрела
    [SerializeField] private GameObject _hitEffectPrefab; // Префаб эффекта попадания

    private Camera _mainCamera; // Кэшированная ссылка на главную камеру

    private void Awake()
    {
        _mainCamera = Camera.main;

        if (_mainCamera == null)
        {
            Debug.LogError("Камера не найдена");
        }
    }

    /// Прекращает использование оружия
    public void StopUse()
    {
        // Останавливаем все ожидающие вызовы DisableRay
        CancelInvoke(nameof(DisableRay));
        // Немедленно выключаем луч при отпускании кнопки
        DisableRay();
    }

    /// Использует оружие (выстрел)
    public void Use()
    {
        if (_mainCamera == null || _shootPoint == null) return;

        // Отменяем предыдущий вызов, если луч ещё активен
        CancelInvoke(nameof(DisableRay));

        // Создаем луч из центра экрана
        Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));

        Vector3 endPoint = _shootPoint.position + ray.direction * _maxDistance;

        // Проверяем попадание
        if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _hitLayer))
        {
            endPoint = hit.point;
            HandleHit(hit);
        }

        // Показываем луч
        ShowLaser(endPoint);

        // Автоматически выключаем луч через указанное время
        Invoke(nameof(DisableRay), _timeRay);
    }

    /// Отображает лазерный луч
    private void ShowLaser(Vector3 endPoint)
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(0, _shootPoint.position);
            _lineRenderer.SetPosition(1, endPoint);
        }

        if (_shootEffect != null)
        {
            _shootEffect.Play();
        }
    }

    /// Отключает отображение лазерного луча
    private void DisableRay()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }
    }

    /// Обрабатывает попадание по цели
    private void HandleHit(RaycastHit hit)
    {
        if (hit.collider == null) return;

        // Нанесение урона
        var damageable = hit.collider.GetComponent<IDamageable>();
        if (damageable != null && _item is MainWeapon weapon)
        {
            Vector3 direction = GetNormalizedDirection(_shootPoint.position, hit.point);
            damageable.TakeDamage(weapon.Damage, hit.point, direction);
        }
        else
        {
            SpawnHitEffect(hit.point, hit.normal);
        }
    }
    /// Получает нормализованное направление между двумя точками
    private Vector3 GetNormalizedDirection(Vector3 from, Vector3 to)
    {
        return (to - from).normalized;
    }

    /// Создает эффект попадания в указанной позиции
    private void SpawnHitEffect(Vector3 position, Vector3 normal)
    {
        if (_hitEffectPrefab != null)
        {
            Instantiate(_hitEffectPrefab, position, Quaternion.LookRotation(normal));
        }
    }

    /// Очистка при выключении объекта
    private void OnDisable()
    {
        StopUse();
    }
}