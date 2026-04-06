using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public Transform headSlot;
    public Transform armorSlot;
    public Transform weaponSlot;
    public Transform leftBootSlot;
    public Transform rightBootSlot;

    GameObject currentHelmet;
    GameObject currentArmor;
    GameObject currentWeapon;
    GameObject currentBootL;
    GameObject currentBootR;

    public void EquipItem(EquipmentItem item)
    {
        Debug.Log("ITEM TYPE: " + item.type);
        switch(item.type)
        {
            case EquipmentType.Helmet:
                if(currentHelmet) Destroy(currentHelmet);
                currentHelmet = Instantiate(item.prefabLeft, headSlot);
                break;

            case EquipmentType.Armor:
                Debug.Log("EQUIPPING ARMOR");
                if(currentArmor) Destroy(currentArmor);
                currentArmor = Instantiate(item.prefabLeft, armorSlot);

                currentArmor.transform.localPosition = Vector3.zero;
                currentArmor.transform.localRotation = Quaternion.identity;
                break;

            case EquipmentType.Weapon:
                if(currentWeapon) Destroy(currentWeapon);
                currentWeapon = Instantiate(item.prefabLeft, weaponSlot);
                break;

            case EquipmentType.Boots:
             Debug.Log("EQUIPPING BOOTS");
                if(currentBootL) Destroy(currentBootL);
                if(currentBootR) Destroy(currentBootR);

                currentBootL = Instantiate(item.prefabLeft, leftBootSlot);
                currentBootR = Instantiate(item.prefabRight, rightBootSlot);

                Player player = FindObjectOfType<Player>();
                player.AddBootStats(item.speedBonus, item.jumpBonus);

                break;
        }
    }
}