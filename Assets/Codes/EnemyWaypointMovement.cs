using UnityEngine;
using System.Collections.Generic;

public class EnemyWaypointMovement : MonoBehaviour
{
    [Header("Waypoints")]
    public List<Transform> waypoints;
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float waypointReachedDistance = 0.1f;
    public bool loop = true;
    public Transform visual;

    [Header("Combat Settings")]
    public float damage = 10f;
    public float attackCooldown = 1f;
    public float knockbackForce = 15f;

    private Rigidbody2D rb;
    private int currentWaypointIndex = 0;
    private Vector2 movementDirection;
    private float lastAttackTime;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (waypoints == null || waypoints.Count == 0){
            Debug.LogError("No waypoints assigned to the enemy!");
            enabled = false;
            return;
        }

        SetTargetWaypoint(currentWaypointIndex);
    }

    void FixedUpdate()
    {
        MoveTowardsWaypoint();
        CheckIfWaypointReached();        
    }

    void SetTargetWaypoint(int index){
        if (waypoints.Count == 0) return;

        currentWaypointIndex = index;
        Vector2 targetPosition = waypoints[currentWaypointIndex].position;
        movementDirection = (targetPosition - (Vector2)transform.position).normalized;
    }

    void MoveTowardsWaypoint(){
        if (waypoints.Count == 0) return;

        Vector2 targetPosition = waypoints[currentWaypointIndex].position;
        movementDirection = (targetPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = movementDirection * moveSpeed;

        if(movementDirection.x > 0.1)
        {
            visual.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            visual.localScale = new Vector3(-1, 1, 1);
        }
    }

    void CheckIfWaypointReached(){
        if (waypoints.Count == 0) return;

        float distanceToWaypoint = Vector2.Distance(transform.position, waypoints[currentWaypointIndex].position);

        if(distanceToWaypoint <= waypointReachedDistance)
        {
            GoToNexWaypoint();
        }
    }

    void GoToNexWaypoint(){
        currentWaypointIndex++;

        if(currentWaypointIndex >= waypoints.Count){
            if (loop){
                currentWaypointIndex = 0;
            }
            else {
                enabled = false;
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        SetTargetWaypoint(currentWaypointIndex);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryAttackPlayer(collision.gameObject);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryAttackPlayer(collision.gameObject);
        }
    }

    void TryAttackPlayer(GameObject player)
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(damage, knockbackDirection, knockbackForce);
                lastAttackTime = Time.time;
            }
        }
    }
}
