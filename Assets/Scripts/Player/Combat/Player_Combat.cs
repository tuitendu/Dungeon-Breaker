using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player_Combat : MonoBehaviour
{
    private Player_CombatBase combat;
    private Animator anim;

    private bool basicOnCD;
    private bool skill1OnCD;
    private bool skill2OnCD;
    private bool skill3OnCD;
    private bool skill4OnCD;

    // Cơ chế Global Cooldown (Khoá các action khác khi đang ra chiêu)
    private bool isActionLocked;
    public float actionLockTime = 0.5f;

    // ===== SHOP HOOK =====
    // NPC Shop sẽ đăng ký vào đây khi player bước vào vùng trigger
    private NPC_Shop _nearbyShop = null;

    /// <summary>Được gọi bởi NPC_Shop khi player bước vào vùng trigger</summary>
    public void RegisterNearbyShop(NPC_Shop shop)
    {
        _nearbyShop = shop;
        Debug.Log("[Player_Combat] Đã đăng ký NPC Shop gần đây");
    }

    /// <summary>Được gọi bởi NPC_Shop khi player bước ra khỏi vùng trigger</summary>
    public void UnregisterNearbyShop(NPC_Shop shop)
    {
        if (_nearbyShop == shop)
        {
            _nearbyShop = null;
            Debug.Log("[Player_Combat] Đã huỷ đăng ký NPC Shop");
        }
    }

    [Header("UI")]
    public Image basicFill;
    public Image skill1Fill;
    public Image skill2Fill;
    public Image skill3Fill;
    public Image skill4Fill;

    private PlayerAudio _audio;

    private void Awake()
    {
        combat = GetComponent<Player_CombatBase>();
        anim   = GetComponent<Animator>();
        _audio = GetComponent<PlayerAudio>();

        if (combat == null)
            Debug.LogError("Không tìm thấy Player_CombatBase");
    }

    // ===== BASIC =====
    private void FaceNearestEnemy()
    {
        if (combat == null) return;
        
        // Quét quái trong bán kính 10 đơn vị
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, 10f, combat.enemyLayer);
        if (cols.Length == 0) return;

        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var c in cols)
        {
            float d = Vector2.Distance(transform.position, c.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = c.transform;
            }
        }

        if (nearest != null)
        {
            PlayerMove pm = GetComponent<PlayerMove>();
            // Ép Player nhìn về hướng con quái gần nhất
            if (pm != null) pm.FaceDirection(nearest.position - transform.position);
        }
    }

    private void ClearCombatTriggers()
    {
        if (anim == null) return;
        anim.ResetTrigger("IsAttackBase");
        anim.ResetTrigger("IsSkill1");
        anim.ResetTrigger("IsSkill2");
        anim.ResetTrigger("IsSkill3");
        anim.ResetTrigger("IsSkill4");
    }

    private bool IsPlayingHurt()
    {
        if (anim == null) return false;
        // Kiểm tra xem có đang ở chuỗi animation Hurt không
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Hurt") ||
               anim.GetNextAnimatorStateInfo(0).IsName("Hurt");
    }

    public void BtnBasic()
    {
        // Nếu đang đứng gần NPC Shop → mở shop thay vì đánh
        if (_nearbyShop != null)
        {
            _nearbyShop.OpenShop();
            return;
        }

        if (basicOnCD || isActionLocked || IsPlayingHurt()) return;
        FaceNearestEnemy();
        if (!combat.BasicAttack()) return;

        _audio?.PlayBasicAttack();
        ClearCombatTriggers();
        if (anim != null) anim.SetTrigger("IsAttackBase");

        StartCoroutine(LockAction());
        basicOnCD = true;
        StartCoroutine(Cooldown(
            basicFill,
            combat.basicCooldown,
            () => basicOnCD = false
        ));
    }

    public void BtnSkill1()
    {
        if (skill1OnCD || isActionLocked || IsPlayingHurt()) return;
        FaceNearestEnemy();
        if (!combat.Skill1()) return;

        _audio?.PlaySkill1();
        ClearCombatTriggers();
        if (anim != null) anim.SetTrigger("IsSkill1");

        StartCoroutine(LockAction());
        skill1OnCD = true;
        StartCoroutine(Cooldown(
            skill1Fill,
            combat.skill1Cooldown,
            () => skill1OnCD = false
        ));
    }

    public void BtnSkill2()
    {
        if (skill2OnCD || isActionLocked || IsPlayingHurt()) return;
        FaceNearestEnemy();
        if (!combat.Skill2()) return;

        _audio?.PlaySkill2();
        ClearCombatTriggers();
        if (anim != null) anim.SetTrigger("IsSkill2");

        StartCoroutine(LockAction());
        skill2OnCD = true;
        StartCoroutine(Cooldown(
            skill2Fill,
            combat.skill2Cooldown,
            () => skill2OnCD = false
        ));
    }

    public void BtnSkill3()
    {
        if (skill3OnCD || isActionLocked || IsPlayingHurt()) return;
        FaceNearestEnemy();
        if (!combat.Skill3()) return;

        _audio?.PlaySkill3();
        ClearCombatTriggers();
        if (anim != null) anim.SetTrigger("IsSkill3");

        StartCoroutine(LockAction());
        skill3OnCD = true;
        StartCoroutine(Cooldown(
            skill3Fill,
            combat.skill3Cooldown,
            () => skill3OnCD = false
        ));
    }

    public void BtnSkill4()
    {
        if (skill4OnCD || isActionLocked || IsPlayingHurt()) return;
        FaceNearestEnemy();
        if (!combat.Skill4()) return;

        _audio?.PlaySkill4();
        ClearCombatTriggers();
        if (anim != null) anim.SetTrigger("IsSkill4");

        StartCoroutine(LockAction());
        skill4OnCD = true;
        StartCoroutine(Cooldown(
            skill4Fill,
            combat.skill4Cooldown,
            () => skill4OnCD = false
        ));
    }

    IEnumerator LockAction()
    {
        isActionLocked = true;
        yield return new WaitForSeconds(actionLockTime);
        isActionLocked = false;
    }

    IEnumerator Cooldown(Image fill, float time, System.Action onDone)
    {
        fill.fillAmount = 0f;
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            fill.fillAmount = t / time;
            yield return null;
        }

        fill.fillAmount = 1f;
        onDone?.Invoke();
    }
}
