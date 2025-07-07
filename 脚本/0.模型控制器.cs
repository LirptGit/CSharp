using UnityEngine;
using System.Collections;

/// <summary>
/// 模型控制器 - 提供旋转、移动和缩放功能，支持平滑过渡和立即重置
/// </summary>
public class ModelControllerWorldRotation : MonoBehaviour
{
    // ========== 旋转设置 ==========
    [Header("旋转设置")]
    [Tooltip("鼠标拖动时的旋转速度")]
    public float rotationSpeed = 3f;
    [Tooltip("X轴旋转的最小角度限制")]
    public float minXRotation = -80f;
    [Tooltip("X轴旋转的最大角度限制")]
    public float maxXRotation = 80f;
    [Tooltip("旋转动画的平滑时间（值越大越平滑但响应越慢）")]
    public float rotationSmoothTime = 0.1f;
    [Tooltip("旋转方向（1为正常方向，-1为反向旋转）")]
    public int direction = 1;
    [Tooltip("重置旋转时的默认角度")]
    public Vector3 defaultRotation = Vector3.zero;
    [Tooltip("重置时旋转平滑时间的加速倍率（值越大重置越快）")]
    public float resetSpeedMultiplier = 5f;
    [Tooltip("是否启用旋转功能")]
    public bool enableRotation = true;

    // ========== 移动设置 ==========
    [Header("移动设置")]
    [Tooltip("鼠标拖动时的移动速度")]
    public float moveSpeed = 5f;
    [Tooltip("X轴移动的最小和最大范围")]
    public Vector2 xMoveLimit = new Vector2(-10, 10);
    [Tooltip("Y轴移动的最小和最大范围")]
    public Vector2 yMoveLimit = new Vector2(-5, 5);
    [Tooltip("Z轴移动的最小和最大范围")]
    public Vector2 zMoveLimit = new Vector2(-10, 10);
    [Tooltip("移动动画的平滑时间")]
    public float moveSmoothTime = 0.1f;
    [Tooltip("是否启用移动功能")]
    public bool enableMovement = true;

    // ========== 缩放设置 ==========
    [Header("缩放设置")]
    [Tooltip("滚轮缩放的速度")]
    public float zoomSpeed = 5f;
    [Tooltip("缩放的最小和最大范围")]
    public Vector2 scaleLimit = new Vector2(0.5f, 3f);
    [Tooltip("缩放动画的平滑时间")]
    public float zoomSmoothTime = 0.1f;
    [Tooltip("重置缩放时的默认大小")]
    public float defaultScale = 1f;
    [Tooltip("是否启用缩放功能")]
    public bool enableZoom = true;

    // ========== 私有变量 ==========
    private Vector3 _currentRotation;      // 当前实际旋转角度
    private Vector3 _targetRotation;       // 目标旋转角度
    private Vector3 _rotationVelocity;     // 旋转平滑速度
    private Vector3 _targetPosition;       // 目标位置
    private float _targetScale;            // 目标缩放值
    private Vector3 _positionVelocity;     // 位置平滑速度
    private float _scaleVelocity;          // 缩放平滑速度
    private Vector2 _lastMousePosition;    // 上一帧鼠标位置（用于计算移动/旋转增量）
    private bool _isResetting = false;     // 是否正在重置中
    private Coroutine _resetCoroutine;     // 重置协程引用
    private bool _useImmediateReset = false; // 是否使用立即重置模式

    /// <summary>
    /// 初始化时设置初始值
    /// </summary>
    void Start()
    {
        InitializeValues();
    }

    /// <summary>
    /// 初始化所有变量值为当前transform的状态
    /// </summary>
    void InitializeValues()
    {
        _currentRotation = transform.eulerAngles;
        _targetRotation = _currentRotation;
        _targetPosition = transform.position;
        _targetScale = transform.localScale.x;
    }

    /// <summary>
    /// 每帧更新处理输入和平滑过渡
    /// </summary>
    void Update()
    {
        // 非重置状态才处理输入
        if (!_isResetting)
        {
            if (enableRotation) HandleRotation();
            if (enableMovement) HandleMovement();
            if (enableZoom) HandleZoom();
        }

        // 应用平滑过渡效果
        ApplySmoothTransforms();
    }

