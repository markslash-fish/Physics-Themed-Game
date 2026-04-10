using UnityEngine;

public enum EquipmentType
{
    Monocle,
    Armor,
    Weapon,
    Boots
}

public class EquipmentItem : MonoBehaviour
{
    public EquipmentType type;

    public GameObject prefabLeft;
    public GameObject prefabRight;

    [Header("Boot Stats")]
    public float speedBonus;
    public float jumpBonus;

    [Header("Weapon Stats")]
    public float heavyDamageBonus;

    [Header("Armor Stats")]
    public float damageReduction;

    [Header("Monocle Stats")]
    public float cooldownReduction;
}