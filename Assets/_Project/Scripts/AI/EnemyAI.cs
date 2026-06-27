// =============================================================================
// EnemyAI.cs
// =============================================================================
// PURPOSE:
//   Smart combat AI using NavMesh + Finite State Machine.
//   States: Idle → Patrol → Investigate → Chase → Attack → Flee → Dead
//
// ENGINE: Unity 6 AI Navigation package
// =============================================================================

using UnityEngine;
using UnityEngine.AI;
using ShadowEmpire.Core;

namespace ShadowEmpire.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        public enum AIState { Idle, Patrol, Investigate, Chase, Attack, Flee, Dead }

        [Header("Stats")]
        public float health = 100f;
        public float damage = 15f;
        public float sightRange = 25f;
        public float attackRange = 6f;
        public float fireRate = 1.5f;
        public LayerMask targetLayer;

        [Header("Patrol")]
        public Transform[] patrolPoints;
        public float patrolWaitTime = 2f;

        private NavMeshAgent _agent;
        private AIState _state = AIState.Idle;
        private Transform _target;
        private float _nextFireTime;
        private int _patrolIndex;

        // -------- Events --------
        public event System.Action OnDied;

        // -------- Lifecycle --------
        private void Awake() => _agent = GetComponent<NavMeshAgent>();

        private void Update()
        {
            if (_state == AIState.Dead) return;

            ScanForTarget();
            switch (_state)
            {
                case AIState.Idle:      TickIdle(); break;
                case AIState.Patrol:    TickPatrol(); break;
                case AIState.Investigate: TickInvestigate(); break;
                case AIState.Chase:     TickChase(); break;
                case AIState.Attack:    TickAttack(); break;
                case AIState.Flee:      TickFlee(); break;
            }
        }

        // -------- State logic --------
        private void ScanForTarget()
        {
            var hits = Physics.OverlapSphere(transform.position, sightRange, targetLayer);
            if (hits.Length > 0)
            {
                _target = hits[0].transform;
                if (Vector3.Distance(transform.position, _target.position) <= attackRange)
                    SetState(AIState.Attack);
                else
                    SetState(AIState.Chase);
            }
        }

        private void TickIdle()
        {
            _agent.isStopped = true;
            if (patrolPoints != null && patrolPoints.Length > 0)
                SetState(AIState.Patrol);
        }

        private void TickPatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;
            var dest = patrolPoints[_patrolIndex].position;
            _agent.isStopped = false;
            _agent.speed = 2f;
            _agent.SetDestination(dest);
            if (_agent.remainingDistance < 0.5f)
            {
                _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
                SetState(AIState.Idle);
                Invoke(nameof(ResumePatrol), patrolWaitTime);
            }
        }
        private void ResumePatrol() => SetState(AIState.Patrol);

        private void TickInvestigate() { /* react to noise */ }

        private void TickChase()
        {
            if (_target == null) { SetState(AIState.Patrol); return; }
            _agent.isStopped = false;
            _agent.speed = 5.5f;
            _agent.SetDestination(_target.position);
            if (Vector3.Distance(transform.position, _target.position) <= attackRange)
                SetState(AIState.Attack);
        }

        private void TickAttack()
        {
            _agent.isStopped = true;
            transform.LookAt(_target);
            if (Time.time >= _nextFireTime)
            {
                Fire();
                _nextFireTime = Time.time + fireRate;
            }
            if (Vector3.Distance(transform.position, _target.position) > attackRange * 1.2f)
                SetState(AIState.Chase);
        }

        private void TickFlee()
        {
            Vector3 dir = (transform.position - _target.position).normalized;
            _agent.SetDestination(transform.position + dir * 20f);
            if (health > 50f) SetState(AIState.Chase);
        }

        // -------- Combat --------
        public void TakeDamage(float amount)
        {
            health -= amount;
            if (health <= 0) Die();
            else if (health < 25f) SetState(AIState.Flee);
        }

        private void Fire()
        {
            // Raycast hit
            if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out var hit, attackRange))
            {
                var dmg = hit.collider.GetComponentInParent<IDamageable>();
                dmg?.TakeDamage(damage);
            }
            // Muzzle flash + sfx hook
            EventBus.Publish(new EnemyFiredEvent(transform.position));
        }

        private void Die()
        {
            SetState(AIState.Dead);
            _agent.isStopped = true;
            OnDied?.Invoke();
            EventBus.Publish(new EnemyDiedEvent(transform.position));
            Destroy(gameObject, 5f);
        }

        private void SetState(AIState s) { _state = s; }
    }

    public interface IDamageable { void TakeDamage(float amount); }
    public struct EnemyFiredEvent : IEvent { public Vector3 Position; public EnemyFiredEvent(Vector3 p) { Position = p; } }
    public struct EnemyDiedEvent : IEvent { public Vector3 Position; public EnemyDiedEvent(Vector3 p) { Position = p; } }
}