    /// <summary>
    /// 处理旋转输入（右键拖动）
    /// </summary>
    void HandleRotation()
    {
        // 右键按下时记录初始状态
        if (Input.GetMouseButtonDown(1))
        {
            _lastMousePosition = Input.mousePosition;
            _currentRotation = transform.eulerAngles;
            _targetRotation = _currentRotation;
        }

        // 右键按住时计算旋转
        if (Input.GetMouseButton(1))
        {
            Vector2 currentMousePos = Input.mousePosition;
            Vector2 mouseDelta = currentMousePos - _lastMousePosition;

            // 根据鼠标移动计算目标旋转角度
            _targetRotation += new Vector3(
                -mouseDelta.y * rotationSpeed * 0.1f * direction,  // X轴旋转（上下移动）
                mouseDelta.x * rotationSpeed * 0.1f * direction,   // Y轴旋转（左右移动）
                0                                                 // Z轴不旋转
            );

            // 限制X轴旋转角度
            _targetRotation.x = ClampAngle(_targetRotation.x, minXRotation, maxXRotation);
            _lastMousePosition = currentMousePos;
        }
    }

    /// <summary>
    /// 处理移动输入（左键拖动）
    /// </summary>
    void HandleMovement()
    {
        // 左键按下时记录初始状态
        if (Input.GetMouseButtonDown(0))
        {
            _lastMousePosition = Input.mousePosition;
        }

        // 左键按住时计算移动
        if (Input.GetMouseButton(0))
        {
            Vector2 currentMousePos = Input.mousePosition;
            Vector2 mouseDelta = (currentMousePos - _lastMousePosition) * moveSpeed * 0.01f;

            // 将屏幕空间移动转换为世界空间移动
            Vector3 moveOffset = Camera.main.transform.TransformDirection(new Vector3(mouseDelta.x, mouseDelta.y, 0));
            moveOffset.z = 0; // 保持Z轴不变（2D平面移动）
            _targetPosition += moveOffset;

            // 限制移动范围
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, xMoveLimit.x, xMoveLimit.y);
            _targetPosition.y = Mathf.Clamp(_targetPosition.y, yMoveLimit.x, yMoveLimit.y);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, zMoveLimit.x, zMoveLimit.y);

