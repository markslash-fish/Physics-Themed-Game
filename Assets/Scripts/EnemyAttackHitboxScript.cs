using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class EnemyAttackHitboxScript : NetworkBehaviour
{
    public List<Collider> ignoredColliders = new List<Collider>();
    [SerializeField] EnemyAI enemyAI = null;
    [SerializeField] Player player = null;
    [SerializeField] private Vector3 attackPointOffset;
    [SerializeField] private float attackRange;
    [SerializeField] private string targetTag, hitBoxType;
    [SerializeField] private LayerMask targetMask, enemyMask, playerMask;

    private Coroutine attackRoutine;
    private void OnEnable()
    {
      
    }
    private void OnDisable()
    {
        
    }
    private void Awake()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
        player = GetComponentInParent<Player>();
    }
   

    void Start()
    {
        if(hitBoxType == "Player")
        {
            targetTag = "Enemy";
            targetMask = enemyMask;

        }
        else if(hitBoxType == "Enemy")
        {
            targetTag = "Player";
            targetMask = playerMask;
        }
    }

    public void StartDamageWindow(float duration)
    {
        if (!IsServer) return;
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(DamageWindow(duration));
    }
   
    void Update()
    {
        
    }
   
    private IEnumerator DamageWindow(float duration)
    {
        ResetIgnoredList();
        float timer = 0f;

        while( timer < duration)
        {
            DealDamage();
            timer += Time.deltaTime;
            yield return null;
        }
        attackRoutine = null;
    }
    public void DealDamage()
    {
        if (!IsServer) return;

        Vector3 spherePos = transform.TransformPoint(attackPointOffset);
        Collider[] hitEntities = Physics.OverlapSphere(spherePos, attackRange, targetMask);
          


            foreach (Collider entity in hitEntities)
            {
                if (entity.TryGetComponent(out IDamageable damageable))
                {
                   if(!ignoredColliders.Contains(entity))
                   {
                    int damage = (hitBoxType == "Player") ? player.playerDamage : enemyAI.enemyDamage;
                    Vector3 hitDir = (entity.transform.position - transform.parent.position).normalized;
                    hitDir.y = 0.15f;
            DamageServerRpc(
                entity.GetComponent<NetworkObject>().NetworkObjectId,
                damage,
                hitDir
            );
                        ignoredColliders.Add(entity);

                    Debug.Log("AAAAAAAAAAAAA");
                   }
               
                
                }
            }


        
     }
 
    public void ResetIgnoredList()
    {
        ignoredColliders.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
public void ResetHitbox()
{
    ignoredColliders.Clear();
}
[ServerRpc(RequireOwnership = false)]
void DamageServerRpc(ulong targetId, int damage, Vector3 hitDir)
{
    if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject obj))
    {
        if (obj.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damage, hitDir);
        }
    }
}

}
