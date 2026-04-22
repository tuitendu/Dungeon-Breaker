using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("===== NPC PREFAB =====")]
    [Tooltip("Prefab cua NPC muon sinh ra (phai co NPCWander.cs)")]
    public GameObject npcPrefab;

    [Header("===== SO LUONG & THOI GIAN =====")]
    [Tooltip("So luong NPC toi da song cung luc")]
    public int maxNPCs = 3;

    [Tooltip("Thoi gian hoi sinh (giay) sau khi 1 NPC bi huy")]
    public float respawnTime = 30f;

    [Header("===== KHU VUC SPAWN =====")]
    [Tooltip("Ban kinh vung sinh NPC ngau nhien quanh vi tri Spawner")]
    public float spawnRadius = 3f;

    [Header("===== WAYPOINTS CHIA SE (TUY CHON) =====")]
    [Tooltip("Keo cac Transform lam waypoints. NPC se nhan waypoints nay de di tuan.\n" +
             "Neu de trong, NPC se tu dong dung che do RandomInRadius.")]
    public Transform[] sharedWaypoints;

    [Header("===== TUY CHON NPC =====")]
    [Tooltip("Ghi de ban kinh lang thang cua NPC? (-1 = giu nguyen default cua prefab)")]
    public float overrideWanderRadius = -1f;

    [Tooltip("Ghi de toc do di chuyen cua NPC? (-1 = giu nguyen default cua prefab)")]
    public float overrideMoveSpeed = -1f;

    // Danh sach NPC dang song
    private List<GameObject> activeNPCs = new List<GameObject>();
    private void Start()
    {
        for (int i = 0; i < maxNPCs; i++)
        {
            SpawnNPC();
        }
    }

    private void SpawnNPC()
    {
        if (npcPrefab == null)
        {
            Debug.LogError($"[NPCSpawner] '{gameObject.name}' chua gan npcPrefab!", this);
            return;
        }

        // Vi tri spawn ngau nhien trong vong tron
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        // Tao NPC
        GameObject newNPC = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
        newNPC.name = $"{npcPrefab.name}_[{activeNPCs.Count}]";

        // Xep lam con cua Spawner cho gon
        newNPC.transform.SetParent(this.transform);

        // Cau hinh NPCWander
        NPCWander wander = newNPC.GetComponent<NPCWander>();
        if (wander != null)
        {
            // Truyen spawn point
            wander.SetSpawnPoint(spawnPos);

            // Truyen waypoints (neu co)
            if (sharedWaypoints != null && sharedWaypoints.Length > 0)
            {
                wander.SetWaypoints(sharedWaypoints);
                wander.wanderMode = NPCWander.WanderMode.Waypoints;
            }
            else
            {
                wander.wanderMode = NPCWander.WanderMode.RandomInRadius;
            }

            // Ghi de thong so (neu duoc cau hinh)
            if (overrideWanderRadius > 0f)
                wander.wanderRadius = overrideWanderRadius;

            if (overrideMoveSpeed > 0f)
                wander.moveSpeed = overrideMoveSpeed;
        }
        else
        {
            Debug.LogWarning($"[NPCSpawner] Prefab '{npcPrefab.name}' khong co NPCWander.cs!", newNPC);
        }

        activeNPCs.Add(newNPC);

        // Theo doi NPC de hoi sinh khi bi huy
        StartCoroutine(WatchForDestroy(newNPC));
    }

    // =========================================================
    //  THEO DOI & HOI SINH
    // =========================================================
    private IEnumerator WatchForDestroy(GameObject npc)
    {
        // Cho den khi NPC bi Destroy
        while (npc != null)
        {
            yield return new WaitForSeconds(1f);
        }

        activeNPCs.Remove(npc);
        Debug.Log($"[NPCSpawner] '{gameObject.name}': NPC mat! Hoi sinh sau {respawnTime}s...");

        yield return new WaitForSeconds(respawnTime);
        SpawnNPC();
    }

    // =========================================================
    //  GIZMOS
    // =========================================================
    private void OnDrawGizmos()
    {
        // Vung spawn (xanh duong nhat)
        Gizmos.color = new Color(0.3f, 0.5f, 1f, 0.15f);
        Gizmos.DrawSphere(transform.position, spawnRadius);

        Gizmos.color = new Color(0.3f, 0.5f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    private void OnDrawGizmosSelected()
    {
        if (sharedWaypoints == null || sharedWaypoints.Length == 0) return;

        Gizmos.color = new Color(0f, 1f, 0.8f, 1f); // Xanh ngoc
        for (int i = 0; i < sharedWaypoints.Length; i++)
        {
            if (sharedWaypoints[i] == null) continue;
            Gizmos.DrawSphere(sharedWaypoints[i].position, 0.25f);

            if (i + 1 < sharedWaypoints.Length && sharedWaypoints[i + 1] != null)
                Gizmos.DrawLine(sharedWaypoints[i].position, sharedWaypoints[i + 1].position);
        }
        if (sharedWaypoints.Length > 1 && sharedWaypoints[0] != null
            && sharedWaypoints[sharedWaypoints.Length - 1] != null)
        {
            Gizmos.DrawLine(sharedWaypoints[sharedWaypoints.Length - 1].position,
                            sharedWaypoints[0].position);
        }
    }
}
