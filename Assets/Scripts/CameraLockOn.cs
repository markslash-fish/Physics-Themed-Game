using UnityEngine;
using Unity.Cinemachine;

public class CameraLockOn : MonoBehaviour
{
    [SerializeField] CinemachineCamera thirdPersonCam;
    [SerializeField] CinemachineCamera lockOnCam;
    [SerializeField] PlayerInputReader input;

    public Player playerScript;
    public Transform player;
    public Transform currentTarget;
    public Transform lockPoint; // Camera Lock-in

    public float lockRange = 15f;
    

    private bool isLocked;

    void Update()
    {
        if (isLocked && currentTarget != null)
        {
            UpdateLockPosition();
        }
    }

void FindTarget()
{
    Collider[] hits = Physics.OverlapSphere(player.position, lockRange);

    float closestDistance = Mathf.Infinity;
    Transform bestTarget = null;

    foreach (var hit in hits)
    {
        if (hit.CompareTag("Enemy"))
        {
            float dist = Vector3.Distance(player.position, hit.transform.position);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestTarget = hit.transform;
            }
        }
    }

    if (bestTarget != null)
    {
        currentTarget = bestTarget;
        isLocked = true;
        
    }
}

    void Unlock()
    {
        isLocked = false;
        currentTarget = null;
        

        lockOnCam.Priority = 5;
        thirdPersonCam.Priority = 20;
    }

    void UpdateLockPosition()
    {
        Vector3 midPoint = (player.position + currentTarget.position) / 2f;
        lockPoint.position = midPoint;
    }
        void ToggleLock()
    {
        if (!isLocked)
            FindTarget();
        else
            Unlock();
    }
    void OnEnable()
{
    input.onLockOn += ToggleLock;
}

void OnDisable()
{
    input.onLockOn -= ToggleLock;
}
}