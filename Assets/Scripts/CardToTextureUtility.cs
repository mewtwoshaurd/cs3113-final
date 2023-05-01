using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct UnitToMat
{
    public UnitType unitType;
    public Material material;
    public Texture texture;
}

[System.Serializable]
public struct ItemToMat
{
    public ItemType itemType;
    public Material material;
    public Texture texture;
}

public class CardToTextureUtility : MonoBehaviour
{
    [SerializeField] List<UnitToMat> unitToMatList;
    [SerializeField] List<ItemToMat> itemToMatList;

    public Material getUnitMat(UnitType unitType)
    {
        foreach (UnitToMat unitToMat in unitToMatList)
        {
            if (unitToMat.unitType == unitType)
            {
                return unitToMat.material;
            }
        }
        return null;
    }

    public Material getItemMat(ItemType itemType)
    {
        foreach (ItemToMat itemToMat in itemToMatList)
        {
            if (itemToMat.itemType == itemType)
            {
                return itemToMat.material;
            }
        }
        return null;
    }

    public Texture getUnitTexture(UnitType unitType)
    {
        foreach (UnitToMat unitToMat in unitToMatList)
        {
            if (unitToMat.unitType == unitType)
            {
                return unitToMat.texture;
            }
        }
        return null;
    }

    public Texture getItemTexture(ItemType itemType)
    {
        foreach (ItemToMat itemToMat in itemToMatList)
        {
            if (itemToMat.itemType == itemType)
            {
                return itemToMat.texture;
            }
        }
        return null;
    }
}
