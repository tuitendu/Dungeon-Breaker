# 🏗️ Kiến Trúc Đã Xây (Scene: Home_TuiTenDu)

> Cập nhật: 2026-03-31 | Dự án: Doan2 - Defense Tower

---

## 📁 Cấu trúc thư mục Scripts

```
Assets/Scripts/
├── Enemy/
│   ├── Chi_So_Enemy/           # Chỉ số quái
│   ├── Combat/
│   │   ├── Quai_Thuong/        # Combat từng loại quái thường
│   │   │   ├── Orc/
│   │   │   ├── Plant/
│   │   │   ├── Slime_1/
│   │   │   └── Vampire/
│   │   └── Boss/               # (chưa implement)
│   ├── Rot_Item/               # Hệ thống rớt đồ
│   └── Spawm_Enemy/            # Spawn & patrol quái
├── Player/
│   ├── Chi_So/                 # Chỉ số player
│   ├── Combat/                 # Combat 3 class
│   ├── Inventory/              # Logic túi đồ
│   ├── Chuc_Nang/              # Di chuyển
│   ├── Nhat_Item/              # Nhặt EXP & vàng
│   └── Tuong_Tac_Hp_Mp/       # HealthBar & ManaBar
├── UI/
│   ├── Avatar/                 # Quản lý giao diện chiến sĩ
│   ├── Gird/                   # Ô chứa item
│   └── Inventory/              # Toàn bộ UI inventory
├── SaveSystem/                 # Lưu/load game
└── TopDownAnimator.cs          # Điều khiển animation chung
```

---

## ⚔️ Hệ thống Quái (Enemy)

### Chỉ số (Chi_So_Enemy/)
| Script | Mô tả |
|--------|-------|
| `EnemyStats.cs` | Component gán lên quái, load từ SO |
| `EnemyStatsData.cs` | ScriptableObject định nghĩa HP, ATK, DEF, EXP, gold... |
| `EnemyStatsData.asset` (×4) | Data SO: Slime, Plant, Vampire, Orc |

### AI & Spawn (Spawm_Enemy/)
| Script | Mô tả |
|--------|-------|
| `EnemyPatrol.cs` | AI tuần tra → phát hiện player → đuổi (A* Pathfinding) |
| `EnemySpawner.cs` | Spawn quái theo wave hoặc khu vực |

### Combat (Enemy/)
| Script | Mô tả |
|--------|-------|
| `EnemyAttackController.cs` | Điều phối vòng lặp tấn công, cooldown, range check |
| `EnemyHitbox.cs` | Phát hiện va chạm với vũ khí player |
| `EnemyHitboxBridge.cs` | Cầu nối giữa hitbox và hệ thống damage |
| `EnemySeparation.cs` | Tránh chồng chéo giữa các quái |
| `AttackBehaviour.cs` | Base interface tấn công |
| `MeleeAttackSO.cs` | Base SO cho tấn công cận chiến |

### Loại quái (Combat/Quai_Thuong/)
| Script | Mô tả |
|--------|-------|
| `OrcAttackSO.cs` + `.asset` | Data tấn công Orc (swing range, damage...) |
| `PlantAttackSO.cs` | Data tấn công Plant |
| `Slime_1.asset` | Data tấn công Slime |
| *(Vampire)* | *(asset tương tự)* |

### Rớt đồ (Rot_Item/)
| Script | Mô tả |
|--------|-------|
| `ItemData.cs` | Base class item |
| `EquipmentItemData.cs` | SO: item trang bị (ATK, DEF bonus...) |
| `ConsumableItemData.cs` | SO: item tiêu thụ (HP/MP potion...) |
| `DropTable.cs` | Bảng xác suất rớt đồ |
| `EnemyDropper.cs` | Khi quái chết → spawn item rớt xuống map |
| `Item_Roi.cs` | Hành vi của item nằm trên map (hút về player) |

---

## 🧙 Hệ thống Player

### Chỉ số (Chi_So/)
| Script | Mô tả |
|--------|-------|
| `PlayerStats.cs` | Tổng hợp chỉ số base + equipment bonus |
| `PlayerStatsData.cs` | SO chứa chỉ số gốc từng class |
| `PlayerLevel.cs` | Hệ thống EXP, lên cấp, tăng chỉ số |
| `PlayerWallet.cs` | Quản lý vàng |
| `SwordStats.asset` | Data SO chỉ số class Sword |

### Di chuyển (Chuc_Nang/)
| Script | Mô tả |
|--------|-------|
| `Playermove.cs` | WASD movement, tích hợp Input System |

