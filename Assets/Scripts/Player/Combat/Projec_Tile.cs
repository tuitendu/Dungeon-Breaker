using UnityEngine;

public enum ProjectileType
{
    Normal,
    Pierce,
    Rain,
    Ultimate
}

public class Projec_Tile : MonoBehaviour
{
    public float speed = 10f;

    private int damage;
    private Vector2 direction;
    private ProjectileType type;

    // Init đầy đủ
    public void Init(Vector2 dir, int dmg, ProjectileType t)
    {
        direction = dir.normalized;
        damage = dmg;
        type = t;

        Destroy(gameObject, 3f);
    }

    public void Init(Vector2 dir, int dmg)
    {
        Init(dir, dmg, ProjectileType.Normal);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyStats enemy = other.GetComponent<EnemyStats>();
        if (enemy != null)
            enemy.TakeDamage(damage);

        // sau này xử lý xuyên / rain tại đây
        if (type != ProjectileType.Pierce)
            Destroy(gameObject);
    }
}
