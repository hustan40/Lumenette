using UnityEngine;
using UnityEngine.InputSystem;

/// Контроллер движения игрока с поддержкой ходьбы, бега, прыжков и управления камерой
public class PlayerMovement : MonoBehaviour
{
    [Header("Настройка перемещения")]
    [SerializeField] private float _speedNormal = 5f; // Обычная скорость движения
    [SerializeField] private float _speedSprint = 8f; // Скорость бега
    [SerializeField] private float _koefMovementInAir = 0.3f; // Коэффициент управления в воздухе
    [SerializeField] private float _koefMovementInAim = 0.5f; // Коэффициент движения при прицеливании
    [SerializeField] private float _jumpHeight = 2f; // Максимальная высота прыжка
    [SerializeField] private float _jumpForce = 8f; // Сила прыжка
    [SerializeField] private float _gravity = -9.81f; // Сила гравитации

    [Header("Настройка камеры")]
    [SerializeField] private float _maxLookAngle = 90f; // Максимальный угол поворота камеры по вертикали
    [SerializeField] private float _zoomValue = 40f; // Значение зума при прицеливании
    [SerializeField] private GameObject _cameraGameobject; // Объект камеры
    [SerializeField] private float _mouseSensitivityX = 1f; // Чувствительность мыши по X
    [SerializeField] private float _mouseSensitivityY = 1f; // Чувствительность мыши по Y
    private CharacterController _controller; // Контроллер персонажа
    private Camera _camera; // Главная камера
    private Vector3 _velocity; // Текущая скорость движения
    private Vector2 _inputMove; // Ввод движения от игрока
    private Vector3 _currentHorizontalMovement; // Текущее горизонтальное движение
    private float _jumpStartY; // Начальная Y позиция прыжка
    private CameraController _cameraController; // Контроллер камеры
    private bool _isAiming; // Флаг прицеливания
    private Vector3 _previousPosition; // Предыдущая позиция для расчета скорости
    private Vector3 _currentVelocity; // Текущая скорость
    private Vector3 _currentHorizontalVelocity; // Текущая горизонтальная скорость
    private bool _isRunning; // Флаг бега

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _camera = Camera.main;

        if (_controller == null)
        {
            Debug.LogError("Контроллер игрока не найден");
        }

        if (_camera == null)
        {
            Debug.LogError("Камера на найдена");
        }

        if (_cameraGameobject != null)
        {
            _cameraController = new CameraController(_cameraGameobject, _mouseSensitivityY, _maxLookAngle, _zoomValue);
        }
        else
        {
            Debug.LogError("Объект камеры не найден");
        }

        _previousPosition = transform.position;
    }
    
    /// Обрабатывает ввод движения от игрока
    public void Move(InputAction.CallbackContext context)
    {
        _inputMove = context.ReadValue<Vector2>();
    }

    /// Обрабатывает ввод прыжка от игрока
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started && _controller != null && _controller.isGrounded)
        {
            _velocity.y = _jumpForce;
            _jumpStartY = transform.position.y;

            // Сохраняем текущую горизонтальную скорость для прыжка
            _velocity.x = _currentHorizontalMovement.x;
            _velocity.z = _currentHorizontalMovement.z;
        }
    }

    /// Обрабатывает ввод бега от игрока
    public void Sprint(InputAction.CallbackContext context)
    {
        _isRunning = context.ReadValueAsButton();
    }

    /// Обрабатывает ввод поворота камеры от игрока
    public void Rotate(InputAction.CallbackContext context)
    {
        Vector2 lookInput = context.ReadValue<Vector2>();
        HandleMouseLook(lookInput);

        if (_cameraController != null)
        {
            _cameraController.HandleCameraRotation(lookInput);
        }
    }

    /// Обрабатывает ввод прицеливания от игрока
    public void Aim(InputAction.CallbackContext context)
    {
        _isAiming = context.ReadValueAsButton();

        if (_cameraController != null)
        {
            _cameraController.SetZoom(_isAiming);
        }
    }
    
    /// Обрабатывает поворот игрока по горизонтали
    private void HandleMouseLook(Vector2 lookInput)
    {
        if (lookInput.sqrMagnitude < 0.01f) return;

        float mouseX = lookInput.x * _mouseSensitivityX;
        transform.Rotate(Vector3.up * mouseX);
    }
    private void Update()
    {
        if (_controller == null || _camera == null) return;
        
        // Расчет текущей скорости
        _currentVelocity = (transform.position - _previousPosition) / Time.deltaTime;
        _previousPosition = transform.position;
        
        // Обработка горизонтального движения
        HandleHorizontalMovement();
        
        // Обработка вертикального движения и гравитации
        HandleVerticalMovement();
        
        // Применение движения
        _controller.Move(_velocity * Time.deltaTime);
    }
    
    /// Обрабатывает горизонтальное движение игрока
    private void HandleHorizontalMovement()
    {
        // Преобразование ввода в направление движения относительно камеры
        Vector3 moveDirection = new Vector3(_inputMove.x, 0, _inputMove.y);
        moveDirection = _camera.transform.TransformDirection(moveDirection);
        moveDirection.y = 0;
        moveDirection.Normalize();

        // Расчет текущей скорости с учетом бега и прицеливания
        float currentSpeed = _isRunning ? _speedSprint : _speedNormal;
        if (_isAiming) currentSpeed *= _koefMovementInAim;

        Vector3 horizontalMovement = moveDirection * currentSpeed;
        _currentHorizontalMovement = horizontalMovement;
        
        if (_controller.isGrounded)
        {
            // На земле: прямое управление
            _velocity.x = horizontalMovement.x;
            _velocity.z = horizontalMovement.z;
            _currentHorizontalVelocity = new Vector3(_currentVelocity.x, 0, _currentVelocity.z);
        }
        else
        {
            // В воздухе: ограниченное управление
            Vector3 airControl = moveDirection * _speedNormal * _koefMovementInAir;
            _velocity.x = _currentHorizontalVelocity.x + airControl.x;
            _velocity.z = _currentHorizontalVelocity.z + airControl.z;
        }
    }
    
    /// Обрабатывает вертикальное движение и гравитацию
    private void HandleVerticalMovement()
    {
        if (_controller.isGrounded)
        {
            // На земле: сброс вертикальной скорости
            if (_velocity.y < 0)
            {
                _velocity.y = -2f; // Небольшое значение для удержания на земле
            }
        }
        else
        {
            // В воздухе: применение гравитации
            _velocity.y += _gravity * Time.deltaTime;
            
            // Ограничение высоты прыжка
            if (transform.position.y > _jumpStartY + _jumpHeight && _velocity.y > 0)
            {
                _velocity.y = 0;
            }
        }
    }
}
