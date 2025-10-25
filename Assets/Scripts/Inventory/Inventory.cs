using UnityEngine;
using UnityEngine.InputSystem;

/// Слот инвентаря для хранения предметов
[System.Serializable]
public class InventorySlot
{
    public Item Item; // Предмет в слоте
    public int Quantity; // Количество предметов
    public bool IsEmpty => Item == null || Quantity == 0; // Проверка на пустоту слота
    
    /// Очищает слот от предметов
    public void Clear()
    {
        Item = null;
        Quantity = 0;
    }
    
    /// Проверяет, можно ли добавить предмет в этот слот
    public bool CanAddItem(Item newItem)
    {
        return IsEmpty || (Item == newItem && Quantity < Item.MaxStack);
    }
}
/// Система инвентаря для управления предметами игрока
public class Inventory : MonoBehaviour 
{
    [SerializeField] private int _slotCount = 9; // Количество слотов в инвентаре
    [SerializeField] private InventorySlot[] _slots; // Массив слотов инвентаря
    [SerializeField] private Transform _handTransform; // Трансформ для размещения предметов в руке
    [SerializeField] private Item _activeItem; // Текущий активный предмет
    
    private int _activeItemSlot = -1; // Индекс активного слота
    private bool _allowToUse = true; // Разрешение на использование предмета
    private Item _tempItem; // Временный предмет для подбора
    private GameObject _tempItemObject; // Объект временного предмета
    private int _tempItemQuantity; // Количество временного предмета
    private GameObject _activeItemObject; // Объект активного предмета в руке

    // События для обновления UI
    public System.Action<InventorySlot[]> OnInventoryChanged;
    
    /// Количество слотов в инвентаре
    public int SlotCount => _slotCount;
    private void Awake()
    {
        InitializeSlots();
    }
    
    /// Инициализирует слоты инвентаря
    private void InitializeSlots()
    {
        _slots = new InventorySlot[_slotCount];
        for (int i = 0; i < _slotCount; i++)
        {
            _slots[i] = new InventorySlot();
        }
    }
    
    /// Добавляет предмет в инвентарь
    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;
        
