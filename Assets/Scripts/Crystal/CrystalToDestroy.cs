using System.Collections;
using UnityEngine;

/// Кристалл, который уничтожается при достижении максимального здоровья
public class CrystalToDestroy : MonoBehaviour, IDamageable
{
    [SerializeField] private Light _light; // Источник света кристалла
    [SerializeField] private float _maxHealth; // Максимальное здоровье кристалла
    [SerializeField] private float _maxLight; // Максимальная интенсивность света
    [SerializeField] private float _speedFade = 1f; // Скорость затухания света
    [SerializeField] private GameObject _toCreate; // Объект для создания при уничтожении
    
    private float _currentHealth; // Текущее здоровье кристалла
    private Coroutine _fadeCoroutine; // Корутина затухания света
    void Awake()
    {
        _currentHealth = 0;
        
        // Если максимальная интенсивность не задана, используем текущую
        if (_maxLight == 0)
        {
            _maxLight = _light.intensity;
        }
        
        // Устанавливаем начальную интенсивность света
        UpdateLightIntensity();
    }

    /// Получение урона 
    public void TakeDamage(float damage, Vector3 hitPosition, Vector3 hitDirection)
    {
        // Останавливаем предыдущую корутину затухания
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        // Увеличиваем здоровье 
        _currentHealth += damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        
        // Обновляем интенсивность света
        UpdateLightIntensity();
        
        // Проверяем состояние кристалла
        if (_currentHealth >= _maxHealth)
        {
            DestroyAndCreate();
        }
        else if (_currentHealth > 0)
        {
            _fadeCoroutine = StartCoroutine(FadeLight());
        }
    }

    /// Корутина постепенного затухания света
    private IEnumerator FadeLight()
    {
        while (_currentHealth > 0)
        {
            _currentHealth -= Time.deltaTime * _speedFade;
            _currentHealth = Mathf.Max(_currentHealth, 0);
            
            UpdateLightIntensity();
            
            yield return null; 
        }
    }
    /// Уничтожает кристалл и создает новый объект на его месте
    private void DestroyAndCreate()
    {
        if (_toCreate != null)
        {
            Instantiate(_toCreate, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
    
    /// Обновляет интенсивность света на основе текущего здоровья
    private void UpdateLightIntensity()
    {
        if (_light != null && _maxHealth > 0)
        {
            _light.intensity = (_currentHealth / _maxHealth) * _maxLight;
        }
    }
}
