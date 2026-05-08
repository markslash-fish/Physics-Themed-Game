using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCamLockOn : NetworkBehaviour
{
    [Header("References")]
    public CinemachineTargetGroup targetGroup;
    public Animator cameraStateAnimator;

    [Header("Settings")]
    public float detectionRadius = 15f;
    public LayerMask enemyLayer;

    public Transform currentEnemy;

    void Update()
    {
        if (!IsOwner) return;

        // Logic: If we have a reference, we MUST validate it
        if (currentEnemy != null)
        {
            bool isEnemyGone = false;

            // Check if the reference itself became null/destroyed
            if (currentEnemy == null)
            {
                isEnemyGone = true;
            }
            else
            {
                // Check if the object is disabled or about to be destroyed
                if (!currentEnemy.gameObject.activeInHierarchy)
                {
                    isEnemyGone = true;
                }
            }

            if (isEnemyGone)
            {
                Debug.Log("Enemy lost or despawned. Resetting camera.");
                ClearLockOn();
                return;
            }

            // Standard Distance Check
            float dist = Vector3.Distance(transform.position, currentEnemy.position);
            if (dist > detectionRadius + 2f)
            {
                ClearLockOn();
            }
        }
    }

    public void ToggleLockOn()
    {
        if (currentEnemy != null)
        {
            ClearLockOn();
            return;
        }

        AttemptLockOn();
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
            // Add the enemy to the Cinemachine Target Group
            targetGroup.AddMember(currentEnemy, 1f, 2f);

            // Switch the State-Driven Camera to the LockOn state
            cameraStateAnimator.SetBool("IsLockedOn", true);
        }
    }

    public void ClearLockOn()
    {
        // Ensure we tell the animator to switch back FIRST
        if (cameraStateAnimator != null)
        {
            cameraStateAnimator.SetBool("IsLockedOn", false);
            // Force the animator to update this frame
            cameraStateAnimator.Update(0);
        }

        if (currentEnemy != null)
        {
            targetGroup.RemoveMember(currentEnemy);
            currentEnemy = null;
        }

        Debug.Log("Lock On Cleared.");
    }
}