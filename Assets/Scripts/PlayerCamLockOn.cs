using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCamLockOn : NetworkBehaviour
{
    public Transform currentTarget;
    public PlayerInputReader inputReader;
    public CinemachineCamera playerLockOnCam;
    public CinemachineCamera playerThirdPersonCam;
    [Header("Cam Lock On")]
    public float camDetectionRange;
    public LayerMask targetLayer;
    Vector3 detectionOffset;
    public override void OnNetworkSpawn()
    {
        inputReader.onLockOn += ToggleLockOn;
    }
    private void Awake()
    {
    
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void ToggleLockOn()
    {
        // If already locked, clear it (Toggle behavior)
        if (playerLockOnCam.Priority > playerThirdPersonCam.Priority)
        {
            ClearLockOn();
            return;
        }

        Vector3 spherePos = transform.TransformPoint(detectionOffset);
        Collider[] nearbyEnemies = Physics.OverlapSphere(spherePos, camDetectionRange, targetLayer);

        Transform bestTarget = null;
        float closestToCenter = -1f; // Dot product range is -1 to 1

        foreach (var collider in nearbyEnemies)
        {
            Vector3 directionToEnemy = (collider.transform.position - transform.position).normalized;

            // 1. Check if the enemy is in front of us
            float dot = Vector3.Dot(transform.forward, directionToEnemy);

            // 2. Line of Sight Check (Don't lock through walls!)
            if (dot > 0.5f) // Roughly a 60-degree cone
            {
                if (Physics.Linecast(transform.position + Vector3.up, collider.transform.position + Vector3.up, out RaycastHit hit))
                {
                    if (hit.transform != collider.transform) continue; // Something is in the way
                }

                // 3. Keep track of the enemy closest to the center of our gaze
                if (dot > closestToCenter)
                {
                    closestToCenter = dot;
                    bestTarget = collider.transform;
                }
            }
        }

        if (bestTarget != null)
        {
            ActivateLockOn(bestTarget);
        }
    }
    private void ActivateLockOn(Transform target)
    {
        playerLockOnCam.Target.LookAtTarget = target;
        playerLockOnCam.Priority = 100;
        playerThirdPersonCam.Priority = 90;
    }

    private void ClearLockOn()
    {
        playerLockOnCam.Priority = 90;
        playerThirdPersonCam.Priority = 100;
        playerLockOnCam.Target.TrackingTarget = null;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 spherePos = transform.TransformPoint(detectionOffset);
        Gizmos.DrawWireSphere(spherePos, camDetectionRange);
    }
}
