using System.Collections;
using UnityEngine;

/// <summary>
/// Script dieu khien NPC di chuyen ngau nhien (lang thang) va dung nghi.
/// Gan vao bat ky NPC nao co Rigidbody2D + Animator.
///
/// ===== HUONG DAN SU DUNG =====
/// 1. Gan script nay vao NPC prefab (vi du: Citizen5)
/// 2. Dam bao NPC co: Rigidbody2D (Kinematic), Animator, CapsuleCollider2D
/// 3. Animator can co cac Parameters:
///    - "IsMoving" (Bool)   : true khi dang di, false khi dung
///    - "MoveX"   (Float)  : huong ngang (-1 = trai, 1 = phai)
///    - "MoveY"   (Float)  : huong doc  (-1 = xuong, 1 = len)
/// 4. Chinh cac thong so trong Inspector theo y muon.
/// ==============================
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class NPCWander : MonoBehaviour
{
    // =========================================================
    //  CHE DO DI CHUYEN
    // =========================================================
    public enum WanderMode
    {
        [Tooltip("Di chuyen ngau nhien trong vong tron quanh diem spawn")]
        RandomInRadius,

        [Tooltip("Di theo danh sach Waypoints (tuan tu hoac ngau nhien)")]
        Waypoints
    }

    [Header("===== CHE DO =====")]
    [Tooltip("Chon cach NPC di chuyen")]
    public WanderMode wanderMode = WanderMode.RandomInRadius;

    // =========================================================
    //  THONG SO DI CHUYEN
    // =========================================================
    [Header("===== TOC DO =====")]
    [Tooltip("Toc do di chuyen cua NPC (don vi / giay)")]
    [Range(0.3f, 3f)]
    public float moveSpeed = 1f;

    [Header("===== CHE DO: Random In Radius =====")]
    [Tooltip("Ban kinh vung NPC duoc phep di lang thang (tinh tu diem spawn)")]
    [Range(1f, 15f)]
    public float wanderRadius = 4f;

    [Header("===== CHE DO: Waypoints =====")]
    [Tooltip("Danh sach cac diem di chuyen. Keo Transform vao day.")]
    public Transform[] waypoints;

    [Tooltip("Chon ngau nhien waypoint tiep theo thay vi di theo thu tu?")]
    public bool randomWaypointOrder = false;

    // =========================================================
    //  THONG SO THOI GIAN
    // =========================================================
    [Header("===== THOI GIAN NGHI =====")]
    [Tooltip("Thoi gian nghi toi thieu (giay) khi den dich")]
    [Range(0.5f, 10f)]
    public float idleTimeMin = 2f;

    [Tooltip("Thoi gian nghi toi da (giay) khi den dich")]
    [Range(1f, 20f)]
    public float idleTimeMax = 5f;

    [Header("===== KHOANG CACH =====")]
    [Tooltip("Khoang cach de coi la 'da den noi' (don vi)")]
    [Range(0.05f, 0.5f)]
    public float arrivalDistance = 0.15f;

    // =========================================================
    //  TRANG THAI NOI BO
    // =========================================================
    private enum NPCState { Idle, Walking }
    private NPCState currentState = NPCState.Idle;

    private Vector3 spawnPoint;        // Vi tri ban dau khi NPC xuat hien
    private Vector3 targetPosition;    // Diem dich dang di toi
    private Vector2 moveDirection;     // Huong di chuyen hien tai

    private int currentWaypointIndex = 0;

    // =========================================================
    //  THAM CHIEU COMPONENT
    // =========================================================
    private Rigidbody2D rb;
    private Animator    anim;
    private SpriteRenderer spriteRenderer;

    // Animator parameter hashes (toi uu, tranh dung string moi frame)
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashMoveX    = Animator.StringToHash("MoveX");
    private static readonly int HashMoveY    = Animator.StringToHash("MoveY");

    // =========================================================
    //  KHOI TAO
    // =========================================================
    private void Awake()
    {
        rb             = GetComponent<Rigidbody2D>();
        anim           = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Dam bao Rigidbody2D dung che do Kinematic (NPC khong bi day boi vat ly)
        rb.bodyType    = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        spawnPoint     = transform.position;
        targetPosition = transform.position;

        // Bat dau vong lap hanh vi: Nghi -> Tim diem -> Di -> Nghi -> ...
        StartCoroutine(BehaviorLoop());
    }

    // =========================================================
    //  VONG LAP HANH VI CHINH
    // =========================================================
    /// <summary>
    /// Coroutine chinh dieu khien toan bo hanh vi NPC:
    /// 1. Dung nghi ngoi (Idle) trong khoang thoi gian ngau nhien
    /// 2. Chon diem dich moi
    /// 3. Di chuyen den do
    /// 4. Lap lai tu buoc 1
    /// </summary>
    private IEnumerator BehaviorLoop()
    {
        while (true)
        {
            // ---------- BUOC 1: NGHI NGOI ----------
            SetState(NPCState.Idle);
            float idleDuration = Random.Range(idleTimeMin, idleTimeMax);
            yield return new WaitForSeconds(idleDuration);

            // ---------- BUOC 2: CHON DIEM DICH ----------
            targetPosition = PickNextDestination();

            // ---------- BUOC 3: DI CHUYEN ----------
            SetState(NPCState.Walking);
            yield return StartCoroutine(MoveToTarget());

            // Sau khi den noi -> quay lai buoc 1 (Idle)
        }
    }

    // =========================================================
    //  CHON DIEM DICH
    // =========================================================
    /// <summary>
    /// Tra ve vi tri tiep theo NPC se di den,
    /// tuy thuoc vao wanderMode dang chon.
    /// </summary>
    private Vector3 PickNextDestination()
    {
        switch (wanderMode)
        {
            // ---- Che do 1: Ngau nhien trong ban kinh ----
            case WanderMode.RandomInRadius:
                Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
                return spawnPoint + new Vector3(randomOffset.x, randomOffset.y, 0f);

            // ---- Che do 2: Theo Waypoints ----
            case WanderMode.Waypoints:
                if (waypoints == null || waypoints.Length == 0)
                {
                    // Khong co waypoint -> dung tai cho
                    return transform.position;
                }

                if (randomWaypointOrder)
                {
                    currentWaypointIndex = Random.Range(0, waypoints.Length);
                }
                else
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                }

                if (waypoints[currentWaypointIndex] != null)
                    return waypoints[currentWaypointIndex].position;
                else
                    return transform.position;

            default:
                return transform.position;
        }
    }

    // =========================================================
    //  DI CHUYEN TOI DIEM DICH
    // =========================================================
    /// <summary>
    /// Coroutine di chuyen NPC den targetPosition.
    /// Dung lai khi khoang cach <= arrivalDistance.
    /// </summary>
    private IEnumerator MoveToTarget()
    {
        while (true)
        {
            Vector2 currentPos = rb.position;
            Vector2 target2D   = new Vector2(targetPosition.x, targetPosition.y);
            float   distance   = Vector2.Distance(currentPos, target2D);

            // Da den noi?
            if (distance <= arrivalDistance)
            {
                rb.linearVelocity = Vector2.zero;
                yield break; // Thoat coroutine -> quay ve BehaviorLoop
            }

            // Tinh huong di chuyen
            moveDirection = (target2D - currentPos).normalized;

            // Cap nhat animation
            UpdateAnimation(moveDirection);

            // yield de FixedUpdate lo phan di chuyen thuc te
            yield return null;
        }
    }

    // =========================================================
    //  VẬT LÝ - DI CHUYEN
    // =========================================================
    private void FixedUpdate()
    {
        if (currentState == NPCState.Walking)
        {
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // =========================================================
    //  ANIMATION
    // =========================================================
    /// <summary>
    /// Cap nhat Animator parameters dua tren huong di chuyen.
    /// Su dung cung convention voi PlayerMove:
    ///   IsMoving (bool), MoveX (float), MoveY (float)
    /// </summary>
    private void UpdateAnimation(Vector2 direction)
    {
        if (anim == null) return;

        bool isMoving = currentState == NPCState.Walking && direction.sqrMagnitude > 0.01f;
        anim.SetBool(HashIsMoving, isMoving);

        if (isMoving)
        {
            // Uu tien truc co gia tri lon hon (giong Player)
            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
            {
                anim.SetFloat(HashMoveX, Mathf.Sign(direction.x));
                anim.SetFloat(HashMoveY, 0f);
            }
            else
            {
                anim.SetFloat(HashMoveX, 0f);
                anim.SetFloat(HashMoveY, Mathf.Sign(direction.y));
            }
        }
    }

    // =========================================================
    //  CHUYEN TRANG THAI
    // =========================================================
    private void SetState(NPCState newState)
    {
        currentState  = newState;
        moveDirection = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool(HashIsMoving, newState == NPCState.Walking);
        }

        if (newState == NPCState.Idle)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // =========================================================
    //  API CONG KHAI - DE SPAWNER GOI
    // =========================================================
    /// <summary>
    /// NPCSpawner goi ham nay de truyen waypoints sau khi Instantiate.
    /// </summary>
    public void SetWaypoints(Transform[] points)
    {
        waypoints            = points;
        currentWaypointIndex = 0;
    }

    /// <summary>
    /// Dat lai diem spawn (dung khi Spawner tao NPC o vi tri khac).
    /// </summary>
    public void SetSpawnPoint(Vector3 point)
    {
        spawnPoint = point;
    }

    // =========================================================
    //  GIZMOS - HIEN THI TRONG SCENE VIEW
    // =========================================================
    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? spawnPoint : transform.position;

        if (wanderMode == WanderMode.RandomInRadius)
        {
            // Vong tron vung lang thang (xanh la nhat)
            Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.15f);
            Gizmos.DrawSphere(center, wanderRadius);

            Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.7f);
            Gizmos.DrawWireSphere(center, wanderRadius);
        }

        if (wanderMode == WanderMode.Waypoints && waypoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawSphere(waypoints[i].position, 0.2f);

                if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
            // Noi diem cuoi ve diem dau
            if (waypoints.Length > 1 && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
                Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }

        // Dau X tai diem dich hien tai (khi dang Play)
        if (Application.isPlaying && currentState == NPCState.Walking)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(targetPosition + Vector3.up * 0.2f, targetPosition + Vector3.down * 0.2f);
            Gizmos.DrawLine(targetPosition + Vector3.left * 0.2f, targetPosition + Vector3.right * 0.2f);
        }

        // Dau X tai spawn point
        Gizmos.color = Color.white;
        Gizmos.DrawLine(center + Vector3.up * 0.15f, center + Vector3.down * 0.15f);
        Gizmos.DrawLine(center + Vector3.left * 0.15f, center + Vector3.right * 0.15f);
    }
}
