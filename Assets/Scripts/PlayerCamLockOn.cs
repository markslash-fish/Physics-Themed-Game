using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCamLockOn : NetworkBehaviour
{
    [Header("References")]
    public CinemachineTargetGroup targetGroup;
    public Animator cameraStateAnimator; // To swap between 3rd Person & LockOn
  

    [Header("Settings")]
    public float detectionRadius = 15f;
    public LayerMask enemyLayer;

    public Transform currentEnemy;

    void Update()
    {

        if (!IsOwner || currentEnemy == null) return;

        float dist = Vector3.Distance(transform.position, currentEnemy.position);
        if (dist > detectionRadius + 2f) // Buffer of 2m to prevent flickering
        {
            ClearLockOn();
        }

    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
      
    }
    public void ToggleLockOn()
    {
      
        if (currentEnemy != null)
        {
            ClearLockOn();
            return;
        }

        AttemptLockOn();
        Debug.Log("Locked");
    }

    void AttemptLockOn()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        float closestDist = Mathf.Infinity;
        Transform bestTarget = null;

        if (enemies.Length == 0) return; 

        foreach (var col in enemies)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                bestTarget = col.transform;
            }
        }

        if (bestTarget != null)
        {
            currentEnemy = bestTarget;
            targetGroup.AddMember(currentEnemy, 1f, 2f);

         
            cameraStateAnimator.SetBool("IsLockedOn", true);

          
        }
    }

    public void ClearLockOn()
    {
        if (currentEnemy != null)
        {
            targetGroup.RemoveMember(currentEnemy);
            currentEnemy = null;
        }
        cameraStateAnimator.SetBool("IsLockedOn", false);
    }
}
