using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] Shotgun shotgunScript;
    [SerializeField] Pistol pistolScript;
    private List<Gun> weapons = new List<Gun>();
    private int inventoryIndex = 0;
    
    void Start()
    {
        weapons.Add(shotgunScript);
        weapons.Add(pistolScript);
    }

    //Update for swapping weapons and turning on and turning off scripts
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Current weapon:" + weapons[inventoryIndex].name);
            weapons[inventoryIndex].enabled = false;
            if (inventoryIndex + 1 <  weapons.Count)
            {
                inventoryIndex++;
                weapons[inventoryIndex].enabled = true;
                Debug.Log("Swapping to:" + weapons[inventoryIndex].name);
            }
            else
            {
                inventoryIndex = 0;
                weapons[inventoryIndex].enabled = true;
                Debug.Log("Swapping to:" + weapons[inventoryIndex].name);
            }

        }
    }
}
