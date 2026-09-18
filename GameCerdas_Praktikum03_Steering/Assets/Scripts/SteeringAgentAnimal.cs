using UnityEngine;

public class SteeringAgentAnimal : MonoBehaviour
{
    [Header("Target & Animal Behavior")]
    [SerializeField] private Transform target;
    [SerializeField] private float panicRadius = 5f;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float maxAcceleration = 8f;
    [SerializeField] private float turnSpeed = 8f;

    [Header("Wander")]
    [SerializeField] private float wanderSpeed = 2.5f;
    [SerializeField] private float wanderChangeInterval = 1.5f;
    [SerializeField] private float wanderAngleChange = 45f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private SteeringSensor sensor;
    [SerializeField] private float avoidanceWeight = 2.5f;

    [Header("Separation")]
    [SerializeField] private LayerMask agentMask;
    [SerializeField] private float separationRadius = 1.5f;
    [SerializeField] private float separationWeight = 1.5f;

    private Vector3 velocity;
    private Vector3 wanderDirection;
    private float wanderTimer;

    public Vector3 Velocity => velocity;

    private void Start()
    {
        wanderDirection = transform.forward;
        wanderTimer = wanderChangeInterval;
    }

    private void Update()
    {
        Vector3 desiredVelocity;

        if (target != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);

            // Struktur Decision Tree Animal NPC
            if (distanceToPlayer < panicRadius)
            {
                desiredVelocity = CalculateFlee();
            }
            else
            {
                desiredVelocity = CalculateWander();
            }
        }
        else
        {
            desiredVelocity = CalculateWander();
        }

        // Semua kondisi melewati Obstacle Avoidance
        desiredVelocity = ApplyObstacleAvoidance(desiredVelocity);

        // Movement
        velocity = Vector3.MoveTowards(
            velocity,
            desiredVelocity,
            maxAcceleration * Time.deltaTime
        );

        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        ApplyMovement();
        UpdateRotation();
    }

    private Vector3 CalculateFlee()
    {
        Vector3 awayFromTarget = transform.position - target.position;
        awayFromTarget.y = 0f;

        // Flee dengan kecepatan maksimum menjauhi target
        return awayFromTarget.normalized * maxSpeed;
    }

    private Vector3 CalculateWander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            float randomAngle = Random.Range(-wanderAngleChange, wanderAngleChange);
            wanderDirection = Quaternion.Euler(0f, randomAngle, 0f) * transform.forward;
            wanderDirection.y = 0f;
            wanderDirection.Normalize();
            wanderTimer = wanderChangeInterval;
        }
        return wanderDirection * wanderSpeed;
    }

    private Vector3 ApplyObstacleAvoidance(Vector3 desiredVelocity)
    {
        if (sensor == null) return desiredVelocity;

        Vector3 checkDirection = desiredVelocity.sqrMagnitude > 0.001f ? desiredVelocity.normalized : transform.forward;
        Vector3 avoidanceDirection = sensor.GetAvoidanceDirection(checkDirection);

        if (avoidanceDirection.sqrMagnitude > 0.001f)
        {
            Vector3 combinedDirection = checkDirection + avoidanceDirection * avoidanceWeight;
            combinedDirection.y = 0f;

            if (combinedDirection.sqrMagnitude > 0.001f) combinedDirection.Normalize();
            float desiredSpeed = Mathf.Max(desiredVelocity.magnitude, wanderSpeed);
            
            return combinedDirection * desiredSpeed;
        }
        return desiredVelocity;
    }

    private void ApplyMovement()
    {
        transform.position += velocity * Time.deltaTime;
    }

    private void UpdateRotation()
    {
        Vector3 horizontalVelocity = velocity;
        horizontalVelocity.y = 0f;

        if (horizontalVelocity.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, panicRadius);
        
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}