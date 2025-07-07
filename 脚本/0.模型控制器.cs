using UnityEngine;
using System.Collections;

/// <summary>
/// ģ�Ϳ����� - �ṩ��ת���ƶ������Ź��ܣ�֧��ƽ�����ɺ���������
/// </summary>
public class ModelControllerWorldRotation : MonoBehaviour
{
    // ========== ��ת���� ==========
    [Header("��ת����")]
    [Tooltip("����϶�ʱ����ת�ٶ�")]
    public float rotationSpeed = 3f;
    [Tooltip("X����ת����С�Ƕ�����")]
    public float minXRotation = -80f;
    [Tooltip("X����ת�����Ƕ�����")]
    public float maxXRotation = 80f;
    [Tooltip("��ת������ƽ��ʱ�䣨ֵԽ��Խƽ������ӦԽ����")]
    public float rotationSmoothTime = 0.1f;
    [Tooltip("��ת����1Ϊ��������-1Ϊ������ת��")]
    public int direction = 1;
    [Tooltip("������תʱ��Ĭ�ϽǶ�")]
    public Vector3 defaultRotation = Vector3.zero;
    [Tooltip("����ʱ��תƽ��ʱ��ļ��ٱ��ʣ�ֵԽ������Խ�죩")]
    public float resetSpeedMultiplier = 5f;
    [Tooltip("�Ƿ�������ת����")]
    public bool enableRotation = true;

    // ========== �ƶ����� ==========
    [Header("�ƶ�����")]
    [Tooltip("����϶�ʱ���ƶ��ٶ�")]
    public float moveSpeed = 5f;
    [Tooltip("X���ƶ�����С�����Χ")]
    public Vector2 xMoveLimit = new Vector2(-10, 10);
    [Tooltip("Y���ƶ�����С�����Χ")]
    public Vector2 yMoveLimit = new Vector2(-5, 5);
    [Tooltip("Z���ƶ�����С�����Χ")]
    public Vector2 zMoveLimit = new Vector2(-10, 10);
    [Tooltip("�ƶ�������ƽ��ʱ��")]
    public float moveSmoothTime = 0.1f;
    [Tooltip("�Ƿ������ƶ�����")]
    public bool enableMovement = true;

    // ========== �������� ==========
    [Header("��������")]
    [Tooltip("�������ŵ��ٶ�")]
    public float zoomSpeed = 5f;
    [Tooltip("���ŵ���С�����Χ")]
    public Vector2 scaleLimit = new Vector2(0.5f, 3f);
    [Tooltip("���Ŷ�����ƽ��ʱ��")]
    public float zoomSmoothTime = 0.1f;
    [Tooltip("��������ʱ��Ĭ�ϴ�С")]
    public float defaultScale = 1f;
    [Tooltip("�Ƿ��������Ź���")]
    public bool enableZoom = true;

    // ========== ˽�б��� ==========
    private Vector3 _currentRotation;      // ��ǰʵ����ת�Ƕ�
    private Vector3 _targetRotation;       // Ŀ����ת�Ƕ�
    private Vector3 _rotationVelocity;     // ��תƽ���ٶ�
    private Vector3 _targetPosition;       // Ŀ��λ��
    private float _targetScale;            // Ŀ������ֵ
    private Vector3 _positionVelocity;     // λ��ƽ���ٶ�
    private float _scaleVelocity;          // ����ƽ���ٶ�
    private Vector2 _lastMousePosition;    // ��һ֡���λ�ã����ڼ����ƶ�/��ת������
    private bool _isResetting = false;     // �Ƿ�����������
    private Coroutine _resetCoroutine;     // ����Э������
    private bool _useImmediateReset = false; // �Ƿ�ʹ����������ģʽ

    /// <summary>
    /// ��ʼ��ʱ���ó�ʼֵ
    /// </summary>
    void Start()
    {
        InitializeValues();
    }

    /// <summary>
    /// ��ʼ�����б���ֵΪ��ǰtransform��״̬
    /// </summary>
    void InitializeValues()
    {
        _currentRotation = transform.eulerAngles;
        _targetRotation = _currentRotation;
        _targetPosition = transform.position;
        _targetScale = transform.localScale.x;
    }

    /// <summary>
    /// ÿ֡���´��������ƽ������
    /// </summary>
    void Update()
    {
        // ������״̬�Ŵ�������
        if (!_isResetting)
        {
            if (enableRotation) HandleRotation();
            if (enableMovement) HandleMovement();
            if (enableZoom) HandleZoom();
        }

        // Ӧ��ƽ������Ч��
        ApplySmoothTransforms();
    }

