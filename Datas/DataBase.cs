using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "DataBase", menuName = "StarterKit/Data Base")]
public class DataBase : ScriptableObject
{
    public bool isTutorialPassed;
    
    #region Player
    [Serializable]
    public class Player
    {
        public float speed;

        public int prisonerStackCount;
        public int prisonerStackCountMax;
        public int objectStackCount;
        public int objectStackCountMax;
        public List<int> upgradeStackCosts = new List<int>();
    }

    public Player player;
    #endregion
    
    #region GuardData
    [Serializable]
    public class GuardData
    {
        [Header("Guard Count Settings")]
        //
        [Tooltip("Current Guard Count Info for Spawining Guard")]
        public int guardCountCurrent;
        [Tooltip("Max Available Guard Count")]
        public int guardCountMax;
        public List<int> guarBuyCosts = new List<int>();

        [Header("Stack Count Settings")]
        //
        [Tooltip("Current Max Stack Count")]
        public int objectStackCountCurrent;
        [Tooltip("Max Available Stack Count")]
        public int objectStackCountMax;
        
        [Header("Prisoner Count Settings")]
        //
        [Tooltip("Current Max Prisoner Tail Count ")]
        public int capacityPrisonerCurrent;
        [Tooltip("Max Available Prisoner Tail Count")]
        public int capacityPrisonerMax;
        
        [Header("Speed")]
        public float speed;
        
        
        [Serializable]
        public class AnimationParameters
        {
            public string walking;
            public string idle;
        }

        public AnimationParameters anim;

    }
    public GuardData guardData;
    #endregion

    #region Prisoner Data
    [Serializable]
    public class PrisonerData
    {
        public float defaultSpeed;
        public float punishmentTimeDefault;
        public float punishmentTimeMax;
        public float addPunishmentChance;

        [Serializable]
        public class AnimationParameters
        {
            public string Walk;
            public string Idle;
            public string Sit;
            public string Yard;
            public string Cell;
            public string Blend;
        }

        public AnimationParameters animParam;
    }

    public PrisonerData prisonerData;
    #endregion

    #region ResourceDatas
    [Serializable]
    public class ResourceData
    {
        public float creationTime;
        public int maxStackCount;
    }

    public ResourceData resourceLoundry;
    public ResourceData resourceFood;
    #endregion

    #region CafeteriaData
    [Serializable]
    public class CafeteriaData
    {
        public bool isCafeteriaActive;
        public int cafeteriaCost;
        public int defaultCafeteriaCost;
        public List<int> upgradeCost;
        public List<int> defaultUpgradeCost;
        public List<int> infoList;
    }

    public CafeteriaData cafeteriaData;
    #endregion

    #region Yard Data
    [Serializable]
    public class YardData
    {
        public bool isYardActive;
        public int yardCost;
        public int defaultYardCost;
        public List<int> upgradeCost;
        public List<int> defaultUpgradeCost;
        public List<int> infoList;
    }
    public YardData yardData;
    #endregion

    #region CellDatas
    [Serializable]
    public class CellData
    {
        public List<int> cellInfos = new List<int>();
        public List<int> cellCosts = new List<int>();
        public List<int> cellCostDefault = new List<int>();

        [Serializable]
        public class AnimationParameters
        {
            public string doorOpen;
            public string doorClose;
        }

        public AnimationParameters animParam;
    }

    public CellData cellData;
    #endregion
    
    #region Area3
    [Serializable]
    public class Area3
    {
        public bool isActive;
        public int cost;
        public int defaultCost;
        public List<int> infos;
        public List<int> upgradeCosts;
        public List<int> defaultUpgradesCosts;
    }

    public Area3 area3;
    #endregion

    #region Tags
    [Serializable]
    public class Tags
    {
        public string cell;
        public string player;
        public string guard;
        public string prisoner;
        public string resouceField;
        public string cellInside;
        public string resource;
        public string yard;
        public string cafeteria;
        public string buyField;
        public string foodDeliveryPoint;
        public string money;
        public string hr;
        public string triggerField;
    }

    public Tags tags;
    #endregion
}
