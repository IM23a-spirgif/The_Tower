using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public static Tower Instance;
    public GameObject bulletPrefab;
    public int damage = 1;
    public float fireRate = 1f;
    public float attackRange = 5f;
    private float nextFireTime;
    private GameObject currentTarget;
    public int extraBullets = 0;  // For "Shoot +1 bullet"
    public bool homingEnabled = false;
    public bool ricochetEnabled = false;
    public bool pierceEnabled = false;

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            RetargetAndFire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void RetargetAndFire()
    {
        currentTarget = GetNearestEnemy();
        if (currentTarget != null)
        {
            FireAtTarget(currentTarget);
        }
    }

    void FireAtTarget(GameObject target)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetDamage(damage);
        bullet.GetComponent<Bullet>().SetTarget(target);
    }

    GameObject GetNearestEnemy()
    {
        float minDist = float.MaxValue;
        GameObject nearestEnemy = null;
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist && dist <= attackRange)
            {
                minDist = dist;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }
    public void OnEnemyDestroyed()
    {
        RetargetAndFire();
    }
}