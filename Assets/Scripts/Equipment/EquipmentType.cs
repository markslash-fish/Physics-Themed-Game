using UnityEngine;

public enum EquipmentType
{
    Helmet,
    Armor,
    Weapon,
    Boots
}

public class EquipmentItem : MonoBehaviour
{
    public EquipmentType type;

    public GameObject prefabLeft;
    public GameObject prefabRight;

     [Header("Stat Bonus")]
    public float speedBonus;
    public float jumpBonus;
}