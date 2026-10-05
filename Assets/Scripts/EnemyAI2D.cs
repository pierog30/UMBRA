using UnityEngine;

public class EnemyAI2D : MonoBehaviour
{
    public enum EnemyKind { SorrowSoul, GrotesqueDemon }
    public enum State { Patrol, Watch, Chase, Search, Return }

    public EnemyKind kind;
    public State CurrentState { get; private set; }
    public float patrolDistance = 3f;
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.8f;
    public float viewDistance = 6.5f;
    public float searchTime = 2.5f;
    public LayerMask obstacleMask;

    private Vector2 origin;
    private Vector2 lastSeen;
    private Transform player;
    private float direction = 1f;
    private float timer;
    private SpriteRenderer body;

    private void Awake()
    {
        origin = transform.position;
        body = GetComponent<SpriteRenderer>();
        CurrentState = State.Patrol;
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
        bool canSee = controller != null && !controller.IsHidden && CanSeePlayer();
        if (canSee)
        {
            lastSeen = player.position;
            if (kind == EnemyKind.GrotesqueDemon && (CurrentState == State.Patrol || CurrentState == State.Return))
            {
                CurrentState = State.Watch;
                timer = 0.55f;
            }
            else if (CurrentState != State.Watch)
            {
                CurrentState = State.Chase;
                timer = searchTime;
            }
        }

        float speed = patrolSpeed;
        Vector2 target = origin + (Vector2.right * patrolDistance * direction);
        switch (CurrentState)
        {
            case State.Watch:
                timer -= Time.deltaTime;
                target = transform.position;
                if (timer <= 0f) CurrentState = State.Chase;
                break;
            case State.Chase:
                speed = chaseSpeed;
                target = canSee ? (Vector2)player.position : lastSeen;
                if (!canSee && Vector2.Distance(transform.position, lastSeen) < 0.35f)
                {
                    CurrentState = State.Search;
                    timer = searchTime;
                }
                break;
            case State.Search:
                timer -= Time.deltaTime;
                target = lastSeen + Vector2.right * Mathf.Sin(Time.time * 3f);
                if (timer <= 0f) CurrentState = State.Return;
                break;
            case State.Return:
                target = origin;
                if (Vector2.Distance(transform.position, origin) < 0.25f) CurrentState = State.Patrol;
                break;
            default:
                if (Vector2.Distance(transform.position, target) < 0.25f) direction *= -1f;
                break;
        }

        float nextX = Mathf.MoveTowards(transform.position.x, target.x, speed * Time.deltaTime);
        if (kind == EnemyKind.GrotesqueDemon && !HasGroundAt(nextX))
        {
            if (CurrentState == State.Patrol)
            {
                direction *= -1f;
            }
            else
            {
                CurrentState = State.Search;
                timer = searchTime;
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
    }

    private bool HasGroundAt(float x)
    {
        Vector2 originPoint = new Vector2(x, transform.position.y - 0.35f);
        return Physics2D.Raycast(originPoint, Vector2.down, 1.5f, obstacleMask).collider != null;
    }

    private bool CanSeePlayer()
    {
        Vector2 delta = player.position - transform.position;
        if (delta.magnitude > viewDistance || Mathf.Abs(delta.y) > 2.4f)
        {
            return false;
        }
        if (kind == EnemyKind.GrotesqueDemon &&
            (CurrentState == State.Patrol || CurrentState == State.Return) &&
            Mathf.Sign(delta.x) != Mathf.Sign(direction))
        {
            return false;
        }
        RaycastHit2D hit = Physics2D.Raycast(transform.position, delta.normalized, delta.magnitude, obstacleMask);
        return hit.collider == null;
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
