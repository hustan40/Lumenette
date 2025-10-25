using UnityEngine;

/// Компонент для подбора предметов в мире игры
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private Item _item; // Предмет для подбора
    [SerializeField] private int _quantity = 1; // Количество предметов
    [SerializeField] private CanvasGroup _canvasGroup; // UI элемент для отображения информации
    [SerializeField] private float _distanceToDisplay = 5f; // Максимальная дистанция для отображения UI
    
    private bool _isTemp; // Флаг временного предмета в инвентаре
    private Inventory _inventory; // Ссылка на инвентарь игрока
    private Camera _mainCamera; // Кэшированная ссылка на главную камеру

    void Awake()
    {
        _inventory = FindAnyObjectByType<Inventory>();
        _mainCamera = Camera.main;

        if (_inventory == null)
        {
            Debug.LogError("Инвентарь не найден на сцене");
        }

        if (_mainCamera == null)
        {
            Debug.LogError("Камера не найдена");
        }

        // Скрываем UI элемент при старте
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0;
        }
    }
    
    /// Вызывается при наведении мыши на предмет
    public void OnMouseEnter()
    {
        if (_mainCamera == null || _inventory == null) return;
        
        float distance = Vector3.Distance(transform.position, _mainCamera.transform.position);
        
        if (distance <= _distanceToDisplay)
        {
            ShowUI();
            SetTempItem();
        }
    }

    /// Вызывается при уходе мыши с предмета
    public void OnMouseExit()
    {
        HideUI();
        ClearTempItem();
    }
    
    /// Показывает UI элемент предмета
    private void ShowUI()
    {
        if (_canvasGroup == null) return;
        
        _canvasGroup.alpha = 1;
        
        // Поворачиваем UI к камере
        if (_mainCamera != null)
        {
            _canvasGroup.transform.rotation = _mainCamera.transform.rotation;
        }
    }
    
    /// Скрывает UI элемент предмета
    private void HideUI()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0;
        }
    }
    
    /// Устанавливает предмет как временный в инвентаре
    private void SetTempItem()
    {
        if (_isTemp || _inventory == null || _item == null) return;
        
        _inventory.TempItem(_item, gameObject, _quantity);
        _isTemp = true;
    }
    
    /// Очищает временный предмет из инвентаря
    private void ClearTempItem()
    {
        if (!_isTemp || _inventory == null) return;
        
        _inventory.TempItem(null, null, 0);
        _isTemp = false;
    }
}
