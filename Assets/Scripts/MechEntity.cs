using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechEntity : MonoBehaviour
{
    private static int FORWARD = Animator.StringToHash("Forward");
    private static int TURN = Animator.StringToHash("Turn");
    private static int IN_AIR = Animator.StringToHash("InAir");

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

    private Vector3 _desiredDirection;
    private Vector3 _forward;
    private float _verticalSpeed = 0f;
    private float _inAir;
    private float _alignmentCoefficient;
    private Vector3 _animatorDeltaPositions = Vector3.zero;

    private bool _jump;
    private bool _fire;

    public Vector3 Velocity { get { return _controller.velocity; } }

    public void UpdateMovementData(Vector3 direction, Vector3 forward)
    {
        _desiredDirection = direction;
        _forward = forward;
    }

    void LateUpdate()
    {
        UpdateLegs();
        UpdateTorso();
        UpdateFalling();
        UpdateJumping();
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
                _root.forward = Vector3.Lerp(_root.forward, _desiredDirection, 0.125f);
                forward = Vector3.Dot(_root.forward, _desiredDirection);
                turn = Vector3.Dot(_root.right, _desiredDirection);
            }
        }
        else
        {
            _animator.SetFloat(FORWARD, 0);
            _animator.SetFloat(TURN, 0);
        }

        _verticalSpeed += _gravity.y * Time.deltaTime;

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
            _verticalSpeed = _gravity.y / 2f;
            _animator.SetFloat(FORWARD, forward);
            _animator.SetFloat(TURN, turn);
        }

        _controller.Move(vertical + horizontal);
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
            _alignmentCoefficient = _legAlignmentCoefficient;
        }
        else
        {
            _inAir = Mathf.Clamp01(_inAir + Time.deltaTime);
            _alignmentCoefficient = 2f;
        }

        _animator.SetFloat(IN_AIR, _inAir);
    }

    private void UpdateJumping()
    {

    }

    private void OnAnimatorMove()
    {
        _animatorDeltaPositions = _animator.deltaPosition;
        _root.forward = _animator.deltaRotation * _root.forward;
    }
}
