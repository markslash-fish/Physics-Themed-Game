using UnityEngine;

public class AnimationRouter : MonoBehaviour
{
    [SerializeField] private EnemyAttackHitboxScript leftHand;
    [SerializeField] private EnemyAttackHitboxScript rightHand;

    public void TriggerLeftHandDamage(float duration)
    {
        if (leftHand != null) leftHand.StartDamageWindow(duration);
    }

    public void TriggerRightHandDamage(float duration)
    {
        if (rightHand != null) rightHand.StartDamageWindow(duration);
    }
  public void clearLeftIgnoreList()
    {
        leftHand.ResetIgnoredList();
    }
    public void clearRightIgnoreList()
    {
        rightHand.ResetIgnoredList();
    }
}

