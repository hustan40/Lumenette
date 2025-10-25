using UnityEngine;
using System.Collections;

/// Миниган-посох - автоматическое оружие с непрерывной стрельбой
public class StaffMinigun : MonoBehaviour, IWeapon
{
    [SerializeField] private Item _item; // Предмет-оружие для получения характеристик
    [SerializeField] private Transform _shootPoint; // Точка выстрела
    [SerializeField] private LineRenderer _lineRenderer; // Визуализация лазерного луча
    [SerializeField] private float _maxDistance = 100f; // Максимальная дистанция выстрела
    [SerializeField] private float _timeRay = 0.1f; // Интервал между выстрелами
    [SerializeField] private LayerMask _hitLayer = ~0; // Слои для попадания
    [SerializeField] private ParticleSystem _shootEffect; // Эффект выстрела
    [SerializeField] private GameObject _hitEffectPrefab; // Префаб эффекта попадания
    
    private bool _isUse; // Флаг активного использования оружия
    private Coroutine _shootingCoroutine; // Корутина непрерывной стрельбы
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
        _isUse = false;
        
        if (_shootingCoroutine != null)
        {
            StopCoroutine(_shootingCoroutine);
            _shootingCoroutine = null;
        }
        
        // Отменяем все отложенные вызовы
        CancelInvoke(nameof(DisableRay));
        DisableRay();
    }

    /// Начинает использование оружия (непрерывная стрельба)
    public void Use()
    {
        if (_isUse || _mainCamera == null || _shootPoint == null) return;
        
        _isUse = true;
        _shootingCoroutine = StartCoroutine(ShootingRoutine());
    }

    /// Корутина непрерывной стрельбы
    private IEnumerator ShootingRoutine()
    {
        while (_isUse)
        {
            PerformShot();
            yield return new WaitForSeconds(_timeRay);
        }
        
        DisableRay();
    }

    /// Выполняет один выстрел
    private void PerformShot()
    {
        // Создаем луч из центра экрана
        Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        
        Vector3 endPoint = _shootPoint.position + ray.direction * _maxDistance;
        
        // Проверяем попадание
        if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _hitLayer))
        {
            endPoint = hit.point;
            HandleHit(hit);
        }

        DisplayLaser(endPoint);
    }

    /// Отображает лазерный луч
    private void DisplayLaser(Vector3 endPoint)
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
        
        // Отменяем предыдущий вызов и планируем новый
        CancelInvoke(nameof(DisableRay));
        Invoke(nameof(DisableRay), _timeRay);
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

    /// Отключает отображение лазерного луча
    private void DisableRay()
    {
        if (_lineRenderer != null && !_isUse)
        {
            _lineRenderer.enabled = false;
        }
    }

    /// Очистка при выключении объекта
    private void OnDisable()
    {
        StopUse();
    }
}