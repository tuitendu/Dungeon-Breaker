using UnityEngine;

/// <summary>
/// Dữ liệu cho 1 slot trong shop: item cần bán + giá vàng
/// </summary>
[CreateAssetMenu(menuName = "Game/Shop Item")]
public class ShopItemData : ScriptableObject
{
    public ItemData item;       // Item sẽ được bán (kéo ItemData vào đây)
    public int price = 100;     // Giá vàng
    public int amount = 1;      // Số lượng item nhận được khi mua
}
