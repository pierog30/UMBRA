using System;
using UnityEngine;

public class EnemyAI2D : MonoBehaviour
{
    public enum EnemyKind { SorrowSoul, GrotesqueDemon }
    public enum State { Patrol, Watch, Chase, Search, Return }

    public EnemyKind kind;
    public State CurrentState { get; private set; }
    public bool CanCurrentlySeePlayer { get; private set; }
    public Vector2 LastKnownPlayerPosition => lastSeen;
    public event Action<State, State> StateChanged;
    public float patrolDistance = 3f;
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.8f;
    public float viewDistance = 6.5f;
    public float verticalTolerance = 2.4f;
    [Range(30f, 180f)] public float visionAngle = 120f;
    public float lostSightGrace = 0.35f;
    public float wallCheckDistance = 0.55f;
    public float searchTime = 2.5f;
    public LayerMask obstacleMask;

    [Header("Runtime Debug")]
    [SerializeField] private State debugState;
    [SerializeField] private bool debugCanSeePlayer;
    [SerializeField] private float debugLostSightTime;

    private Vector2 origin;
    private Vector2 lastSeen;
    private Transform player;
    private float direction = 1f;
    private float timer;
    private float lostSightTimer;
    private float stateTime;
    private SpriteRenderer body;
    private Vector3 baseScale;
    private Color baseColor;

    private void Awake()
    {
        origin = transform.position;
        body = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseColor = body != null ? body.color : Color.white;
        CurrentState = State.Patrol;
        debugState = CurrentState;
        ApplyVisualFeedback(true);
    }

    private void Start()
    {
        PlayerController2D controller = FindAnyObjectByType<PlayerController2D>();
        player = controller != null ? controller.transform : null;
    }

    private void Update()
    {
        if (player == null || GameManager.Instance == null || !GameManager.Instance.CanPlayerMove)
        {
            return;
        }

        State stateAtFrameStart = CurrentState;
        PlayerController2D controller = player.GetComponent<PlayerController2D>();
        bool directSight = controller != null && !controller.IsHidden && CanSeePlayer();
        CanCurrentlySeePlayer = directSight;
        debugCanSeePlayer = directSight;
        if (directSight)
        {
            lostSightTimer = lostSightGrace;
            lastSeen = player.position;
            if (kind == EnemyKind.GrotesqueDemon && (CurrentState == State.Patrol || CurrentState == State.Return))
            {
                ChangeState(State.Watch, 0.55f);
            }
            else if (CurrentState != State.Watch)
            {
                ChangeState(State.Chase, searchTime);
            }
        }
        else
        {
            lostSightTimer = Mathf.Max(0f, lostSightTimer - Time.deltaTime);
        }
        debugLostSightTime = lostSightTimer;

        float speed = patrolSpeed;
        Vector2 target = origin + (Vector2.right * patrolDistance * direction);
        switch (CurrentState)
        {
            case State.Watch:
                timer -= Time.deltaTime;
                target = transform.position;
                if (timer <= 0f) ChangeState(State.Chase, searchTime);
                break;
            case State.Chase:
                speed = chaseSpeed;
                target = directSight ? (Vector2)player.position : lastSeen;
                if (!directSight && lostSightTimer <= 0f &&
                    Vector2.Distance(transform.position, lastSeen) < 0.35f)
                {
                    ChangeState(State.Search, searchTime);
                }
                break;
            case State.Search:
                timer -= Time.deltaTime;
                target = lastSeen + Vector2.right * Mathf.Sin(Time.time * 3f);
                if (timer <= 0f) ChangeState(State.Return);
                break;
            case State.Return:
                target = origin;
                if (Vector2.Distance(transform.position, origin) < 0.25f) ChangeState(State.Patrol);
                break;
            default:
                if (Vector2.Distance(transform.position, target) < 0.25f) direction *= -1f;
                break;
        }

        float nextX = Mathf.MoveTowards(transform.position.x, target.x, speed * Time.deltaTime);
        if (kind == EnemyKind.GrotesqueDemon &&
            (!HasGroundAt(nextX) || HasWallAhead(target.x - transform.position.x)))
        {
            if (CurrentState == State.Patrol)
            {
                direction *= -1f;
            }
            else
            {
                ChangeState(State.Search, searchTime);
            }
            nextX = transform.position.x;
        }
        transform.position = new Vector3(nextX, transform.position.y, transform.position.z);
        if (body != null && Mathf.Abs(target.x - transform.position.x) > 0.02f)
        {
            body.flipX = target.x < transform.position.x;
        }
        if (CurrentState == State.Chase && stateAtFrameStart != State.Chase)
        {
            UmbraAudio.Instance?.PlayScare();
            Camera.main?.GetComponent<CameraFollow2D>()?.AddTrauma(0.18f);
        }
        ApplyVisualFeedback(false);
    }

