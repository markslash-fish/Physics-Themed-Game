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
        Ray ray = new Ray(player.position + Vector3.up, player.forward);
        RaycastHit hit;

        float radius = 2f;

        if (Physics.SphereCast(ray, radius, out hit, lockRange))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                currentTarget = hit.collider.transform;
                isLocked = true;

                playerScript.target = currentTarget;

                Debug.Log("Locked on: " + currentTarget.name);
            }
        }


        
    }

    void Unlock()
    {
        isLocked = false;
        currentTarget = null;
        playerScript.target = null;

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