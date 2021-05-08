using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UI;
using System;

[CreateAssetMenu(fileName ="UIDatabase", menuName = "UIDatabase", order =2)]
public class UIDatabase : ScriptableObject
{
    [SerializeField] public List<GameObject> PanelObject;
    //[SerializeField] public List<GameObject> PopupObject;
    [SerializeField] public List<SpriteAtlas> spriteAtlas = new List<SpriteAtlas>();
    public List<RarityColor> rarityColors;
    public List<RarityColor> marbleStatRarityColors;
    public List<RarityColor> shopRarityColors;
    public List<RarityColor> shopRarityColors_Deco;
    [Space(20)]
    [Header("AssetEffect")]
    public float startSpeed = 5f;
    public float accSpeed = 1.2f;
    public float effSize = 200f;
    public float accStartWaitTime = 0.2f;
    [Space(20)]
    [Header("BoxOpenSprite")]
    public SpriteAtlas EffectSprite;
    [Space(20)]
    [Header("MarbleRarityColor")]
    public List<MarbleRarityColorInfo> marbleRarityColorInfos;
    public RarityColor GetRarityColor(MarbleRarity rarity)
    {
        RarityColor rc = null;
        for (int i = 0; i < rarityColors.Count; i++)
        {
            if (rarityColors[i].rarity == rarity)
            {
                rc = rarityColors[i];
                break;
            }
        }
        return rc;
    }
    public RarityColor GetMarbleStatRarityColor(MarbleRarity rarity)
    {
        RarityColor rc = null;
        for (int i = 0; i < marbleStatRarityColors.Count; i++)
        {
            if (marbleStatRarityColors[i].rarity == rarity)
            {
                rc = marbleStatRarityColors[i];
                break;
            }
        }
        return rc;
    }
    public RarityColor GetShopRarityColor(MarbleRarity rarity)
    {
        RarityColor rc = null;
        for (int i = 0; i < shopRarityColors.Count; i++)
        {
            if (shopRarityColors[i].rarity == rarity)
            {
                rc = shopRarityColors[i];
                break;
            }
        }
        return rc;
    }
    public RarityColor GetShopRarityColorDeco(MarbleRarity rarity)
    {
        RarityColor rc = null;
        for (int i = 0; i < shopRarityColors_Deco.Count; i++)
        {
            if (shopRarityColors_Deco[i].rarity == rarity)
            {
                rc = shopRarityColors_Deco[i];
                break;
            }
        }
        return rc;
    }

    public MarbleRarityColorInfo GetMarbleRarityColorInfo(MarbleRarity rarity)
    {
        MarbleRarityColorInfo rc = null;
        for (int i = 0; i < marbleRarityColorInfos.Count; i++)
        {
            if (marbleRarityColorInfos[i].rarity == rarity)
            {
                rc = marbleRarityColorInfos[i];
                break;
            }
        }
        return rc;
    }
}
[Serializable]
public class RarityColor
{
    public MarbleRarity rarity;
    public Color topColor;
    public Color BottomColor;
}
[Serializable]
public class MarbleRarityColorInfo
{
    public MarbleRarity rarity;
    public Color DecoColor;
    public Color BGColor;
    public Color EffColor;
    public Color LineColor;
    public Color RarityTextColor;
}