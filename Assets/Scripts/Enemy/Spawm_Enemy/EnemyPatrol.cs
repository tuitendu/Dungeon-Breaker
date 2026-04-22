using UnityEngine;
using Pathfinding;
using System.Collections;

/// <summary>
/// AI cho quai vat: Tu dong chuyen 3 trang thai Patrol -> Chase -> Return.
/// Yeu cau: AIPath + AIDestinationSetter + EnemyAttackController tren cung GameObject.
/// </summary>
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(EnemyAttackController))]
public class EnemyPatrol : MonoBehaviour
{
    // =========================================================
    //  CHUYEN TRANG THAI (State Machine)
    // =========================================================
    private enum EnemyState { Patrol, Chase, Return }
    private EnemyState currentState = EnemyState.Patrol;

    // =========================================================
    //  THAM SO CHINH (Chinh trong Inspector)
    // =========================================================
    [Header("Toc Do Di Chuyen")]
    [Tooltip("Toc do khi di tuan tra")]
    public float patrolSpeed = 1.5f;

    [Tooltip("Toc do khi duoi theo Player")]
    public float chaseSpeed = 3f;

    [Tooltip("Toc do khi quay ve diem spawn")]
    public float returnSpeed = 4f;

    [Header("Pham Vi Phat Hien")]
    [Tooltip("Khoang cach bat dau di nguoi Player (hinh tron vang trong Scene View)")]
    public float aggroRange = 5f;

    [Tooltip("Khoang cach toi da duoi Player truoc khi quay ve (hinh tron do trong Scene View)")]
    public float leashRange = 12f;

    [Header("Tuan Tra")]
    [Tooltip("Thoi gian dung cho tai moi diem waypoint (giay)")]
    public float waitTimeAtPoint = 2f;

    // =========================================================
    //  BIEN NOI BO
    // =========================================================
    private Transform[] waypoints;
    private int waypointIndex = 0;
    private bool isWaitingAtPoint = false;

    private Vector3 spawnPoint; // Luu diem spawn khi quai xuat hien

    private AIPath               aiPath;
    private AIDestinationSetter  destinationSetter;
    private EnemyAttackController attackCtrl;
    private EnemyStats           enemyStats;

    // Cache Player: tim 1 lan roi luu lai cho nhe
    private Transform playerTransform;
    private float playerColliderRadius = 0.2f;  // cache bán kính collider player
    private float selfColliderRadius   = 0.15f; // cache bán kính collider enemy

