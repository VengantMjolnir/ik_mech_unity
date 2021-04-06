using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private int _playerID;

    [Header("Sub Behaviours")]
    [SerializeField] private MechEntity _mechEntity;
    [SerializeField] private ThirdPersonCameraController _cameraController;

    [Header("Input Settings")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private float _movementSmoothingSpeed = 1f;

    [SerializeField] private TMP_Text _speedField;
    

    private Vector3 _rawInputMovement;
    private Vector3 _smoothInputMovement;

    private string _actionMapPlayerControls = "Player Controls";
    private string _actionMapMenuControls = "Menu Controls";

    private string _currentControlScheme;

    public void SetupPlayer(int playerID)
    {
        _playerID = playerID;

        _currentControlScheme = _playerInput.currentControlScheme;
    }

    #region  INPUT SYSTEM ACTION METHODS
    public void OnMovement(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();
        _rawInputMovement = new Vector3(inputMovement.x, 0, inputMovement.y);
    }

    public void OnTogglePause(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            //GameManager.Instance.TogglePauseState(this);
        }
    }

    public void OnJump(InputAction.CallbackContext value)
    {
        _mechEntity.Jump(value.ReadValueAsButton());
    }
    #endregion

    #region INPUT SYSTEM AUTOMATIC CALLBACKS
    public void OnControlsChanged()
    {
        if (_playerInput.currentControlScheme != _currentControlScheme)
        {
            _currentControlScheme = _playerInput.currentControlScheme;

            //playerVisualsBehaviour.UpdatePlayerVisuals();
            RemoveAllBindingOverrides();
        }
    }

    public void OnDeviceLost()
    {
        //playerVisualsBehaviour.SetDisconnectedDeviceVisuals();
    }

    public void OnDeviceRegained()
    {
        StartCoroutine(WaitForDeviceToBeRegained());
    }

    IEnumerator WaitForDeviceToBeRegained()
    {
        yield return new WaitForSeconds(0.1f);
        //playerVisualsBehaviour.UpdatePlayerVisuals();
    }
    #endregion

    private void Update()
    {
        CalculateMovementInputSmoothing();
        UpdatePlayerMovement();
        UpdatePlayerAnimationMovement();
    }

    private void CalculateMovementInputSmoothing()
    {
        if (Mathf.Abs(_rawInputMovement.sqrMagnitude - _smoothInputMovement.sqrMagnitude) < 0.0125f)
        {
            _smoothInputMovement = _rawInputMovement;
        }
        {
            _smoothInputMovement = Vector3.Lerp(_smoothInputMovement, _rawInputMovement, Time.deltaTime * _movementSmoothingSpeed);
        }
    }

    private void UpdatePlayerMovement()
    {
        Transform cam = _cameraController.transform;
        Vector3 f = cam.forward;
        Vector3 r = cam.right;
        f.y = 0;
        f.Normalize();
        r.y = 0;
        r.Normalize();
        Vector3 direction = (f * _smoothInputMovement.z) + (r * _smoothInputMovement.x);
        _mechEntity.UpdateMovementData(direction, cam.forward);
        
        if(_speedField != null)
        {
            _speedField.text = $"{Mathf.RoundToInt(_mechEntity.CurrentSpeedKPH)} km/h";
        }
    }

    private void UpdatePlayerAnimationMovement()
    {
        //_playerAnimationBehaviour.UpdateMovementAnimation(_smoothInputMovement.magnitude);
    }

    void RemoveAllBindingOverrides()
    {
        InputActionRebindingExtensions.RemoveAllBindingOverrides(_playerInput.currentActionMap);
    }

}
