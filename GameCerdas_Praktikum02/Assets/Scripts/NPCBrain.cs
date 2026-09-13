using UnityEngine;
using UnityEngine.AI;

public class NPCBrain : MonoBehaviour
{
    public enum NPCState
    {
        Patrol,
        Chase,
        Search
    }

    [Header("References")]
    [SerializeField]
    private NPCSensor sensor;

    [SerializeField]
    private NavMeshAgent agent;

    [SerializeField] private GameObject chaseIndicator; // Masukkan Sprite/UI "!"

    [SerializeField] private GameObject searchIndicator; // Masukkan Sprite/UI "?"

    [Header("Patrol Settings")]
    [SerializeField]
    private Transform[] patrolPoints;

    [SerializeField]
    private float waypointTolerance = 0.7f;

    [SerializeField]
    private float patrolSpeed = 2f;

    [SerializeField] private float patrolWaitTime = 2f;
    private float patrolWaitTimer;

    [Header("Chase Settings")]
    [SerializeField]
    private float chaseSpeed = 4f;

    [Header("Search Settings")]
    [SerializeField]
    private float searchDuration = 4f;

    [SerializeField]
    private float searchTolerance = 0.8f;

    [Header("Debug")]
    [SerializeField]
    private NPCState currentState;

    private NPCState previousState;

    private int patrolIndex = 0;

    // =============================
    // MEMORY
    // =============================

    private Vector3 lastKnownPosition;

    private bool hasLastKnownPosition;

    private float searchTimer;

    private void Start()
    {
        currentState = NPCState.Patrol;
        previousState = currentState;

        GoToCurrentPatrolPoint();
    }

    private void Update()
    {
        UpdateMemory();

        MakeDecision();

        ExecuteCurrentState();
    }

    // ======================================
    // MEMORY
    // ======================================

    private void UpdateMemory()
    {
        if (sensor.CanSeePlayer)
        {
            lastKnownPosition =
                sensor.Player.position;

            hasLastKnownPosition = true;
        }
    }

    // ======================================
    // HEARING
    // ======================================

    public void HearSound(Vector3 soundPosition)
    {
        // Abaikan suara jika sedang mengejar player
        if (currentState == NPCState.Chase) return;

        lastKnownPosition = soundPosition;
        hasLastKnownPosition = true;
        searchTimer = searchDuration;
        
        ChangeState(NPCState.Search);
    }

    // ======================================
    // DECISION
    // ======================================

    private void MakeDecision()
    {
        // PRIORITAS 1
        // PLAYER TERLIHAT
        if (sensor.CanSeePlayer)
        {
            ChangeState(
                NPCState.Chase
            );

            return;
        }

        // PRIORITAS 2
        // PLAYER BARU HILANG
        if (currentState ==
                NPCState.Chase &&
            hasLastKnownPosition)
        {
            searchTimer =
                searchDuration;

            ChangeState(
                NPCState.Search
            );

            return;
        }

        // PRIORITAS 3
        // SEARCH SELESAI
        if (currentState ==
                NPCState.Search &&
            searchTimer <= 0f)
        {
            hasLastKnownPosition =
                false;

            ChangeState(
                NPCState.Patrol
            );
        }
    }

    // ======================================
    // ACTION
    // ======================================

    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case NPCState.Patrol:

                Patrol();
                break;

            case NPCState.Chase:

                Chase();
                break;

            case NPCState.Search:

                Search();
                break;
        }
    }

    // ======================================
    // PATROL
    // ======================================

    private void Patrol()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // Jika sedang menunggu di waypoint
        if (patrolWaitTimer > 0)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0)
            {
                // Lanjut ke waypoint berikutnya
                patrolIndex++;
                if (patrolIndex >= patrolPoints.Length) patrolIndex = 0;
                GoToCurrentPatrolPoint();
            }
            return; 
        }

        // Jika sudah sampai di waypoint, mulai timer
        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            patrolWaitTimer = patrolWaitTime;
        }
    }

    private void GoToCurrentPatrolPoint()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return;
        }

        agent.SetDestination(
            patrolPoints[
                patrolIndex
            ].position
        );
    }

    // ======================================
    // CHASE
    // ======================================

    private void Chase()
    {
        agent.speed =
            chaseSpeed;

        if (sensor.Player == null)
            return;

        agent.SetDestination(
            sensor.Player.position
        );
    }

    // ======================================
    // SEARCH
    // ======================================

    private void Search()
    {
        agent.speed = patrolSpeed;
        agent.SetDestination(lastKnownPosition);

        if (!agent.pathPending && agent.remainingDistance <= searchTolerance)
        {
            searchTimer -= Time.deltaTime;
            agent.ResetPath(); // Berhenti berjalan

            // Tambahkan efek menengok kiri-kanan
            float lookSpeed = 150f;
            float lookAngle = Mathf.Sin(Time.time * 4f) * lookSpeed;
            transform.Rotate(Vector3.up, lookAngle * Time.deltaTime);
        }
    }

    // ======================================
    // STATE TRANSITION
    // ======================================

    private void ChangeState(NPCState newState)
    {
        if (currentState == newState) return;

        previousState = currentState;
        currentState = newState;

        if (currentState == NPCState.Patrol)
        {
            GoToCurrentPatrolPoint();
        }

        // Tambahkan pengaturan Indikator
        if (chaseIndicator != null) chaseIndicator.SetActive(currentState == NPCState.Chase);
        if (searchIndicator != null) searchIndicator.SetActive(currentState == NPCState.Search);
    }

    // ======================================
    // DEBUG GIZMOS
    // ======================================

    private void OnDrawGizmosSelected()
    {
        switch (currentState)
        {
            case NPCState.Patrol:

                Gizmos.color =
                    Color.green;
                break;

            case NPCState.Chase:

                Gizmos.color =
                    Color.red;
                break;

            case NPCState.Search:

                Gizmos.color =
                    Color.blue;
                break;
        }

        Gizmos.DrawWireSphere(
            transform.position,
            0.8f
        );

        if (hasLastKnownPosition)
        {
            Gizmos.color =
                Color.magenta;

            Gizmos.DrawSphere(
                lastKnownPosition,
                0.3f
            );

            Gizmos.DrawLine(
                transform.position,
                lastKnownPosition
            );
        }
    }
}