    // =========================================================
    //  KHOI TAO
    // =========================================================
    private void Awake()
    {
        aiPath            = GetComponent<AIPath>();
        destinationSetter = GetComponent<AIDestinationSetter>();
        attackCtrl        = GetComponent<EnemyAttackController>();
        enemyStats        = GetComponent<EnemyStats>();

        // Tim Player 1 lan duy nhat (nen Player dung tag "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            // Lấy bán kính collider thực tế của player
            CircleCollider2D pc = playerObj.GetComponentInChildren<CircleCollider2D>();
            if (pc != null) playerColliderRadius = pc.radius;
            else
            {
                CapsuleCollider2D cc = playerObj.GetComponentInChildren<CapsuleCollider2D>();
                if (cc != null) playerColliderRadius = cc.size.x * 0.5f;
            }
        }
        // Lấy bán kính collider enemy
        CircleCollider2D sc = GetComponentInChildren<CircleCollider2D>();
        if (sc != null) selfColliderRadius = sc.radius;
        else
        {
            CapsuleCollider2D cc = GetComponentInChildren<CapsuleCollider2D>();
            if (cc != null) selfColliderRadius = cc.size.x * 0.5f;
        }
    }

    private void Start()
    {
        spawnPoint = transform.position; // Luu lai vi tri sinh ra

        // Neu khong co waypoint nao -> quat tran cung duoc, quai dung tai cho
        if (waypoints == null || waypoints.Length == 0)
        {
            destinationSetter.target = null;
            aiPath.canMove = false;
        }

        SetState(EnemyState.Patrol);
    }

    // =========================================================
    //  CAP NHAT MO HINH TRANG THAI
    // =========================================================
    private void Update()
    {
        if (playerTransform == null) return;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        float distToHome   = Vector2.Distance(transform.position, spawnPoint);

        switch (currentState)
        {
            // -------------------------------------------------
            case EnemyState.Patrol:
                UpdatePatrol();
                if (distToPlayer <= aggroRange)
                    SetState(EnemyState.Chase); // Phat hien Player -> Duoi
                break;

            // -------------------------------------------------
            case EnemyState.Chase:
                // Cap nhat dich den lien tuc theo Player
                destinationSetter.target = playerTransform;

                if (distToPlayer > leashRange || distToHome > leashRange)
                    SetState(EnemyState.Return); // Qua xa -> Quay ve
                break;

            // -------------------------------------------------
            case EnemyState.Return:
                // Khi da ve gan diem spawn -> Patrol lai
                if (distToHome <= aiPath.endReachedDistance + 0.3f)
                    SetState(EnemyState.Patrol);

                // Player lai tien lai gan -> Chase lai
                else if (distToPlayer <= aggroRange)
                    SetState(EnemyState.Chase);
                break;
        }
    }

    // =========================================================
    //  LOGIC PATROL
    // =========================================================
    private void UpdatePatrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (isWaitingAtPoint) return;

        float distToWaypoint = Vector2.Distance(transform.position, waypoints[waypointIndex].position);
        if (distToWaypoint <= aiPath.endReachedDistance + 0.5f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaitingAtPoint = true;
        aiPath.canMove   = false; // Dung lai

        yield return new WaitForSeconds(waitTimeAtPoint);

        // Chuyen waypoint tiep theo (vong lap)
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
        destinationSetter.target = waypoints[waypointIndex];

        aiPath.canMove   = true;
        isWaitingAtPoint = false;
    }

    // =========================================================
    //  DOI TRANG THAI
    // =========================================================
    private void SetState(EnemyState newState)
    {
        if (currentState == newState) return; // Khong doi trang thai neu da giong

        currentState = newState;
        isWaitingAtPoint = false;
        StopAllCoroutines(); // Huy bo moi Coroutine cu (vi du: WaitAtWaypoint dang chay)

        switch (newState)
        {
            case EnemyState.Patrol:
                aiPath.maxSpeed             = patrolSpeed;
                aiPath.endReachedDistance   = 0.5f; // Reset ve default
                aiPath.canMove              = true;
                if (waypoints != null && waypoints.Length > 0)
                    destinationSetter.target = waypoints[waypointIndex];
                else
                    aiPath.canMove = false;
                break;

            case EnemyState.Chase:
                aiPath.maxSpeed  = chaseSpeed;
                aiPath.canMove   = true;
                aiPath.isStopped = false;

                // endReachedDistance = AttackRange (edge-to-edge) + cả 2 bán kính collider
                // Vì A* đo center-to-center, nhưng collider ngăn không cho 2 tâm chạm nhau.
                // Nếu không cộng bán kính, A* nghĩ "chưa tới" và cứ chạy mãi.
                float attackRange = (enemyStats != null) ? enemyStats.AttackRange : 0.5f;
                aiPath.endReachedDistance = attackRange + selfColliderRadius + playerColliderRadius;
                destinationSetter.target  = playerTransform;
                break;

            case EnemyState.Return:
                aiPath.maxSpeed          = returnSpeed;
                aiPath.canMove           = true;
                // Tao 1 Transform tam thoi tai vi tri spawnPoint de A* co the dich
                destinationSetter.target  = CreateTempTarget(spawnPoint);
                break;
        }

        Debug.Log($"[{name}] Trang thai: {newState}");
    }

    // =========================================================
    //  HO TRO: TAO TARGET TAM THOI
    //  (A* can 1 Transform de theo, nen tao 1 GO nho lam dich)
    // =========================================================
    private Transform tempReturnTarget;

    private Transform CreateTempTarget(Vector3 position)
    {
        if (tempReturnTarget == null)
        {
            GameObject go      = new GameObject($"_ReturnTarget_{name}");
            go.hideFlags       = HideFlags.HideInHierarchy; // An trong Hierarchy cho gon
            tempReturnTarget   = go.transform;
        }
        tempReturnTarget.position = position;
        return tempReturnTarget;
    }

    // =========================================================
    //  SPAWNER GOI HAM NAY DE TRUYEN WAYPOINTS
    // =========================================================
    /// <summary>
    /// EnemySpawner goi ham nay ngay sau Instantiate de truyen tuyen duong tuan tra.
    /// </summary>
    public void SetWaypoints(Transform[] points)
    {
        waypoints     = points;
        waypointIndex = 0;
        if (waypoints != null && waypoints.Length > 0)
            destinationSetter.target = waypoints[0];
    }

    // =========================================================
    //  DON DEP
    // =========================================================
    private void OnDestroy()
    {
        // Khi quai bi Destroy, xoa luon target tam thoi
        if (tempReturnTarget != null)
            Destroy(tempReturnTarget.gameObject);
    }

    // =========================================================
    //  GIZMOS DEBUG
    // =========================================================
    private void OnDrawGizmosSelected()
    {
        // Vong tron phat hien (vang)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        // Vong tron leash (do)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, leashRange);

        // Dau X tai diem spawn (mau trang)
        Gizmos.color = Color.white;
        Vector3 sp = Application.isPlaying ? spawnPoint : transform.position;
        Gizmos.DrawLine(sp + Vector3.up * 0.3f,   sp + Vector3.down  * 0.3f);
        Gizmos.DrawLine(sp + Vector3.left * 0.3f, sp + Vector3.right * 0.3f);
    }
}
