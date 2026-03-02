using UnityEngine;

public class AttackVisualizer : MonoBehaviour
{
    private void OnEnable()
    {
        AttackRandomizer.OnWeightChange += HandleWeight;
    }
    private void OnDisable()
    {
        AttackRandomizer.OnWeightChange -= HandleWeight;
    }

    void HandleWeight(float newWeight)
    {

    }

}