    /// <summary>
    /// ������ת���루�Ҽ��϶���
    /// </summary>
    void HandleRotation()
    {
        // �Ҽ�����ʱ��¼��ʼ״̬
        if (Input.GetMouseButtonDown(1))
        {
            _lastMousePosition = Input.mousePosition;
            _currentRotation = transform.eulerAngles;
            _targetRotation = _currentRotation;
        }

        // �Ҽ���סʱ������ת
        if (Input.GetMouseButton(1))
        {
            Vector2 currentMousePos = Input.mousePosition;
            Vector2 mouseDelta = currentMousePos - _lastMousePosition;

            // ��������ƶ�����Ŀ����ת�Ƕ�
            _targetRotation += new Vector3(
                -mouseDelta.y * rotationSpeed * 0.1f * direction,  // X����ת�������ƶ���
                mouseDelta.x * rotationSpeed * 0.1f * direction,   // Y����ת�������ƶ���
                0                                                 // Z�᲻��ת
            );

            // ����X����ת�Ƕ�
            _targetRotation.x = ClampAngle(_targetRotation.x, minXRotation, maxXRotation);
            _lastMousePosition = currentMousePos;
        }
    }

    /// <summary>
    /// �����ƶ����루����϶���
    /// </summary>
    void HandleMovement()
    {
        // �������ʱ��¼��ʼ״̬
        if (Input.GetMouseButtonDown(0))
        {
            _lastMousePosition = Input.mousePosition;
        }

        // �����סʱ�����ƶ�
        if (Input.GetMouseButton(0))
        {
            Vector2 currentMousePos = Input.mousePosition;
            Vector2 mouseDelta = (currentMousePos - _lastMousePosition) * moveSpeed * 0.01f;

            // ����Ļ�ռ��ƶ�ת��Ϊ����ռ��ƶ�
            Vector3 moveOffset = Camera.main.transform.TransformDirection(new Vector3(mouseDelta.x, mouseDelta.y, 0));
            moveOffset.z = 0; // ����Z�᲻�䣨2Dƽ���ƶ���
            _targetPosition += moveOffset;

            // �����ƶ���Χ
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, xMoveLimit.x, xMoveLimit.y);
            _targetPosition.y = Mathf.Clamp(_targetPosition.y, yMoveLimit.x, yMoveLimit.y);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, zMoveLimit.x, zMoveLimit.y);

