using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public Transform MonocleSlot;
    public Transform armorSlot;
    public Transform leftGauntletSlot;
    public Transform rightGauntletSlot;
    public Transform leftBootSlot;
    public Transform rightBootSlot;

    GameObject currentMonocle;
    GameObject currentArmor;
    GameObject currentGauntletL;
    GameObject currentGauntletR;
    GameObject currentBootL;
    GameObject currentBootR;

    Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();
    }

    public void EquipItem(EquipmentItem item)
    {
        Debug.Log("ITEM TYPE: " + item.type);
        switch(item.type)
        {
            case EquipmentType.Monocle:
                if(currentMonocle) Destroy(currentMonocle);
                currentMonocle = Instantiate(item.prefabLeft, MonocleSlot);

                currentMonocle.transform.localPosition = Vector3.zero;
                currentMonocle.transform.localRotation = Quaternion.identity;

                player.AddMonocleStats(item.cooldownReduction);
                break;

            case EquipmentType.Armor:
                Debug.Log("EQUIPPING ARMOR");
                if(currentArmor) Destroy(currentArmor);
                currentArmor = Instantiate(item.prefabLeft, armorSlot);

                currentArmor.transform.localPosition = Vector3.zero;
                currentArmor.transform.localRotation = Quaternion.identity;

                player.AddArmorStats(item.damageReduction);
                break;

            case EquipmentType.Weapon:

                Debug.Log("EQUIPPING GAUNTLETS");

                if(currentGauntletL) Destroy(currentGauntletL);
                if(currentGauntletR) Destroy(currentGauntletR);

                currentGauntletL = Instantiate(item.prefabLeft, leftGauntletSlot);
                currentGauntletR = Instantiate(item.prefabRight, rightGauntletSlot);

                currentGauntletL.transform.localPosition = Vector3.zero;
                currentGauntletL.transform.localRotation = Quaternion.identity;

                currentGauntletR.transform.localPosition = Vector3.zero;
                currentGauntletR.transform.localRotation = Quaternion.identity;


                player.AddWeaponStats(item.heavyDamageBonus);

            break;
            
            case EquipmentType.Boots:
             Debug.Log("EQUIPPING BOOTS");
                if(currentBootL) Destroy(currentBootL);
                if(currentBootR) Destroy(currentBootR);

                currentBootL = Instantiate(item.prefabLeft, leftBootSlot);
                currentBootR = Instantiate(item.prefabRight, rightBootSlot);

                currentBootL.transform.localPosition = Vector3.zero;
                currentBootL.transform.localRotation = Quaternion.identity;

                currentBootR.transform.localPosition = Vector3.zero;
                currentBootR.transform.localRotation = Quaternion.identity;


                player.AddBootStats(item.speedBonus, item.jumpBonus);

                break;
        }
    }
}