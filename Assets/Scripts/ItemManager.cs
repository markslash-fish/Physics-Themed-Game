using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "Equipment")]
public class ItemManager : ScriptableObject
{
    public GameObject itemPrefab1 = null;
    public GameObject itemPrefab2 = null;
    public EquipmentType equipmentType;
    public string itemName;
    public float skillCooldown;
    public string animTrigger;
    public int baseDamage;

    public enum EquipmentType
    {
        Monocle,
        Armor,
        Weapon,
        Boots
    }
}