        // Пытаемся добавить в существующий стек
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].Item == item && _slots[i].Quantity < item.MaxStack)
            {
                int spaceLeft = item.MaxStack - _slots[i].Quantity;
                int addAmount = Mathf.Min(quantity, spaceLeft);
                
                _slots[i].Quantity += addAmount;
                quantity -= addAmount;
                
                if (quantity <= 0)
                {
                    OnInventoryChanged?.Invoke(_slots);
                    return true;
                }
            }
        }
        
        // Ищем пустой слот
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].IsEmpty)
            {
                _slots[i].Item = item;
                _slots[i].Quantity = quantity;
                OnInventoryChanged?.Invoke(_slots);
                return true;
            }
        }
        
        return false; // Инвентарь полон
    }
    
    /// Удаляет предмет из слота
    public void RemoveItem(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Length || quantity <= 0) return;
        
        _slots[slotIndex].Quantity -= quantity;
        if (_slots[slotIndex].Quantity <= 0)
        {
            _slots[slotIndex].Clear();
        }
        
        OnInventoryChanged?.Invoke(_slots);
    }
    
    /// Меняет местами предметы в двух слотах
    public void SwapSlots(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _slots.Length || 
            toIndex < 0 || toIndex >= _slots.Length || fromIndex == toIndex) return;
        
        (Item tempItem, int tempQuantity) = (_slots[fromIndex].Item, _slots[fromIndex].Quantity);
        
        _slots[fromIndex].Item = _slots[toIndex].Item;
        _slots[fromIndex].Quantity = _slots[toIndex].Quantity;
        
        _slots[toIndex].Item = tempItem;
        _slots[toIndex].Quantity = tempQuantity;
        
        OnInventoryChanged?.Invoke(_slots);
    }

    /// Получает слот по индексу
    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= _slots.Length) return null;
        return _slots[index];
    }

    /// Подбирает временный предмет по нажатию E
    public void AddItemByE(InputAction.CallbackContext context)
    {
        if (!context.started || _tempItem == null) return;

        if (AddItem(_tempItem, _tempItemQuantity))
        {
            if (_tempItemObject != null)
            {
                Destroy(_tempItemObject);
            }
            ClearTempItem();
        }
    }
    
    /// Устанавливает временный предмет для подбора
    public void TempItem(Item item, GameObject gameObject, int quantity)
    {
        _tempItem = item;
        _tempItemObject = gameObject;
        _tempItemQuantity = quantity;
    }

    /// Очищает временный предмет
    private void ClearTempItem()
    {
        _tempItem = null;
        _tempItemObject = null;
        _tempItemQuantity = 0;
    }
    
    /// Выбирает предмет из слота по нажатию цифровой клавиши
    public void ChooseItem(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        
        string keyName = context.control.name;
        if (!int.TryParse(keyName, out int index)) return;
        
        index--; // Преобразуем в индекс массива (1-9 -> 0-8)
        
        if (index < 0 || index >= _slots.Length) return;
        
        // Если выбран тот же слот, что и активный - убираем предмет
        if (_activeItemSlot == index)
        {
            ClearActiveItem();
            return;
        }
        
        // Если в выбранном слоте нет предмета - ничего не делаем
        if (_slots[index].IsEmpty)
        {
            return;
        }
        
        // Устанавливаем новый активный предмет
        SetActiveItem(index, _slots[index].Item);
    }

    /// Устанавливает активный предмет
    private void SetActiveItem(int slotIndex, Item item)
    {
        ClearActiveItem();
        
        _activeItem = item;
        _activeItemSlot = slotIndex;

        if (_activeItem == null) return;
        
        // Создаем объект предмета в руке
        if (_activeItem.Type == Item.ItemType.Weapon || _activeItem.Type == Item.ItemType.Throw)
        {
            if (_activeItem is MainWeapon weapon && weapon.Weapon != null)
            {
                _activeItemObject = Instantiate(weapon.Weapon, _handTransform.position, _handTransform.rotation, _handTransform);
                
                // Для метательных предметов делаем кинематическими
                if (_activeItem.Type == Item.ItemType.Throw)
                {
                    var rb = _activeItemObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                    }
                }
            }
        }
    }

    /// Очищает активный предмет
    private void ClearActiveItem(bool needToDestroy = true)
    {
        if (_activeItemObject != null)
        {
            if (needToDestroy)
            {
                Destroy(_activeItemObject);
            }
            _activeItemObject = null;
        }
        
        _activeItem = null;
        _activeItemSlot = -1;
    }

    /// Использует активный предмет (основное действие)
    public void ActiveItemMain(InputAction.CallbackContext context)
    {
        if (_activeItem?.Type != Item.ItemType.Weapon || _activeItemObject == null) return;

        var weaponComponent = _activeItemObject.GetComponent<IWeapon>();
        if (weaponComponent == null) return;

        if (context.started && _allowToUse)
        {
            weaponComponent.Use();
            _allowToUse = false;

            if (_activeItem is MainWeapon weapon)
            {
                Invoke(nameof(ResetCooldown), weapon.Cooldown);
            }
        }
        else if (context.canceled)
        {
            weaponComponent.StopUse();
        }
    }
    /// Бросает активный предмет
    public void ThrowActiveItem(InputAction.CallbackContext context)
    {
        if (!context.started || _activeItem?.Type != Item.ItemType.Throw || _activeItemObject == null) return;

        var weaponComponent = _activeItemObject.GetComponent<IWeapon>();
        var rb = _activeItemObject.GetComponent<Rigidbody>();

        if (weaponComponent != null)
        {
            weaponComponent.Use();
        }

        RemoveItem(_activeItemSlot);

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        ClearActiveItem(false);
    }
    
    /// Сбрасывает кулдаун использования предмета
    private void ResetCooldown()
    {
        _allowToUse = true;
    }
    /// Поворачивает предмет в руке к камере
    public void RotateItem(InputAction.CallbackContext context)
    {
        if (!context.performed || _activeItem?.Type != Item.ItemType.Weapon) return;
        
        if (Camera.main != null)
        {
            _handTransform.rotation = Camera.main.transform.rotation * Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
