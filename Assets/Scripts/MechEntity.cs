using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechEntity : MonoBehaviour
{
    private static int FORWARD = Animator.StringToHash("Forward");
    private static int TURN = Animator.StringToHash("Turn");
    private static int IN_AIR = Animator.StringToHash("InAir");
    private static int ANIM_SPEED = Animator.StringToHash("AnimSpeed");

    [Header("Mech Parts")]
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _root;
    [SerializeField] private Transform _torso;

    [Header("Tweak Params")]
    [SerializeField] private float _legAlignmentCoefficient = 0.8f;
    [SerializeField] private float _aimOffset = 0.125f;
    [SerializeField] private float _inAirSpeed = 25f;
    [SerializeField] private Vector3 _gravity = new Vector3(0, -1.8f, 0);
    [SerializeField] private float _maxSpeedKPH = 155;
    [SerializeField] private AnimationCurve _footstepVolumeCurve;
    [SerializeField] private AnimationCurve _jumpCurve;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _maxJumpHeight = 30f;

    [Header("Sounds")]
    [SerializeField] private List<AudioClip> _footSteps;

    private Vector3 _desiredDirection;
    private Vector3 _forward;
    private float _verticalSpeed = 0f;
    private float _inAir;
    private float _alignmentCoefficient;
    private Vector3 _animatorDeltaPositions = Vector3.zero;
    private float _currentSpeedKPH = 0f;

    private bool _jump;
    private bool _fire;

    public float MaxSpeedKPH { get { return _maxSpeedKPH; } }
    public float CurrentSpeedKPH { get { return _currentSpeedKPH; } }
    public Vector3 Velocity { get { return _controller.velocity; } }

    public void UpdateMovementData(Vector3 direction, Vector3 forward)
    {
        _desiredDirection = direction;
        _forward = forward;
    }

    public void Jump(bool activate)
    {
        if (!_jump && _controller.isGrounded)
        {
            _verticalSpeed = -_gravity.y * Time.deltaTime;
        }
        _jump = activate;
    }

    public void PlayFootstep(float volume = 1f)
    {
        float speedRatio = _currentSpeedKPH / _maxSpeedKPH;
        volume = _footstepVolumeCurve.Evaluate(speedRatio);
        if (_footSteps != null && _footSteps.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, _footSteps.Count);
            AudioSource.PlayClipAtPoint(_footSteps[index], transform.position, volume);
        }
    }

    void LateUpdate()
    {
        UpdateFalling();
        UpdateJumping();
        UpdateLegs();
        UpdateTorso();

        Vector3 planarVelocity = _controller.velocity;
        planarVelocity.y = 0f;
        _currentSpeedKPH = Mathf.Round(planarVelocity.magnitude * 3.6f);
    }

    private void UpdateLegs()
    {
        float forward = 0f;
        float turn = 0f;

        if (_desiredDirection.sqrMagnitude > 0f)
        {
            forward = Vector3.Dot(_root.forward, _desiredDirection);
            turn = Vector3.Dot(_root.right, _desiredDirection);
            if (forward < _alignmentCoefficient)
            {
                _root.forward = Vector3.Slerp(_root.forward, _desiredDirection, 0.125f);
                forward = Vector3.Dot(_root.forward, _desiredDirection);
                turn = Vector3.Dot(_root.right, _desiredDirection);
            }
        }
        else
        {
            _animator.SetFloat(FORWARD, 0);
            _animator.SetFloat(TURN, 0);
        }

        Vector3 vertical = Vector3.up * _verticalSpeed;
        Vector3 horizontal;

        if (_inAir > 0.1f)
        {
            horizontal = _root.forward * Time.deltaTime * 0.75f * forward;
            horizontal *= _inAirSpeed;
            horizontal.y = 0f;
        }
        else
        {
            horizontal = _animatorDeltaPositions;
            _animatorDeltaPositions = Vector3.zero;
            _animator.SetFloat(FORWARD, forward);
            _animator.SetFloat(TURN, turn);
        }

        Vector3 position = _controller.transform.position;
        CollisionFlags flags = _controller.Move(vertical + horizontal);
        if (flags.HasFlag(CollisionFlags.CollidedSides))
        {
            Vector3 delta = _controller.transform.position - position;
            float ratio = Mathf.Clamp(delta.magnitude / horizontal.magnitude, 0.1f, 1.0f);
            Debug.Log($"Blocked: {ratio}");
            forward *= ratio;
            _animator.SetFloat(FORWARD, forward);
        }
    }
    private void UpdateTorso()
    {
        Vector3 lookPoint = _forward;
        lookPoint.y *= 0.6f;
        lookPoint.y += _aimOffset;
        lookPoint.Normalize();
        lookPoint += _torso.position;

        _torso.LookAt(lookPoint);
    }

    private void UpdateFalling()
    {
        if (_controller.isGrounded)
        {
            _inAir = 0;
            _verticalSpeed += _gravity.y * Time.deltaTime;
            _alignmentCoefficient = _legAlignmentCoefficient;
        }
        else
        {
            _verticalSpeed += _gravity.y * Time.deltaTime * 0.5f;
            _inAir = Mathf.Clamp01(_inAir + Time.deltaTime);
            _alignmentCoefficient = 2f;
        }

        _animator.SetFloat(IN_AIR, _inAir);
    }

    private void UpdateJumping()
    {
        if (_jump)
        {
            float jumpRatio = Mathf.Clamp01(transform.position.y / _maxJumpHeight);
            _verticalSpeed += -_gravity.y * _jumpCurve.Evaluate(jumpRatio) * Time.deltaTime * _jumpForce;
        }
    }

    private void OnAnimatorMove()
    {
        _animatorDeltaPositions = _animator.deltaPosition;
        _root.forward = _animator.deltaRotation * _root.forward;
    }
}
