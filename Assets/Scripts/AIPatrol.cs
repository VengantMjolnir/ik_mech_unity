using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class AIPatrol : MonoBehaviour
    {
        private static int ANIM_SPEED = Animator.StringToHash("AnimSpeed");

        [SerializeField]
        private NavMeshAgent _navMeshAgent;
        [SerializeField]
        private MechEntity _mechEntity;
        [SerializeField]
        private List<Transform> _pathNodes = new List<Transform>();
        [SerializeField]
        private float _movementSmoothingSpeed = 1f;
        [SerializeField]
        private float _lookLerpFactor = 1f;
        private Vector3 _currentLookDir = Vector3.zero;

        public Vector3 _rawInputMovement;
        public Vector3 _smoothInputMovement;
        public Vector3 _inputVector;

        private int _currentPathIndex = 0;
        private Animator _animator;

        private void Start()
        {
            TravelToNode(_currentPathIndex);

            _navMeshAgent.updatePosition = false;
            _navMeshAgent.updateRotation = false;
            _currentLookDir = _mechEntity.transform.forward;
            _animator = _mechEntity.GetComponent<Animator>();
        }

        private int GetNextNodeWrap(int index)
        {
            return (index + 1) % _pathNodes.Count;
        }

        private void TravelToNode(int index)
        {
            Debug.Log($"Pathing to node {index}");
            _navMeshAgent.SetDestination(_pathNodes[index].position);
        }

        private void Update()
        {
            
            if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance < _navMeshAgent.stoppingDistance)
            {
                Debug.Log($"Agent has arrived, going to next node");
                _currentPathIndex = GetNextNodeWrap(_currentPathIndex);
                TravelToNode(_currentPathIndex);
            }
            else
            {
                Vector3 worldDeltaPosition = _navMeshAgent.nextPosition - _mechEntity.transform.position;
                _currentLookDir = Vector3.Lerp(_currentLookDir, _navMeshAgent.destination - _mechEntity.transform.position, _lookLerpFactor * Time.deltaTime);
                _currentLookDir.y = 0;
                _currentLookDir.Normalize();

                _rawInputMovement = _navMeshAgent.desiredVelocity / _navMeshAgent.speed;

                float distance = worldDeltaPosition.magnitude;
                //if (distance > _navMeshAgent.radius)
                //{
                _navMeshAgent.nextPosition = _mechEntity.transform.position + 0.5f * worldDeltaPosition;
                //}
                float t = 1f + distance / (_navMeshAgent.stoppingDistance);
                t = Mathf.Clamp(t, 1f, 2f);
                _animator.SetFloat(ANIM_SPEED, t);
                
                CalculateMovementInputSmoothing();
                _inputVector = _smoothInputMovement * (t / 2f);
                
                _mechEntity.UpdateMovementData(_inputVector, _currentLookDir);
            }
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

        private void OnDrawGizmos()
        {
            for (int i = 0; i < _pathNodes.Count; ++i)
            {
                int n = GetNextNodeWrap(i);
                Transform node = _pathNodes[i];
                Transform next = _pathNodes[n];
                Gizmos.DrawLine(node.position, next.position);
            }
        }
    }
}