    private void ChangeState(State nextState, float nextTimer = -1f)
    {
        if (CurrentState == nextState)
        {
            return;
        }

        State previousState = CurrentState;
        CurrentState = nextState;
        debugState = CurrentState;
        stateTime = 0f;
        if (nextTimer >= 0f)
        {
            timer = nextTimer;
        }
        StateChanged?.Invoke(previousState, nextState);
        UmbraGameEvents.PublishInteraction(
            "Enemigo " + name + ": " + previousState + " a " + nextState);
    }

    private void ApplyVisualFeedback(bool immediate)
    {
        if (body == null)
        {
            return;
        }

        stateTime += Time.deltaTime;
        Color stateColor = baseColor;
        float pulseAmount = 0f;
        float pulseSpeed = 1f;
        switch (CurrentState)
        {
            case State.Watch:
                stateColor = baseColor * new Color(1f, 0.78f, 0.35f, 1f);
                pulseAmount = 0.035f;
                pulseSpeed = 7f;
                break;
            case State.Chase:
                stateColor = baseColor * new Color(1f, 0.38f, 0.38f, 1f);
                pulseAmount = 0.07f;
                pulseSpeed = 10f;
                break;
            case State.Search:
                stateColor = baseColor * new Color(0.72f, 0.55f, 1f, 1f);
                pulseAmount = 0.025f;
                pulseSpeed = 5f;
                break;
            case State.Return:
                stateColor = baseColor * new Color(0.62f, 0.78f, 0.9f, 1f);
                break;
        }
        stateColor.a = baseColor.a;

        float pulse = 1f + Mathf.Sin(stateTime * pulseSpeed) * pulseAmount;
        Vector3 targetScale = baseScale * pulse;
        if (immediate)
        {
            body.color = stateColor;
            transform.localScale = targetScale;
            return;
        }

        body.color = Color.Lerp(body.color, stateColor, Time.deltaTime * 8f);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10f);
    }

    private void OnDisable()
    {
        if (body != null)
        {
            body.color = baseColor;
        }
        transform.localScale = baseScale;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 patrolOrigin = Application.isPlaying ? (Vector3)origin : transform.position;
        Gizmos.color = new Color(0.25f, 0.75f, 1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, viewDistance);
        Gizmos.DrawLine(
            patrolOrigin + Vector3.left * patrolDistance,
            patrolOrigin + Vector3.right * patrolDistance);

        if (Application.isPlaying && CurrentState != State.Patrol)
        {
            Gizmos.color = CurrentState == State.Chase ? Color.red : new Color(0.7f, 0.35f, 1f, 1f);
            Gizmos.DrawWireSphere(lastSeen, 0.25f);
            Gizmos.DrawLine(transform.position, lastSeen);
        }
    }

    private bool HasGroundAt(float x)
    {
        Vector2 originPoint = new Vector2(x, transform.position.y - 0.35f);
        return Physics2D.Raycast(originPoint, Vector2.down, 1.5f, obstacleMask).collider != null;
    }

    private bool CanSeePlayer()
    {
        Vector2 delta = player.position - transform.position;
        if (delta.magnitude > viewDistance || Mathf.Abs(delta.y) > verticalTolerance)
        {
            return false;
        }
        if (kind == EnemyKind.GrotesqueDemon &&
            (CurrentState == State.Patrol || CurrentState == State.Return))
        {
            Vector2 facing = direction >= 0f ? Vector2.right : Vector2.left;
            if (Vector2.Angle(facing, delta) > visionAngle * 0.5f)
            {
                return false;
            }
        }
        RaycastHit2D hit = Physics2D.Raycast(transform.position, delta.normalized, delta.magnitude, obstacleMask);
        return hit.collider == null;
    }

    private bool HasWallAhead(float movement)
    {
        if (Mathf.Abs(movement) < 0.01f) return false;
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.1f;
        Vector2 rayDirection = movement > 0f ? Vector2.right : Vector2.left;
        return Physics2D.Raycast(
            rayOrigin,
            rayDirection,
            wallCheckDistance,
            obstacleMask).collider != null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController2D controller = other.GetComponent<PlayerController2D>();
        if (controller != null && !controller.IsHidden)
        {
            other.GetComponent<PlayerRespawn>()?.Die();
        }
    }
}
