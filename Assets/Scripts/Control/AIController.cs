using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RPG.Movement;
using RPG.Combat;
using RPG.Core;

namespace RPG.Control {
  public class AIController : MonoBehaviour {
    [SerializeField] float chaseDistance = 5f;
    [SerializeField] float suspicionTime = 5f;
    [SerializeField] PatrolPath patrolPath;
    [SerializeField] float waypointTolerance = 1f;
    [SerializeField] float waypointDwellTime = 3f;
    [Range(0, 1)] [SerializeField] float patrolSpeedFraction = 0.2f;

    Fighter fighter;
    Mover mover;
    Health health;
    GameObject player;
    ActionScheduler actionScheduler;

    Vector3 guardPosition;
    float timeSinceLastSawPlayer = Mathf.Infinity;
    float timeSinceLastSawWaypoint = Mathf.Infinity;
    int currentWaypointIndex = 0;

    private void Start() {
      fighter = GetComponent<Fighter>();
      mover = GetComponent<Mover>();
      health = GetComponent<Health>();
      actionScheduler = GetComponent<ActionScheduler>();
      player = GameObject.FindWithTag("Player");

      guardPosition = transform.position;
    }

    private void Update()
      {
        if (health.IsDead()) return;
        if (InAttackRange() && fighter.CanAttack(player)) {
          AttackBehaviour();
        } else if (timeSinceLastSawPlayer < suspicionTime) {
          SuspicionBehaviour();
        }
        else {
          PatrolBehaviour();
        }

        UpdateTimers();
    }

    private void UpdateTimers()
    {
        timeSinceLastSawPlayer += Time.deltaTime;
        timeSinceLastSawWaypoint += Time.deltaTime;
    }

    private void OnDrawGizmosSelected() {
      Gizmos.color = Color.blue;
      Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }

    private void PatrolBehaviour() {
      Vector3 nextPosition = guardPosition;
      if (patrolPath != null) {
        if (AtWaypoint()) {
          timeSinceLastSawWaypoint = 0;
          CycleWaypoint();
        }

        nextPosition = GetCurrentWaypoint();
      }

      if (timeSinceLastSawWaypoint > waypointDwellTime) {
        mover.StartMoveAction(nextPosition, patrolSpeedFraction);
      }
    }

    private bool AtWaypoint() {
      return Vector3.Distance(transform.position, GetCurrentWaypoint()) < waypointTolerance;
    }

    private void CycleWaypoint() {
      currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
    }

    private Vector3 GetCurrentWaypoint() {
      return patrolPath.GetWayPoint(currentWaypointIndex);
    }

    private void SuspicionBehaviour() {
      actionScheduler.CancelCurrentAction();
    }

    private void AttackBehaviour() {
      timeSinceLastSawPlayer = 0;
      fighter.Attack(player);
    }

    private bool InAttackRange() {
      return Vector3.Distance(player.transform.position, transform.position) < chaseDistance;
    }
  }
}
