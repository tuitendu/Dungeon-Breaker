using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Nút quick use cho consumable items (mobile-friendly)
/// - Tap = Dùng item ngay
/// - Hold = Mở menu chọn item khác
/// </summary>
public class UI_QuickUseButton : MonoBehaviour, 
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image background;
    
    [Header("Settings")]
    [SerializeField] private float holdDuration = 0.5f;  // Hold 0.5s để mở menu
    
    [Header("Visual Feedback")]
    [SerializeField] private float pressScale = 0.9f;    // Scale khi nhấn
    [SerializeField] private Color pressColor = new Color(0.8f, 0.8f, 0.8f);
    
    private float pressTime;
    private bool isHolding;
    private Vector3 originalScale;
    private Color originalColor;
    
    private void Awake()
    {
        originalScale = transform.localScale;
        if (background != null)
            originalColor = background.color;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        pressTime = Time.time;
        isHolding = true;
        
        // Visual feedback
        transform.localScale = originalScale * pressScale;
        if (background != null)
            background.color = pressColor;
        
        StartCoroutine(CheckHold());
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        
        // Reset visual
        transform.localScale = originalScale;
        if (background != null)
            background.color = originalColor;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Nếu không hold → dùng item
        float pressDuration = Time.time - pressTime;
        
        if (pressDuration < holdDuration)
        {
            if (QuickUseManager.Instance != null)
            {
                QuickUseManager.Instance.UseCurrentItem();
            }
        }
    }
    
    private IEnumerator CheckHold()
    {
        yield return new WaitForSeconds(holdDuration);
        
        if (isHolding)
        {
            // Hold đủ lâu → mở menu
            Debug.Log("[QuickUseButton] Hold detected → Opening menu");
            
            if (QuickUseManager.Instance != null)
            {
                QuickUseManager.Instance.OpenItemMenu();
            }
            
            // Vibration feedback (mobile)
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif
        }
    }
    
    /// <summary>
    /// Update hiển thị button
    /// </summary>
    public void UpdateDisplay(ConsumableItemData item, int count)
    {
        if (item != null && count > 0)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
            
            countText.text = $"x{count}";
            countText.enabled = true;
            
            background.enabled = true;
        }
        else
        {
            // Không có item → ẩn button
            iconImage.sprite = null;
            iconImage.enabled = false;
            countText.enabled = false;
            background.enabled = false;
        }
    }
}
