using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gan vao mot GameObject trong Scene de tao khu vuc sinh quai.
/// Quai se duoc sinh ngau nhien trong vong tron spawnRadius.
/// Moi con quai chet se duoc hoi sinh sau respawnTime giay.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("Prefab cua quai vat muon sinh ra (vi du: Slime_1)")]
    public GameObject enemyPrefab;

    [Tooltip("So luong quai toi da song cung luc tai khu vuc nay")]
    public int maxEnemies = 3;

    [Tooltip("Thoi gian cho hoi sinh (giay) sau khi 1 con quai chet")]
    public float respawnTime = 5f;

    [Tooltip("Ban kinh vung sinh quai - Quai se xuat hien ngau nhien trong vong tron nay")]
    public float spawnRadius = 2f;

    [Header("Waypoints (Duong tuan tra)")]
    [Tooltip("Keo cac Transform lam diem di chuyen vao day. Quai sinh ra se di tuan theo thu tu.")]
    public Transform[] spawnerWaypoints;

    // Danh sach cac con quai dang song
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        // Sinh ra du so luong quai khi bat dau game
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnEnemy();
        }
    }

    /// <summary>
    /// Sinh ra 1 con quai ngay tai vi tri ngau nhien trong vong tron spawner.
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"[EnemySpawner] '{gameObject.name}' chua gan enemyPrefab!", this);
            return;
        }

        // Vi tri spawn ngau nhien trong vong tron
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        // Tao ra con quai
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        newEnemy.name = $"{enemyPrefab.name}_[{activeEnemies.Count}]";

        // Xep vao lam con cua Spawner trong Hierarchy cho gon
        newEnemy.transform.SetParent(this.transform);

        // Truyen danh sach Waypoints cho script Patrol cua no
        EnemyPatrol patrol = newEnemy.GetComponent<EnemyPatrol>();
        if (patrol != null)
        {
            patrol.SetWaypoints(spawnerWaypoints);
        }
        else
        {
            Debug.LogWarning($"[EnemySpawner] Prefab '{enemyPrefab.name}' khong co EnemyPatrol.cs!", newEnemy);
        }

        activeEnemies.Add(newEnemy);

        // Theo doi cai chet cua quai de hoi sinh
        StartCoroutine(WatchForDeath(newEnemy));
    }

    /// <summary>
    /// Cho den khi con quai bi Destroy (== null), sau do doi respawnTime de sinh lai.
    /// </summary>
    private IEnumerator WatchForDeath(GameObject enemy)
    {
        // Cho den khi no bi Destroy
        while (enemy != null)
        {
            yield return new WaitForSeconds(0.5f); // Kiem tra moi 0.5 giay, nhe hon Update
        }

        // Quai da chet, don khoi danh sach
        activeEnemies.Remove(enemy); // se tu dong bo null

        Debug.Log($"[EnemySpawner] '{gameObject.name}': Quai chet! Hoi sinh sau {respawnTime}s...");

        // Doi roi sinh lai
        yield return new WaitForSeconds(respawnTime);

        SpawnEnemy();
    }

    // Hien thi vung spawner trong Scene View de thuan tien chinh sua
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f); // Cam nhat
        Gizmos.DrawSphere(transform.position, spawnRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // Cam dam (vien ngoai)
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    // Hien thi waypoints de de nhin
    private void OnDrawGizmosSelected()
    {
        if (spawnerWaypoints == null || spawnerWaypoints.Length == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < spawnerWaypoints.Length; i++)
        {
            if (spawnerWaypoints[i] == null) continue;
            Gizmos.DrawSphere(spawnerWaypoints[i].position, 0.3f);

            // Ve duong noi giua cac waypoints
            if (i + 1 < spawnerWaypoints.Length && spawnerWaypoints[i + 1] != null)
                Gizmos.DrawLine(spawnerWaypoints[i].position, spawnerWaypoints[i + 1].position);
        }
        // Noi waypoint cuoi ve dau
        if (spawnerWaypoints.Length > 1 && spawnerWaypoints[0] != null && spawnerWaypoints[spawnerWaypoints.Length - 1] != null)
            Gizmos.DrawLine(spawnerWaypoints[spawnerWaypoints.Length - 1].position, spawnerWaypoints[0].position);
    }
}
