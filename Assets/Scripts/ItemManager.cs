using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "Equipment")]
public class ItemManager : ScriptableObject
{
    public string itemName;
    public GameObject itemPrefab1 = null;
    public GameObject itemPrefab2 = null;
    public EquipmentType equipmentType;
    public string animTrigger;
    public float skillCooldown;
    public int baseDamage;

    [Header("Stat Bonus")]
    public int attackBonus;
    public int defenseBonus;
    public int healthBonus;
    public int staminaBonus;
    public int speedBonus;
    public float senseBonus;

    
    public enum EquipmentType
    {
        Monocle,
        Armor,
        Weapon,
        Boots
    }
}



