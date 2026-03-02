using System;
using System.Collections;
using UnityEngine;

public class AttackRandomizer : MonoBehaviour
{
    [Header("Settings")]
    public float minWeightValue = 10.0f;
    public float maxWeightValue = 100.0f;
    public float weightValue;

    public static event Action<float> OnWeightChange;

    private void Start()
    {
        // Start the automated "dice roll" loop
        StartCoroutine(RandomizeWeightValue());
    }

    IEnumerator RandomizeWeightValue()
    {
        // Wait 3 seconds before the first run (initial delay)
        yield return new WaitForSeconds(3f);

        while (true)
        {
            UpdateWeightValue();

            // Wait 4 seconds before the next roll
            yield return new WaitForSeconds(4f);
        }
    }

    void UpdateWeightValue()
    {
        
        weightValue = UnityEngine.Random.Range(minWeightValue, maxWeightValue);
        OnWeightChange?.Invoke(weightValue);
        Debug.Log("New Weight Value: " + weightValue);
    }
}