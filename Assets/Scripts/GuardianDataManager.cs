using UnityEngine;

[CreateAssetMenu(fileName = "GuardianData", menuName = "Guardian Data")]
public class GuardianDataManager : ScriptableObject
{
   
    public enum PoiseLevel
    {
        low,
        medium,
        high
    }

    [Header("Poise Level")]
    public PoiseLevel poise;

    [Header("Enemy Name")]
    public string enemyName;

    [Header("Enemy Stats")]
    public float enemyHealth;
    public float enemyStamina;
    public float enemyDefense;

    [Header("Enemy Combat")]
    public float enemyMinAttackPower;
    public float enemyMaxAttackPower;
    

    public int enemyID;
    public string enemyKey;
    
   

    
}
