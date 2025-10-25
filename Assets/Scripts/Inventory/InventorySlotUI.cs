using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// UI элемент слота инвентаря с поддержкой drag & drop
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage; // Иконка предмета
    [SerializeField] private TMP_Text _quantityText; // Текст количества
    
    private int _slotIndex; // Индекс слота в инвентаре
    private Inventory _inventory; // Ссылка на инвентарь
    private CanvasGroup _canvasGroup; // Для управления прозрачностью при перетаскивании
    private Vector3 _originalPosition; // Исходная позиция для возврата после drag
    
    /// Инициализирует слот UI с привязкой к инвентарю
    public void Initialize(int index, Inventory inv)
    {
        _slotIndex = index;
        _inventory = inv;
        _canvasGroup = GetComponent<CanvasGroup>();
        
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        _originalPosition = transform.localPosition;
        ClearSlotVisuals();
    }
    
    /// Начало перетаскивания слота
    public void OnBeginDrag(BaseEventData data)
    {
        if (_inventory?.GetSlot(_slotIndex)?.IsEmpty != false) return;
        
        _canvasGroup.alpha = 0.6f;
        _canvasGroup.blocksRaycasts = false;
        _originalPosition = transform.localPosition;
    }
    
    /// Процесс перетаскивания слота
    public void OnDrag(BaseEventData data)
    {
        if (!(data is PointerEventData pointerData) || _inventory?.GetSlot(_slotIndex)?.IsEmpty != false) return;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            pointerData.position,
            pointerData.pressEventCamera,
            out Vector2 localPoint))
        {
            transform.localPosition = localPoint;
        }
    }
    
    /// Завершение перетаскивания слота
    public void OnEndDrag(BaseEventData data)
    {
        // Восстанавливаем визуальное состояние
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
        transform.localPosition = _originalPosition;
        
        // Обрабатываем цель сброса
        if (data is PointerEventData pointerData)
        {
            HandleDropTarget(pointerData.pointerEnter);
        }
    }
    
    /// Обрабатывает цель сброса при drag & drop
    private void HandleDropTarget(GameObject dropTarget)
    {
        if (dropTarget == null) return;
        
        var targetSlot = dropTarget.GetComponent<InventorySlotUI>();
        if (targetSlot != null && targetSlot != this && _inventory != null)
        {
            _inventory.SwapSlots(_slotIndex, targetSlot._slotIndex);
        }
    }
       
    /// Обновляет визуальное отображение слота
    public void UpdateSlot(InventorySlot slot)
    {
        if (slot?.IsEmpty != false)
        {
            ClearSlotVisuals();
        }
        else
        {
            SetSlotVisuals(slot);
        }
    }
    
    /// Очищает визуальное отображение слота
    private void ClearSlotVisuals()
    {
        if (_iconImage != null)
        {
            _iconImage.sprite = null;
            _iconImage.color = Color.clear;
        }
        
        if (_quantityText != null)
        {
            _quantityText.text = "";
        }
    }
    
    /// Устанавливает визуальное отображение для предмета
    private void SetSlotVisuals(InventorySlot slot)
    {
        if (_iconImage != null && slot.Item?.Icon != null)
        {
            _iconImage.sprite = slot.Item.Icon;
            _iconImage.color = Color.white;
        }
        
        if (_quantityText != null)
        {
            _quantityText.text = slot.Quantity > 1 ? slot.Quantity.ToString() : "";
        }
    }
}