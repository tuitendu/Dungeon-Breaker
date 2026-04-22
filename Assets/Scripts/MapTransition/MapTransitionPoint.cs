using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEngine.UIElements.NavigationMoveEvent;

public class MapTransitionPoint : MonoBehaviour
{
    [Header("Camera Config")]
    [SerializeField] private PolygonCollider2D mapBoundary;
    private CinemachineConfiner2D cinemachineConfiner;

    [Header("Transition Config")]
    [SerializeField] private string targetMapID;
    [SerializeField] private Transform targetTransition;
    [SerializeField] private Vector2 spawnOffset;

    private static bool isTransitioning = false;

    private void Awake() => cinemachineConfiner = FindAnyObjectByType<CinemachineConfiner2D>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Quan trọng: Chỉ chạy nếu là Player và chưa đang transition
        if (collision.CompareTag("Player") && !isTransitioning)
        {
            StartCoroutine(ExecuteTransition(collision));
        }
    }

    private IEnumerator ExecuteTransition(Collider2D player)
    {
        isTransitioning = true;

        if (SceneFadeManager.Instance != null)
            yield return StartCoroutine(SceneFadeManager.Instance.FadeRoutine(1f, 1.2f));

  
        player.transform.position = targetTransition.position + (Vector3)spawnOffset;

        // Cập nhật Map ID cho SaveSystem
        if (GameSaveManager.Instance != null)
            GameSaveManager.Instance.currentMapID = targetMapID;

        if (cinemachineConfiner != null)
        {
            cinemachineConfiner.BoundingShape2D = mapBoundary;
            cinemachineConfiner.InvalidateBoundingShapeCache();
        }

        yield return new WaitForSeconds(0.7f);

        if (SceneFadeManager.Instance != null)
            yield return StartCoroutine(SceneFadeManager.Instance.FadeRoutine(0f, 1.2f));

        isTransitioning = false;
    }
}
