using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    public Transform target;
    public LayerMask raycastMask;
    public BoxCollider targetCollider;

    [Header("Camera Values")]
    public float startingDistance = 15;
    public float startingYaw = 0;
    public float startingPitch = 0;
    public float positionLerpFactor = 0.25f;
    public float maximumPitch = 30f;
    public float minimumPitch = -30f;
    public float minimumDistance = 1f;
    public float maxFov = 33f;
    public float minFov = 45f;
    public AnimationCurve pitchSpeedCurve;
    public AnimationCurve distanceCurve;
    public AnimationCurve fovCurve;
    [Header("Input")]
    public float verticalSpeed = 50f;
    public float horizontalSpeed = 100f;

    [Header("Debug Controls")]
    public bool log = false;

    private Transform _transform;
    private Camera _camera;
    private float _fov;
    private float _distance;
    private float _yaw;
    private float _pitch;

    private bool _hideCursor = true;
    private Vector2 _desiredInput = Vector2.zero;

    public Camera Camera
    {
        get { return _camera; }
    }

    private static ThirdPersonCameraController _instance;
    public static ThirdPersonCameraController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ThirdPersonCameraController>();
            }
            return _instance;
        }
    }

    private void Log(string message)
    {
        if (log)
        {
            Debug.Log(message);
        }
    }

    void Start()
    {
        _transform = GetComponent<Transform>();
        _camera = GetComponent<Camera>();
        _fov = _camera.fieldOfView;

        StartCoroutine(DelayedLockMouse());
    }

    IEnumerator DelayedLockMouse()
    {
        yield return new WaitForSeconds(1f); 
        UpdateCursor();
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void UpdateCursor()
    {
        Cursor.lockState = _hideCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !_hideCursor;
    }

    #region  INPUT SYSTEM ACTION METHODS
    public void OnCameraMovement(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();
        HandleInput(inputMovement.x, inputMovement.y);
        SetInput(0f, 0f);
    }

    public void OnCameraLook(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();
        SetInput(inputMovement.x, inputMovement.y);
    }

    public void OnToggleCursor(InputAction.CallbackContext value)
    {
        _hideCursor = !_hideCursor;
        Debug.Log($"Cursor is hidden: {_hideCursor}");
        UpdateCursor();
    }
    #endregion

    public void HandleInput(float horizontal, float vertical)
    {
        float dt = Time.deltaTime;
        float h = horizontal;
        float v = vertical;

        if (System.Math.Abs(h) > float.Epsilon || System.Math.Abs(v) > float.Epsilon)
        {
            float t = _pitch / maximumPitch;
            float pitchSpeedModifier = pitchSpeedCurve.Evaluate(t);
            _yaw += h * dt * horizontalSpeed;
            _pitch += v * dt * verticalSpeed * pitchSpeedModifier;

            _pitch = Mathf.Clamp(_pitch, minimumPitch, maximumPitch);
        }
    }

    public void SetInput(float horizontal, float vertical)
    {
        _desiredInput = new Vector2(horizontal, vertical);
    }

    public void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (_desiredInput != Vector2.zero)
        {
            HandleInput(_desiredInput.x, _desiredInput.y);
        }

        float targetYaw = startingYaw + _yaw;
        float targetPitch = startingPitch + _pitch;
        float t = _pitch / maximumPitch;
        _distance = Mathf.Lerp(startingDistance, minimumDistance, distanceCurve.Evaluate(t));
        _fov = Mathf.Lerp(minFov, maxFov, fovCurve.Evaluate(t));

        Vector3 pos = target.position;
        Quaternion rotation = Quaternion.Euler(targetPitch, targetYaw, 0);
        Vector3 forward = Vector3.forward * _distance;
        Vector3 dir = rotation * forward;

        _transform.position = Vector3.Lerp(_transform.position, pos + dir, positionLerpFactor);

        _transform.LookAt(target);
        _camera.fieldOfView = _fov;
    }
}
