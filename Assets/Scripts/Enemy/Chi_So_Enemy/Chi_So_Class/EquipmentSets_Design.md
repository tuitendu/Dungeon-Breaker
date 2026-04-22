# Thiết Kế 4 Bộ Trang Bị - Swordsman

## Chỉ Số Gốc Nhân Vật (SwordStats.asset)

| Chỉ số | Giá trị gốc |
|--------|------------|
| HP     | 26         |
| MP     | 70         |
| ATK    | 5          |
| DEF    | 10         |
| MDEF   | 6          |
| critRate | 0%       |

---

## Quy Ước Slot Trang Bị

| Slot | equipSlot | Chỉ số chính theo ảnh |
|------|-----------|----------------------|
| Weapon (Kiếm)       | 1 | +ATK (SpATK) |
| Armor (Giáp)        | 2 | +DEF (cộng giáp), +MDEF (giáp đặc biệt) |
| Pants (Quần/Mũ)     | 3 | +HP, hồi HP%/s |
| Boots (Giày)        | 4 | +MP, hồi MP%/s |
| Accessory (Trang sức)| 5 | +CritRate% |

> Rarity: Common=0 | Uncommon=1 | Rare=2 | Epic=3 | Legendary=4

---

## Bộ 1 — Common (Bình Thường) ⬜
**Tên bộ: "Chiến Binh Sơ Khai"**
**Giá bán: 50 vàng / món**

| Món | id | itemName | Slot | Chỉ số |
|-----|----|----------|------|--------|
| Kiếm       | set1_weapon_common    | Kiem Sat Gi    | Weapon    | ATK +2 |
| Giáp       | set1_armor_common     | Giap Vai Thu   | Armor     | DEF +3, MDEF +2 |
| Quần       | set1_pants_common     | Quan Vai Thu   | Pants     | HP +8, HPRegen 2%/s |
| Giày       | set1_boots_common     | Giay Da Thu    | Boots     | MP +15, MPRegen 3%/s |
| Trang sức  | set1_accessory_common | Vong Tay Sat  | Accessory | CritRate +5% |

**Tổng bonus cả bộ:**
- ATK +2 | DEF +3 | MDEF +2 | HP +8 | MP +15 | CritRate +5%
- HP sau khi mặc: 26 + 8 = **34** | MP: 70 + 15 = **85**
- ATK sau khi mặc: 5 + 2 = **7** | DEF: 10 + 3 = **13**

---

## Bộ 2 — Uncommon (Trung Bình) 🟩
**Tên bộ: "Thép Chiến"**
**Giá bán: 150 vàng / món**

| Món | id | itemName | Slot | Chỉ số |
|-----|----|----------|------|--------|
| Kiếm       | set2_weapon_uncommon    | Kiem Thep Chien     | Weapon    | ATK +4 |
| Giáp       | set2_armor_uncommon     | Giap Thep Chien     | Armor     | DEF +6, MDEF +4 |
| Quần       | set2_pants_uncommon     | Quan Thep Chien     | Pants     | HP +16, HPRegen 4%/s |
| Giày       | set2_boots_uncommon     | Giay Thep Chien     | Boots     | MP +28, MPRegen 5%/s |
| Trang sức  | set2_accessory_uncommon | Vong Co Ngoc Luc   | Accessory | CritRate +12% |

**Tổng bonus cả bộ:**
- ATK +4 | DEF +6 | MDEF +4 | HP +16 | MP +28 | CritRate +12%
- HP sau khi mặc: 26 + 16 = **42** | MP: 70 + 28 = **98**
- ATK sau khi mặc: 5 + 4 = **9** | DEF: 10 + 6 = **16**

---

## Bộ 3 — Rare (Cao) 🟦
**Tên bộ: "Bích Hoa"**
**Giá bán: 400 vàng / món**

