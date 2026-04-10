using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "Equipment")]
public class ItemManager : ScriptableObject
{
    public GameObject itemPrefab;
    public string itemName;
    public float skillCooldown;
    public string animTrigger;
    public int baseDamage;
}
