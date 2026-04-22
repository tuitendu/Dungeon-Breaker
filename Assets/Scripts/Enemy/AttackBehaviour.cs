using UnityEngine;

public abstract class AttackBehaviour : ScriptableObject
{
    [Header("Cài đặt chung")]
    public string animTrigger = "Attack"; // Tên Trigger trong Animator


    public abstract void ExecuteAttack(GameObject attacker, GameObject target);
}