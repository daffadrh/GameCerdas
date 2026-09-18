using UnityEngine;

public class SteeringAgent : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private bool useTarget = true;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float maxAcceleration = 8f;
    [SerializeField] private float turnSpeed = 8f;

    [Header("Pursue & Arrive")]
    [SerializeField] private float slowRadius = 4f;
    [SerializeField] private float stopRadius = 1.5f;
    [SerializeField] private float predictionTime = 1.5f;

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
    private Vector3 lastTargetPosition;

    public Vector3 Velocity => velocity;

    private void Start()
    {
        wanderDirection = transform.forward;
        wanderTimer = wanderChangeInterval;
        if (target != null) lastTargetPosition = target.position;
    }

    private void Update()
    {
        Vector3 desiredVelocity;

        if (useTarget && target != null)
        {
            // Menggunakan Pursue menggantikan Arrive biasa
            desiredVelocity = CalculatePursue();
        }
        else
        {
            desiredVelocity = CalculateWander();
        }

        desiredVelocity = ApplyObstacleAvoidance(desiredVelocity);

        velocity = Vector3.MoveTowards(
            velocity,
            desiredVelocity,
            maxAcceleration * Time.deltaTime
        );

        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        ApplyMovement();
        UpdateRotation();

        // Catat posisi terakhir target di akhir frame untuk prediksi kecepatan di frame berikutnya
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }

    private Vector3 CalculatePursue()
    {
        Vector3 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;

        if (distance <= stopRadius) return Vector3.zero;

        // Hitung prediksi kecepatan target
        Vector3 targetVelocity = (target.position - lastTargetPosition) / Time.deltaTime;

        // Prediksi posisi target di masa depan
        float lookAheadTime = Mathf.Clamp(distance / maxSpeed, 0f, predictionTime);
        Vector3 predictedPosition = target.position + targetVelocity * lookAheadTime;

        Vector3 desiredDirection = predictedPosition - transform.position;
        desiredDirection.y = 0f;

        float desiredSpeed = maxSpeed;

        // Logika slow down (Arrive) diaplikasikan pada Pursue
        if (distance < slowRadius)
        {
            float range = Mathf.Max(slowRadius - stopRadius, 0.001f);
            float normalizedDistance = (distance - stopRadius) / range;
            desiredSpeed = maxSpeed * Mathf.Clamp01(normalizedDistance);
        }

        return desiredDirection.normalized * desiredSpeed;
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
        Gizmos.DrawWireSphere(transform.position, stopRadius);
        Gizmos.DrawWireSphere(transform.position, slowRadius);
        if (target != null) Gizmos.DrawLine(transform.position, target.position);
    }
}