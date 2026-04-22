using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ManaBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;

    [Header("Animation")]
    [SerializeField] private float fillSpeed = 5f;

    private float targetFill = 1f;
    private int currentMax = 100;

    public void SetMaxMana(int mana)
    {
        currentMax = mana;
        if (fillImage != null && mana > 0)
        {
            float fill = (float)fillImage.fillAmount * currentMax;
            targetFill = fill / currentMax;
        }
    }

    public void SetMana(int mana, bool snap = false)
    {
        if (currentMax > 0)
        {
            targetFill = (float)mana / currentMax;
            targetFill = Mathf.Clamp01(targetFill);
            if (snap && fillImage != null)
            {
                fillImage.fillAmount = targetFill;
            }
        }
    }

    private void Update()
    {
        if (fillImage != null)
        {
            if (Mathf.Abs(fillImage.fillAmount - targetFill) > 0.001f)
            {
                fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, fillSpeed * Time.deltaTime);
            }
            else
            {
                fillImage.fillAmount = targetFill;
            }
        }
    }

    private void OnValidate()
    {
        if (fillImage == null)
        {
            Transform fill = transform.Find("Fill");
            if (fill != null)
                fillImage = fill.GetComponent<Image>();
        }
    }
}
