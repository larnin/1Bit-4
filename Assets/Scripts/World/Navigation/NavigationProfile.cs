using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class NavigationProfile
{
    [SerializeField] public int buildingDetectionDistance = 10;
    [SerializeField] public bool canWalkOnWater = true;
    [SerializeField] public int radius = 1;
    [SerializeField] public int climbStep = 1;
    [SerializeField] public float climbCost = 1;
    [SerializeField] public int fallStep = 2;
    [SerializeField] public float fallCost = 1;
    [SerializeField] public float minSideDistance = 5;
    [SerializeField] public Team team;
}