| Món | id | itemName | Slot | Chỉ số |
|-----|----|----------|------|--------|
| Kiếm       | set3_weapon_rare    | Kiem Bich Hoa   | Weapon    | ATK +7, CritRate +5% |
| Giáp       | set3_armor_rare     | Giap Bich Hoa   | Armor     | DEF +12, MDEF +8 |
| Quần       | set3_pants_rare     | Quan Bich Hoa   | Pants     | HP +28, HPRegen 7%/s |
| Giày       | set3_boots_rare     | Giay Bich Hoa   | Boots     | MP +42, MPRegen 8%/s |
| Trang sức  | set3_accessory_rare | Nhan Bich Hoa  | Accessory | CritRate +20% |

**Tổng bonus cả bộ:**
- ATK +7 | DEF +12 | MDEF +8 | HP +28 | MP +42 | CritRate +25% (5% kiếm + 20% trang sức)
- HP sau khi mặc: 26 + 28 = **54** | MP: 70 + 42 = **112**
- ATK sau khi mặc: 5 + 7 = **12** | DEF: 10 + 12 = **22**

---

## Bộ 4 — Legendary (Huyền Thoại) 🟡
**Tên bộ: "Thiên Long"**
**Giá bán: 1500 vàng / món**

| Món | id | itemName | Slot | Chỉ số |
|-----|----|----------|------|--------|
| Kiếm       | set4_weapon_legendary    | Thien Long Kiem    | Weapon    | ATK +15, CritRate +10% |
| Giáp       | set4_armor_legendary     | Thien Long Giap    | Armor     | DEF +22, MDEF +16 |
| Quần       | set4_pants_legendary     | Thien Long Quan    | Pants     | HP +52, HPRegen 12%/s |
| Giày       | set4_boots_legendary     | Thien Long Hia     | Boots     | MP +70, MPRegen 15%/s |
| Trang sức  | set4_accessory_legendary | Thien Long Vu Khi  | Accessory | CritRate +35% |

**Tổng bonus cả bộ:**
- ATK +15 | DEF +22 | MDEF +16 | HP +52 | MP +70 | CritRate +45% (10% kiếm + 35% trang sức)
- HP sau khi mặc: 26 + 52 = **78** (x3 base HP) | MP: 70 + 70 = **140** (x2 base MP)
- ATK sau khi mặc: 5 + 15 = **20** (x4 base ATK) | DEF: 10 + 22 = **32** (x3.2 base DEF)

---

## So Sánh Tổng Hợp 4 Bộ

| Bộ | Tier | ATK bonus | DEF bonus | MDEF bonus | HP bonus | MP bonus | CritRate bonus |
|----|------|-----------|-----------|----------|----------|----------|----------------|
| Bộ 1 | Common     | +2  | +3  | +2  | +8  | +15 | +5%  |
| Bộ 2 | Uncommon   | +4  | +6  | +4  | +16 | +28 | +12% |
| Bộ 3 | Rare       | +7  | +12 | +8  | +28 | +42 | +25% |
| Bộ 4 | Legendary  | +15 | +22 | +16 | +52 | +70 | +45% |

## Tỷ Lệ Tăng So Với Base Stats (%)

| Bộ | ATK% | DEF% | MDEF% | HP%  | MP%  |
|----|------|------|-------|------|------|
| Bộ 1 | +40%  | +30%  | +33%  | +31%  | +21%  |
| Bộ 2 | +80%  | +60%  | +67%  | +62%  | +40%  |
| Bộ 3 | +140% | +120% | +133% | +108% | +60%  |
| Bộ 4 | +300% | +220% | +267% | +200% | +100% |

---

## Ghi Chú Thiết Kế

- **Kiếm Rare & Legendary** có bonus CritRate thêm vào kiếm (ngoài trang sức) để thể hiện vũ khí cao cấp có đặc tính đặc biệt.
- **HPRegen / MPRegen** hoạt động mỗi 1 giây (xử lý trong `EquipmentManager.HandleRegeneration()`), giá trị % tính trên max HP/MP.
- **Legendary** HP tăng x3, ATK tăng x4 so với gốc — phù hợp cho end-game.
- **File .asset** dùng `guid: 42d789335925d6b4bba248b24097f570` tương ứng với script `EquipmentItemData.cs`.
