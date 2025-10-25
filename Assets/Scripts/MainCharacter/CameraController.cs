using UnityEngine;
using Unity.Cinemachine;
using Cysharp.Threading.Tasks;
using System.Threading;

/// Контроллер камеры для управления поворотом и зумом с использованием Cinemachine
public class CameraController
{
    private float _mouseSensitivity; // Чувствительность мыши
    private float _maxLookAngle; // Максимальный угол поворота по вертикали
    private float _xRotation; // Текущий поворот по X (вертикаль)
    private float _yRotation; // Текущий поворот по Y (горизонталь)
    private float _zoomValue; // Значение зума при прицеливании
    private float _zoomValueDefault; // Стандартное значение зума
    private float _zoomSpeed = 0.5f; // Скорость зума
    private GameObject _camera; // Объект камеры
    private CinemachineCamera _cameraCin; // Компонент Cinemachine камеры
    private CancellationTokenSource _cts; // Токен отмены для асинхронных операций

    /// Конструктор контроллера камеры
    public CameraController(GameObject camera, float mouseSensitivity, float maxLookAngle, float zoomValue)
    {
        _camera = camera;
        _mouseSensitivity = mouseSensitivity;
        _maxLookAngle = maxLookAngle;
        _zoomValue = zoomValue;
        
        if (_camera != null)
        {
            _cameraCin = _camera.GetComponent<CinemachineCamera>();
            
            if (_cameraCin != null)
            {
                _zoomValueDefault = _cameraCin.Lens.FieldOfView;
            }
            else
            {
                Debug.LogError("CinemachineCamera не найдена");
            }
        }
        else
        {
            Debug.LogError("Камера не найдена");
        }
        
        LockCursorToCenter();
    }

    /// Обрабатывает поворот камеры на основе ввода мыши
    public void HandleCameraRotation(Vector2 lookInput)
    {
        if (lookInput.sqrMagnitude < 0.01f || _camera == null) return;

        float mouseY = lookInput.y * _mouseSensitivity;
        float mouseX = lookInput.x * _mouseSensitivity;

        _yRotation += mouseX;
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -_maxLookAngle, _maxLookAngle);

        _camera.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
    }

    /// Устанавливает режим зума камеры
    public void SetZoom(bool zooming)
    {
        if (_cameraCin == null) return;
        
        // Отменяем предыдущую операцию зума
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        
        if (zooming)
        {
            ZoomIn(_cts.Token).Forget();
        }
        else
        {
            ZoomOut(_cts.Token).Forget();
        }
    }

    /// Плавно приближает камеру (уменьшает FOV)
    private async UniTaskVoid ZoomIn(CancellationToken token)
    {
        try
        {
            while (_cameraCin.Lens.FieldOfView > _zoomValue && !token.IsCancellationRequested)
            {
                _cameraCin.Lens.FieldOfView = Mathf.Max(_cameraCin.Lens.FieldOfView - _zoomSpeed, _zoomValue);
                await UniTask.NextFrame(cancellationToken: token);
            }
        }
        catch (System.OperationCanceledException)
        {
            // Операция отменена
        }
    }
  
    /// Плавно отдаляет камеру (увеличивает FOV)
    private async UniTaskVoid ZoomOut(CancellationToken token)
    {
        try
        {
            while (_cameraCin.Lens.FieldOfView < _zoomValueDefault && !token.IsCancellationRequested)
            {
                _cameraCin.Lens.FieldOfView = Mathf.Min(_cameraCin.Lens.FieldOfView + _zoomSpeed, _zoomValueDefault);
                await UniTask.NextFrame(cancellationToken: token);
            }
        }
        catch (System.OperationCanceledException)
        {
            // Операция отменена
        }
    }

    /// Блокирует курсор в центре экрана и скрывает его
    public void LockCursorToCenter()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// Разблокирует курсор и делает его видимым
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    /// Очищает ресурсы контроллера
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}