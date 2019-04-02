using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : PathfindingAgent
{
    [Header("Unit Controller")]
    [SerializeField] internal int hitPoints = 1;
    [SerializeField] private int meleeDamage = 1;
    [SerializeField] private float attackDelay = 1;
    [SerializeField] private float attackAnimationDuration = 0;
    [SerializeField] private GameObject projectile = null;
    [SerializeField] private bool respawns = false;

    internal string team;
    internal string enemyTeam;
    internal int totalHitPoints;
    internal Vector3 spawnPosition;
    internal UnitController target;
    internal HealthBar healthBar;

    private new Collider collider;
    private Vector3 size;
    private const float meleeAttackRange = 1;
    private const float respawnDelay = 10;
    internal float nextAttack = 0;
    private bool isAttacking = false;

    protected override void Awake()
    {
        base.Awake();
        IdentifyTeam();
        totalHitPoints = hitPoints;
        collider = GetComponent<Collider>();
        size = collider.bounds.size;
    }

    protected override void Start()
    {
        base.Start();
        healthBar = HUD.instance.CreateHealthBar(this);
        spawnPosition = transform.position;
    }

    void OnDestroy()
    {
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }
    }

    // Inflict the given amount of damage to the unit.
    public void TakeDamage(int damage)
    {
        // Update hit points
        hitPoints -= damage;
        hitPoints = Mathf.Max(0, hitPoints);
        animator.SetFloat("health", hitPoints);
        
        // Death check
        if (hitPoints <= 0)
        {
            Disable();
            collider.enabled = false;
            if (!respawns)
            {
                Destroy(gameObject, 5);
            }
            else
            {
                StartCoroutine(Respawn(respawnDelay));
            }
        }
    }

    // Instantly kill the target.
    public void Kill()
    {
        TakeDamage(hitPoints);
    }

    // Heal the unit.
    public void Heal()
    {
        hitPoints = totalHitPoints;
        animator.SetFloat("health", hitPoints);
    }

    // Make the unit fire a projectile toward the target position.
    public void Fire(Vector3 position)
    {
        if (!activated || isAttacking) return;
        Face(position, FireForward);
    }

    // Make the unit attack the target enemy in melee range.
    public void Attack(UnitController target)
    {
        if (!activated || isAttacking) return;
        this.target = target;
        if (InMeleeAttackRange())
        {
            Stop();
            PerformAttack();
        }
        else
        {
            Chase(target.transform, MeleeAttackDistance(), PerformAttack);
        }
    }

    // Make the unit fire a projectile towards their forward direction.
    private void FireForward()
    {
        if (Time.time >= nextAttack)
        {
            isAttacking = true;
            nextAttack = Time.time + attackDelay;
            StartCoroutine(FireForwardCoroutine());
        }
    }

    // Damage the current target of the unit.
    // This assumes that the target is in melee range.
    private void PerformAttack()
    {
        if (Time.time >= nextAttack)
        {
            isAttacking = true;
            nextAttack = Time.time + attackDelay;
            StartCoroutine(PerformAttackCoroutine());
        }
        else
        {
            target = null;
        }
    }

    // Coroutine which adds a delay to the range attack to simualte the wind up.
    private IEnumerator FireForwardCoroutine()
    {
        animator.Play("Attack");
        yield return new WaitForSeconds(attackAnimationDuration);
        var offset = (Vector3.up * size.y * 0.5f) + (transform.forward * size.z / 2);
        var instance = Instantiate(projectile, transform.position + offset, transform.rotation);
        instance.GetComponent<Projectile>().target = enemyTeam;
        isAttacking = false;
    }

    // Coroutine which adds a delay to the melee attack to simulate the wind up.
    private IEnumerator PerformAttackCoroutine()
    {
        animator.Play("Attack");
        yield return new WaitForSeconds(attackAnimationDuration);
        if (target != null)
        {
            target.TakeDamage(meleeDamage);
            target = null;
        }
        isAttacking = false;
    }

    // Make the unit respawn after the given delay.
    private IEnumerator Respawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.position = spawnPosition;
        hitPoints = totalHitPoints;
        animator.SetFloat("health", hitPoints);
        collider.enabled = true;
        activated = true;
    }

    private float MeleeAttackDistance()
    {
        return size.z / 2 + target.size.z / 2 + meleeAttackRange;
    }

    // Check if the unit is in melee attack range of its target.
    private bool InMeleeAttackRange()
    {
        var distance = Vector3.Distance(transform.position, target.transform.position);
        return distance <= MeleeAttackDistance();
    }

    // Return the team that the unit belongs to by looking at the tag.
    private void IdentifyTeam()
    {
        if (tag.StartsWith("blue"))
        {
            team = "blue";
            enemyTeam = "red";
        }
        else if (tag.StartsWith("red"))
        {
            team = "red";
            enemyTeam = "blue";
        }
        else
        {
            Debug.LogError("Invalid team tag on current unit");
        }
    }
}