            _lastMousePosition = currentMousePos;
        }
    }

    /// <summary>
    /// 处理缩放输入（鼠标滚轮）
    /// </summary>
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // 根据滚轮输入调整目标缩放值，并限制在范围内
            _targetScale = Mathf.Clamp(_targetScale + scroll * zoomSpeed, scaleLimit.x, scaleLimit.y);
        }
    }

    /// <summary>
    /// 应用平滑过渡效果到transform
    /// </summary>
    void ApplySmoothTransforms()
    {
        // 立即重置模式：直接设置目标值，不进行平滑
        if (_useImmediateReset)
        {
            transform.eulerAngles = _targetRotation;
            transform.position = _targetPosition;
            transform.localScale = Vector3.one * _targetScale;
            _useImmediateReset = false; // 重置标志
            return;
        }

        // ===== 平滑旋转 =====
        // 根据是否重置状态选择平滑时间
        float currentRotationSmoothTime = _isResetting ?
            rotationSmoothTime / resetSpeedMultiplier : rotationSmoothTime;

        // 使用SmoothDampAngle平滑过渡每个旋转轴（处理360度环绕）
        float smoothX = Mathf.SmoothDampAngle(_currentRotation.x, _targetRotation.x, ref _rotationVelocity.x, currentRotationSmoothTime);
        float smoothY = Mathf.SmoothDampAngle(_currentRotation.y, _targetRotation.y, ref _rotationVelocity.y, currentRotationSmoothTime);
        float smoothZ = Mathf.SmoothDampAngle(_currentRotation.z, _targetRotation.z, ref _rotationVelocity.z, currentRotationSmoothTime);

        _currentRotation = new Vector3(smoothX, smoothY, smoothZ);
        transform.eulerAngles = _currentRotation;

        // ===== 平滑移动 =====
        transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _positionVelocity, moveSmoothTime);

        // ===== 平滑缩放 =====
        float currentScale = Mathf.SmoothDamp(transform.localScale.x, _targetScale, ref _scaleVelocity, zoomSmoothTime);
        transform.localScale = Vector3.one * currentScale;

        // 检查重置是否完成（所有属性都接近目标值）
        if (_isResetting &&
            Vector3.Distance(_currentRotation, _targetRotation) < 0.1f &&
            Vector3.Distance(transform.position, _targetPosition) < 0.1f &&
            Mathf.Abs(transform.localScale.x - _targetScale) < 0.01f)
        {
            _isResetting = false;
        }
    }

    /// <summary>
    /// 角度限制方法（处理360度环绕）
    /// </summary>
    /// <param name="angle">当前角度</param>
    /// <param name="min">最小角度</param>
    /// <param name="max">最大角度</param>
    /// <returns>限制后的角度</returns>
    float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180) angle -= 360;  // 将角度转换到-180~180范围
        angle = Mathf.Clamp(angle, min, max);
        if (angle < 0) angle += 360;    // 转换回0~360范围
        return angle;
    }

    /// <summary>
    /// 立即重置位置到原点
    /// </summary>
    public void ResetPositionImmediate()
    {
        _targetPosition = Vector3.zero;
        _positionVelocity = Vector3.zero; // 重置速度
        _currentRotation = transform.eulerAngles; // 保持当前旋转
        _targetScale = transform.localScale.x; // 保持当前缩放
        _useImmediateReset = true; // 启用立即重置模式
    }

    /// <summary>
    /// 立即重置旋转到默认角度
    /// </summary>
    public void ResetRotationImmediate()
    {
        _targetRotation = defaultRotation;
        _rotationVelocity = Vector3.zero; // 重置速度
        _currentRotation = _targetRotation; // 同步当前值
        _targetPosition = transform.position; // 保持当前位置
        _targetScale = transform.localScale.x; // 保持当前缩放
        _useImmediateReset = true; // 启用立即重置模式
    }

    /// <summary>
    /// 立即重置缩放到默认大小
    /// </summary>
    public void ResetScaleImmediate()
    {
        _targetScale = defaultScale;
        _scaleVelocity = 0f; // 重置速度
        _targetPosition = transform.position; // 保持当前位置
        _currentRotation = transform.eulerAngles; // 保持当前旋转
        _useImmediateReset = true; // 启用立即重置模式
    }

    /// <summary>
    /// 平滑重置所有状态（位置、旋转、缩放）
    /// </summary>
    /// <param name="duration">重置持续时间（秒）</param>
    public void ResetAllSmooth(float duration = 0.5f)
    {
        // 如果已有重置协程在运行，先停止
        if (_resetCoroutine != null)
        {
            StopCoroutine(_resetCoroutine);
        }
        _resetCoroutine = StartCoroutine(ResetAllCoroutine(duration));
    }

    /// <summary>
    /// 重置所有状态的协程
    /// </summary>
    private IEnumerator ResetAllCoroutine(float duration)
    {
        _isResetting = true; // 标记为重置状态
        float originalMultiplier = resetSpeedMultiplier;
        resetSpeedMultiplier = 10f; // 加速重置过程

        // 记录初始值用于插值
        Vector3 startPos = transform.position;
        Vector3 startRot = transform.eulerAngles;
        float startScale = transform.localScale.x;

        // 设置目标值
        _targetPosition = Vector3.zero;
        _targetRotation = defaultRotation;
        _targetScale = defaultScale;

        // 插值过程
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration); // 计算插值比例

            // 使用Lerp进行平滑过渡（避免SmoothDamp可能导致的卡顿）
            transform.position = Vector3.Lerp(startPos, _targetPosition, t);
            transform.eulerAngles = new Vector3(
                Mathf.LerpAngle(startRot.x, _targetRotation.x, t),
                Mathf.LerpAngle(startRot.y, _targetRotation.y, t),
                Mathf.LerpAngle(startRot.z, _targetRotation.z, t)
            );
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, _targetScale, t);

            yield return null;
        }

        // 确保最终状态准确
        ResetAllImmediate();

        // 恢复原始设置
        resetSpeedMultiplier = originalMultiplier;
        _isResetting = false;
        _resetCoroutine = null;
    }

    /// <summary>
    /// 立即重置所有状态（位置、旋转、缩放）
    /// </summary>
    public void ResetAllImmediate()
    {
        // 设置目标值
        _targetPosition = Vector3.zero;
        _targetRotation = defaultRotation;
        _targetScale = defaultScale;

        // 同步当前值
        _currentRotation = _targetRotation;

        // 重置所有速度
        _positionVelocity = Vector3.zero;
        _rotationVelocity = Vector3.zero;
        _scaleVelocity = 0f;

        // 直接设置transform
        transform.position = _targetPosition;
        transform.eulerAngles = _targetRotation;
        transform.localScale = Vector3.one * _targetScale;

        // 启用立即重置模式（确保下一帧不会平滑）
        _useImmediateReset = true;
    }

    /// <summary>
    /// 在场景视图中绘制辅助Gizmos
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 绘制移动范围立方体
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(
            (xMoveLimit.x + xMoveLimit.y) * 0.5f,
            (yMoveLimit.x + yMoveLimit.y) * 0.5f,
            (zMoveLimit.x + zMoveLimit.y) * 0.5f
        );
        Vector3 size = new Vector3(
            xMoveLimit.y - xMoveLimit.x,
            yMoveLimit.y - yMoveLimit.x,
            zMoveLimit.y - zMoveLimit.x
        );
        Gizmos.DrawWireCube(center, size);

        // 绘制原点位置标记
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(Vector3.zero, 0.2f);
    }
}