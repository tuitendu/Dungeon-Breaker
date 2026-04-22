using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager Instance;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        if (targetAlpha > 0)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        float startAlpha = canvasGroup.alpha;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;

        // Nếu màn hình đã sáng hoàn toàn (targetAlpha = 0), mở khóa để bấm nút
        if (targetAlpha <= 0)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}