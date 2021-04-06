using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MechController : MonoBehaviour
{
    [SerializeField] private CharacterController _controller;

    private Vector3 _desiredDirection;
    private Vector3 _forward;

    public void UpdateDesiredDirection(Vector3 direction, Vector3 forward)
    {
        _desiredDirection = direction;
        _forward = forward;
    }

    private void FixedUpdate()
    {
        
    }
}

