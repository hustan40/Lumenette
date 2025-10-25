using UnityEngine;

/// UI контроллер для отображения инвентаря игрока
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform _slotsParent; // Родительский объект для слотов
    [SerializeField] private GameObject _slotPrefab; // Префаб слота инвентаря
    
    private InventorySlotUI[] _slotUIs; // Массив UI слотов
    private Inventory _inventory; // Ссылка на инвентарь
    
    private void Start()
    {
        _inventory = Object.FindFirstObjectByType<Inventory>();
        
        if (_inventory == null)
        {
            Debug.LogError("Инвентарь не найден на сцене");
            return;
        }
        
        InitializeUI();
        
        // Подписываемся на событие изменения инвентаря
        _inventory.OnInventoryChanged += UpdateUI;
    }
    
    /// Инициализирует UI слоты инвентаря
    private void InitializeUI()
    {
        if (_slotPrefab == null || _slotsParent == null)
        {
            Debug.LogError("Родительского слота или префаба нету");
            return;
        }
        
        _slotUIs = new InventorySlotUI[_inventory.SlotCount];
        
        for (int i = 0; i < _inventory.SlotCount; i++)
        {
            GameObject slotObject = Instantiate(_slotPrefab, _slotsParent);
            var slotUI = slotObject.GetComponent<InventorySlotUI>();
            
            if (slotUI == null)
            {
                Debug.LogError($"Слот префаба не имеет InventorySlotUI");
                continue;
            }
            
            _slotUIs[i] = slotUI;
            _slotUIs[i].Initialize(i, _inventory);
        }
    }
    
    /// Обновляет отображение всех слотов инвентаря
    private void UpdateUI(InventorySlot[] slots)
    {
        if (_slotUIs == null || slots == null) return;
        
        int maxIndex = Mathf.Min(_slotUIs.Length, slots.Length);
        
        for (int i = 0; i < maxIndex; i++)
        {
            if (_slotUIs[i] != null)
            {
                _slotUIs[i].UpdateSlot(slots[i]);
            }
        }
    }
    
    /// Отписывается от событий при уничтожении объекта
    private void OnDestroy()
    {
        if (_inventory != null)
        {
            _inventory.OnInventoryChanged -= UpdateUI;
        }
    }
}
