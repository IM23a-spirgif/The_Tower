using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 1.5f;
    private Transform tower;
    private EnemyHealth enemyHealth;

    void Start()
    {
        GameObject towerObject = GameObject.FindGameObjectWithTag("Tower");

        if (towerObject != null)
        {
            tower = towerObject.transform;
        }
        enemyHealth = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (tower == null || enemyHealth == null) return;
        transform.position = Vector2.MoveTowards(transform.position, tower.position, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, tower.position) < 0.5f)
        {
            int damageDealt = enemyHealth.GetCurrentHealth();
            tower.GetComponent<TowerHealth>().TakeDamage(damageDealt);
            enemyHealth.NotifySpawner();
            Destroy(gameObject);
        }
    }
}