### Combat (Combat/)
| Script | Mô tả |
|--------|-------|
| `Player_CombatBase.cs` | Abstract base: auto-target kẻ gần nhất |
| `Sword_Combat.cs` | Skill 1 swing, Skill 2, Skill 3 (SpinningSword) |
| `Archer_Combat.cs` | Bắn tên (projectile) |
| `Mage_Combat.cs` | Phép thuật AOE |
| `Player_Combat.cs` | Dispatcher chọn logic theo class |
| `Projec_Tile.cs` | Đạn/tên bay |
| `SpinningSwordEffect.cs` | Skill 3: kiếm quay quanh player liên tục |

### Nhặt đồ (Nhat_Item/)
| Script | Mô tả |
|--------|-------|
| `Nhat_Exp.cs` | Va chạm với EXP orb → cộng điểm EXP |
| `Nhat_Vang.cs` | Va chạm với coin → cộng vàng |

### HP/MP (Tuong_Tac_Hp_Mp/)
| Script | Mô tả |
|--------|-------|
| `HealthBar.cs` | Slider HP thanh trên đầu nhân vật |
| `ManaBar.cs` | Slider MP |

### Inventory (Inventory/)
| Script | Mô tả |
|--------|-------|
| `InventoryManager.cs` | Singleton, quản lý danh sách items trong túi |
| `EquipmentManager.cs` | Quản lý slot trang bị (vũ khí, giáp...) |
| `InventorySlot.cs` | Data một ô trong túi |

---

## 🖥️ UI

| Script | Mô tả |
|--------|-------|
| `UI_InventoryPanel.cs` | Panel túi đồ, mở/đóng |
| `UI_InventorySlot.cs` | Hiển thị + drag-drop 1 ô item |
| `UI_EquipmentSlot.cs` | Slot trang bị trên UI |
| `UI_QuickUseButton.cs` | Nút dùng nhanh item |
| `UI_QuickUseMenu.cs` | Menu chọn item dùng nhanh |
| `UI_QuickUseMenuSlot.cs` | Ô trong menu dùng nhanh |
| `UI_UseItemButton.cs` | Nút "Dùng" khi click vào item |
| `QuickUseManager.cs` | Logic quản lý shortcut dùng item |
| `UI_ExpBar.cs` | Thanh EXP trên HUD |
| `UI_GoldDisplay.cs` | Hiển thị số vàng |
| `Quan_Ly_Giao_Dien_CS.cs` | Toggle UI chiến sĩ (character sheet) |
| `Dinh_Nghia_O_Chua_Item.cs` | Định nghĩa ô grid chứa item |

---

## 💾 Save System (SaveSystem/)

| Script | Mô tả |
|--------|-------|
| `SaveData.cs` | Struct/class chứa toàn bộ data cần lưu |
| `SaveSystem.cs` | Đọc/ghi file JSON lên disk |
| `GameSaveManager.cs` | Orchestrator: thu thập data từ các hệ thống & khôi phục |
| `ItemDatabase.cs` | Tra cứu item bằng ID khi load game |
| `ItemDataBase.asset` | SO database tất cả item trong game |

---

## 🎬 Animation

| Script | Mô tả |
|--------|-------|
| `TopDownAnimator.cs` | Điều khiển Animator: idle/walk/attack theo hướng di chuyển |

---

## 📊 ScriptableObject Assets

| Asset | Vị trí |
|-------|--------|
| `EnemyStatsData` (Slime, Plant, Vampire, Orc) | `Enemy/Chi_So_Enemy/Chi_So_Class/` |
| `OrcAttackSO`, `PlantAttackSO` | `Enemy/Combat/Quai_Thuong/{loại}/` |
| `SwordStats` | `Player/Chi_So/Chi_So_Player/Chi_So_Class/` |
| `ItemDataBase` | `Assets/Database/` |
| Drop tables & item prefabs | `Enemy/Rot_Item/` |

---

## ✅ Tổng kết nhanh — Đã làm được

- [x] Player: di chuyển, 3 class combat (Sword/Archer/Mage), auto-target
- [x] Player: hệ thống HP/MP, EXP, lên cấp, vàng
- [x] Player: Inventory + Equipment + Quick Use
- [x] Enemy: 4 loại quái (Slime, Plant, Vampire, Orc) với SO riêng
- [x] Enemy: AI Patrol → Chase → Attack (A* Pathfinding)
- [x] Enemy: Hitbox, Separation, Attack cooldown
- [x] Enemy: Rớt đồ theo DropTable (Equipment + Consumable)
- [x] UI: HUD đầy đủ (HP, MP, EXP, Gold), Inventory panel, Equipment UI
- [x] Save/Load: JSON persistence toàn bộ trạng thái game
- [x] Animation: TopDown 8-direction animator
- [ ] Boss: (chưa implement)
- [ ] Scene transitions: (chưa rõ)
