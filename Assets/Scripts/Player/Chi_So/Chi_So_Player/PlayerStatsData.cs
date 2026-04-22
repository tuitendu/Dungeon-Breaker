using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStats",
    menuName = "RPG/Player Stats Data")]
public class PlayerStatsData : ScriptableObject
{
    public string className;

    [Header("Core")]
    public int maxHP;
    public int maxMP;
    public int maxSTA;

    [Header("Attack")]
    public int ATK;
    public int MATK;
    public float range;

    [Header("Defense")]
    public int DEF;
    public int MDEF;

    [Header("Other")]
    public float speed;
    [Range(0f, 1f)]
    public float critRate;
}
