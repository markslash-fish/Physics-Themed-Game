using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class EquipmentManager : NetworkBehaviour
{

    public List<ItemManager> equippedItems = new List<ItemManager>();
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
        
    }

    public void EquipItem(ItemManager item)
    {
        Debug.Log("ITEM TYPE: " + item.equipmentType);
        switch (item.equipmentType)
        {
            case ItemManager.EquipmentType.Monocle:
               
              currentMonocle = Instantiate(item.itemPrefab1, MonocleSlot);

                currentMonocle.transform.localPosition = Vector3.zero;
                currentMonocle.transform.localRotation = Quaternion.identity;

               
                break;

            case ItemManager.EquipmentType.Armor:
                Debug.Log("EQUIPPING ARMOR");

                currentArmor = Instantiate(item.itemPrefab1, armorSlot);

                 currentArmor.transform.localPosition = Vector3.zero;
                 currentArmor.transform.localRotation = Quaternion.identity;

              
                break;

            case ItemManager.EquipmentType.Weapon:

                Debug.Log("EQUIPPING GAUNTLETS");

  
                
                
                    currentGauntletL = Instantiate(item.itemPrefab1, leftGauntletSlot);
                    currentGauntletR = Instantiate(item.itemPrefab2, rightGauntletSlot);

                



                currentGauntletL.transform.localPosition = Vector3.zero;
                currentGauntletL.transform.localRotation = Quaternion.identity;
                
                currentGauntletR.transform.localPosition = Vector3.zero;
                currentGauntletR.transform.localRotation = Quaternion.identity;


               

            break;
            
            case ItemManager.EquipmentType.Boots:
             Debug.Log("EQUIPPING BOOTS");
  
               
                
                    currentBootL = Instantiate(item.itemPrefab1, leftBootSlot);
                    currentBootR = Instantiate(item.itemPrefab2, rightBootSlot);
                
                    
 
                currentBootL.transform.localPosition = Vector3.zero;
                currentBootL.transform.localRotation = Quaternion.identity;

                currentBootR.transform.localPosition = Vector3.zero;
                currentBootR.transform.localRotation = Quaternion.identity;


                

                break;
        }
    }
}