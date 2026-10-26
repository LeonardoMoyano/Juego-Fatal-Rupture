using UnityEngine;

/// <summary>
/// IA del enemigo:
///   - MERODEANDO  : camina a puntos aleatorios. Detecta al jugador por distancia + tag.
///                   Las paredes bloquean la detección inicial via Raycast.
///   - PERSIGUIENDO: ya detectó al jugador. Lo persigue SIN parar hasta matarlo o morir.
///                   No pierde al jugador por paredes ni distancia.
///   - BUSCANDO    : el jugador murió o desapareció. Vuelve a merodear.
/// </summary>
public class EnemyFollow : MonoBehaviour
{
    // ─── Referencias ──────────────────────────────────────────────────────────
    [Header("Referencias")]
    [Tooltip("Se asigna automáticamente si se deja vacío (busca por tag 'Player').")]
    public Transform player;

    // ─── Movimiento ───────────────────────────────────────────────────────────
    [Header("Movimiento")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed  = 3f;
    [Tooltip("Radio de patrulla aleatorio desde la posición inicial")]
    public float patrolRadius = 5f;
    [Tooltip("Distancia mínima al waypoint para considerarlo alcanzado")]
    public float waypointReachedDistance = 0.4f;
    [Tooltip("Segundos de espera en cada waypoint")]
    public float patrolWaitTime = 1.5f;

    // ─── Visión / Detección ───────────────────────────────────────────────────
    [Header("Deteccion")]
    [Tooltip("Radio para detectar al jugador por primera vez")]
    public float visionRadius = 15f;
    [Tooltip("Layers que bloquean la visión inicial (paredes). Si no se asigna, ignora obstáculos.")]
    public LayerMask wallLayer;

    // ─── Estado interno ───────────────────────────────────────────────────────
    private enum State { Patrolling, Chasing }
    private State currentState = State.Patrolling;

    private Vector2 spawnPosition;
    private Vector2 currentWaypoint;
    private float   patrolWaitTimer  = 0f;
    private bool    waitingAtWaypoint = false;

    private Rigidbody2D rb;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        spawnPosition = transform.position;
        PickNewWaypoint();
    }

    void Update()
    {
        // Si el jugador fue eliminado, volver a merodear
        if (player == null)
        {
            currentState = State.Patrolling;
        }

        switch (currentState)
        {
            case State.Patrolling: HandlePatrol(); break;
            case State.Chasing:   HandleChase();  break;
        }
    }

    // ─── MERODEANDO ───────────────────────────────────────────────────────────
    private void HandlePatrol()
    {
        if (CanDetectPlayer())
        {
            currentState = State.Chasing;
            return;
        }

        if (waitingAtWaypoint)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
            {
                waitingAtWaypoint = false;
                PickNewWaypoint();
            }
            StopMovement();
            return;
        }

        float dist = Vector2.Distance(transform.position, currentWaypoint);
        if (dist <= waypointReachedDistance)
        {
            waitingAtWaypoint = true;
            patrolWaitTimer   = patrolWaitTime;
        }
        else
        {
            MoveTowards(currentWaypoint, patrolSpeed);
        }
    }

    // ─── PERSIGUIENDO ─────────────────────────────────────────────────────────
    private void HandleChase()
    {
        // Una vez detectado, persigue sin parar. Solo para si el player desaparece.
        if (player == null)
        {
            currentState = State.Patrolling;
            PickNewWaypoint();
            return;
        }

        MoveTowards(player.position, chaseSpeed);
    }

    // ─── DETECCIÓN INICIAL ────────────────────────────────────────────────────
    /// <summary>
    /// Detección por distancia + Raycast contra paredes.
    /// Si wallLayer no está configurado, detecta solo por distancia.
    /// </summary>
    private bool CanDetectPlayer()
    {
        if (player == null) return false;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > visionRadius) return false;

        // Si no hay wallLayer configurado, detecta directamente
        if (wallLayer.value == 0) return true;

        // Raycast: si choca una pared antes del jugador, no detecta
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, wallLayer);

        // Si el rayo no choca con nada (no hay pared entre medio) → detecta
        return hit.collider == null;
    }

    // ─── UTILIDADES ───────────────────────────────────────────────────────────
    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        if (rb != null)
            rb.linearVelocity = dir * speed;
        else
            transform.position += (Vector3)(dir * speed * Time.deltaTime);
    }

    private void StopMovement()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void PickNewWaypoint()
    {
        currentWaypoint = spawnPosition + UnityEngine.Random.insideUnitCircle * patrolRadius;
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        // Radio de detección – amarillo
        Gizmos.color = new UnityEngine.Color(1f, 0.9f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        // Radio de patrulla – azul
        Vector2 origin = UnityEngine.Application.isPlaying ? spawnPosition : (Vector2)transform.position;
        Gizmos.color = new UnityEngine.Color(0.2f, 0.5f, 1f, 0.2f);
        Gizmos.DrawWireSphere(origin, patrolRadius);
    }
}