            _lastMousePosition = currentMousePos;
        }
    }

    /// <summary>
    /// �����������루�����֣�
    /// </summary>
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // ���ݹ����������Ŀ������ֵ���������ڷ�Χ��
            _targetScale = Mathf.Clamp(_targetScale + scroll * zoomSpeed, scaleLimit.x, scaleLimit.y);
        }
    }

    /// <summary>
    /// Ӧ��ƽ������Ч����transform
    /// </summary>
    void ApplySmoothTransforms()
    {
        // ��������ģʽ��ֱ������Ŀ��ֵ��������ƽ��
        if (_useImmediateReset)
        {
            transform.eulerAngles = _targetRotation;
            transform.position = _targetPosition;
            transform.localScale = Vector3.one * _targetScale;
            _useImmediateReset = false; // ���ñ�־
            return;
        }

        // ===== ƽ����ת =====
        // �����Ƿ�����״̬ѡ��ƽ��ʱ��
        float currentRotationSmoothTime = _isResetting ?
            rotationSmoothTime / resetSpeedMultiplier : rotationSmoothTime;

        // ʹ��SmoothDampAngleƽ������ÿ����ת�ᣨ����360�Ȼ��ƣ�
        float smoothX = Mathf.SmoothDampAngle(_currentRotation.x, _targetRotation.x, ref _rotationVelocity.x, currentRotationSmoothTime);
        float smoothY = Mathf.SmoothDampAngle(_currentRotation.y, _targetRotation.y, ref _rotationVelocity.y, currentRotationSmoothTime);
        float smoothZ = Mathf.SmoothDampAngle(_currentRotation.z, _targetRotation.z, ref _rotationVelocity.z, currentRotationSmoothTime);

        _currentRotation = new Vector3(smoothX, smoothY, smoothZ);
        transform.eulerAngles = _currentRotation;

        // ===== ƽ���ƶ� =====
        transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _positionVelocity, moveSmoothTime);

        // ===== ƽ������ =====
        float currentScale = Mathf.SmoothDamp(transform.localScale.x, _targetScale, ref _scaleVelocity, zoomSmoothTime);
        transform.localScale = Vector3.one * currentScale;

        // ��������Ƿ���ɣ��������Զ��ӽ�Ŀ��ֵ��
        if (_isResetting &&
            Vector3.Distance(_currentRotation, _targetRotation) < 0.1f &&
            Vector3.Distance(transform.position, _targetPosition) < 0.1f &&
            Mathf.Abs(transform.localScale.x - _targetScale) < 0.01f)
        {
            _isResetting = false;
        }
    }

    /// <summary>
    /// �Ƕ����Ʒ���������360�Ȼ��ƣ�
    /// </summary>
    /// <param name="angle">��ǰ�Ƕ�</param>
    /// <param name="min">��С�Ƕ�</param>
    /// <param name="max">���Ƕ�</param>
    /// <returns>���ƺ�ĽǶ�</returns>
    float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180) angle -= 360;  // ���Ƕ�ת����-180~180��Χ
        angle = Mathf.Clamp(angle, min, max);
        if (angle < 0) angle += 360;    // ת����0~360��Χ
        return angle;
    }

    /// <summary>
    /// ��������λ�õ�ԭ��
    /// </summary>
    public void ResetPositionImmediate()
    {
        _targetPosition = Vector3.zero;
        _positionVelocity = Vector3.zero; // �����ٶ�
        _currentRotation = transform.eulerAngles; // ���ֵ�ǰ��ת
        _targetScale = transform.localScale.x; // ���ֵ�ǰ����
        _useImmediateReset = true; // ������������ģʽ
    }

    /// <summary>
    /// ����������ת��Ĭ�ϽǶ�
    /// </summary>
    public void ResetRotationImmediate()
    {
        _targetRotation = defaultRotation;
        _rotationVelocity = Vector3.zero; // �����ٶ�
        _currentRotation = _targetRotation; // ͬ����ǰֵ
        _targetPosition = transform.position; // ���ֵ�ǰλ��
        _targetScale = transform.localScale.x; // ���ֵ�ǰ����
        _useImmediateReset = true; // ������������ģʽ
    }

    /// <summary>
    /// �����������ŵ�Ĭ�ϴ�С
    /// </summary>
    public void ResetScaleImmediate()
    {
        _targetScale = defaultScale;
        _scaleVelocity = 0f; // �����ٶ�
        _targetPosition = transform.position; // ���ֵ�ǰλ��
        _currentRotation = transform.eulerAngles; // ���ֵ�ǰ��ת
        _useImmediateReset = true; // ������������ģʽ
    }

    /// <summary>
    /// ƽ����������״̬��λ�á���ת�����ţ�
    /// </summary>
    /// <param name="duration">���ó���ʱ�䣨�룩</param>
    public void ResetAllSmooth(float duration = 0.5f)
    {
        // �����������Э�������У���ֹͣ
        if (_resetCoroutine != null)
        {
            StopCoroutine(_resetCoroutine);
        }
        _resetCoroutine = StartCoroutine(ResetAllCoroutine(duration));
    }

    /// <summary>
    /// ��������״̬��Э��
    /// </summary>
    private IEnumerator ResetAllCoroutine(float duration)
    {
        _isResetting = true; // ���Ϊ����״̬
        float originalMultiplier = resetSpeedMultiplier;
        resetSpeedMultiplier = 10f; // �������ù���

        // ��¼��ʼֵ���ڲ�ֵ
        Vector3 startPos = transform.position;
        Vector3 startRot = transform.eulerAngles;
        float startScale = transform.localScale.x;

        // ����Ŀ��ֵ
        _targetPosition = Vector3.zero;
        _targetRotation = defaultRotation;
        _targetScale = defaultScale;

        // ��ֵ����
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration); // �����ֵ����

            // ʹ��Lerp����ƽ�����ɣ�����SmoothDamp���ܵ��µĿ��٣�
            transform.position = Vector3.Lerp(startPos, _targetPosition, t);
            transform.eulerAngles = new Vector3(
                Mathf.LerpAngle(startRot.x, _targetRotation.x, t),
                Mathf.LerpAngle(startRot.y, _targetRotation.y, t),
                Mathf.LerpAngle(startRot.z, _targetRotation.z, t)
            );
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, _targetScale, t);

            yield return null;
        }

        // ȷ������״̬׼ȷ
        ResetAllImmediate();

        // �ָ�ԭʼ����
        resetSpeedMultiplier = originalMultiplier;
        _isResetting = false;
        _resetCoroutine = null;
    }

    /// <summary>
    /// ������������״̬��λ�á���ת�����ţ�
    /// </summary>
    public void ResetAllImmediate()
    {
        // ����Ŀ��ֵ
        _targetPosition = Vector3.zero;
        _targetRotation = defaultRotation;
        _targetScale = defaultScale;

        // ͬ����ǰֵ
        _currentRotation = _targetRotation;

        // ���������ٶ�
        _positionVelocity = Vector3.zero;
        _rotationVelocity = Vector3.zero;
        _scaleVelocity = 0f;

        // ֱ������transform
        transform.position = _targetPosition;
        transform.eulerAngles = _targetRotation;
        transform.localScale = Vector3.one * _targetScale;

        // ������������ģʽ��ȷ����һ֡����ƽ����
        _useImmediateReset = true;
    }

    /// <summary>
    /// �ڳ�����ͼ�л��Ƹ���Gizmos
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // �����ƶ���Χ������
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

        // ����ԭ��λ�ñ��
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(Vector3.zero, 0.2f);
    }
}