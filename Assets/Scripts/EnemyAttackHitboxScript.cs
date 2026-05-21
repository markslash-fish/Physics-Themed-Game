using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        // Using components in parent, but safely checking them later depending on hitBoxType
        enemyAI = GetComponentInParent<EnemyAI>();
        player = GetComponentInParent<Player>();
    }

    void Start()
    {
        if (hitBoxType == "Player")
        {
            targetTag = "Enemy";
            targetMask = enemyMask;
        }
        else if (hitBoxType == "Enemy")
        {
            targetTag = "Player";
            targetMask = playerMask;
        }
    }

    // FIX 1: Allow the Owner (Player 2 client, or Host enemy) to start the coroutine locally
    public void StartDamageWindow(float duration)
    {
        if (!IsOwner) return;

        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(DamageWindow(duration));
    }

    private IEnumerator DamageWindow(float duration)
    {
        ResetIgnoredList();
        float timer = 0f;

        while (timer < duration)
        {
            DealDamage();
            timer += Time.deltaTime;
            yield return null;
        }
    }

    // FIX 2: Let the Owner perform the Physics query for instant responsiveness 
    public void DealDamage()
    {
        if (!IsOwner) return;

        Vector3 spherePos = transform.TransformPoint(attackPointOffset);
        Collider[] hitEntities = Physics.OverlapSphere(spherePos, attackRange, targetMask, QueryTriggerInteraction.Ignore);

        foreach (Collider entity in hitEntities)
        {
            // We verify the object is part of the Netcode network simulation
            if (entity.TryGetComponent(out NetworkObject netObj))
            {
                if (!ignoredColliders.Contains(entity))
                {
                    int damage = (hitBoxType == "Player") ? player.playerDamage : enemyAI.enemyDamage;
                    Vector3 hitDir = (entity.transform.position - transform.parent.position).normalized;
                    hitDir.y = 0.15f;

                    // FIX 3: Route the data to the server instead of executing locally on Player 2's machine
                    RequestDamageServerRpc(netObj.NetworkObjectId, damage, hitDir);

                    ignoredColliders.Add(entity);
                    Debug.Log($"Hit registered locally by owner! Target NetID: {netObj.NetworkObjectId}");
                }
            }
        }
    }

    // FIX 4: Server RPC executes the actual damage logic globally across the server state
    [ServerRpc]
    private void RequestDamageServerRpc(ulong targetNetworkObjectId, int damage, Vector3 hitDir)
    {
        // Find the matching spawned object securely on the server context
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject targetNetObj))
        {
            if (targetNetObj.TryGetComponent(out IDamageable damageable))
            {
                // Apply health/shield logic on the server state
                damageable.TakeDamage(damage, hitDir);

                if (targetNetObj.CompareTag("Player"))
                {
                    Vector3 lookDir = -hitDir;
                    lookDir.y = 0f;

                    if (lookDir != Vector3.zero)
                    {
                        // Note: If you use ClientNetworkTransform on your player, 
                        // you will want to handle this rotation inside a ClientRpc instead!
                        targetNetObj.transform.rotation = Quaternion.LookRotation(lookDir);
                    }
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
        Vector3 spherePos = transform.TransformPoint(attackPointOffset);
        Gizmos.DrawWireSphere(spherePos, attackRange);
    }

    public void ResetHitbox()
    {
        ignoredColliders.Clear();
    }
}