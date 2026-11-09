using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum NavigationType
{
    Ground,
    Air,
}

[Serializable]
public class NavigationProfile
{
    [SerializeField] public NavigationType navigationType = NavigationType.Ground;
    [SerializeField] public int buildingDetectionDistance = 10;
    [SerializeField] public int radius = 1;
    [SerializeField] public float minSideDistance = 5;
    [SerializeField] public Team team;

    //ground properties
    [ShowIf("navigationType", NavigationType.Ground)]
    [SerializeField] public bool canWalkOnWater = true;
    [ShowIf("navigationType", NavigationType.Ground)]
    [SerializeField] public int climbStep = 1;
    [ShowIf("navigationType", NavigationType.Ground)]
    [SerializeField] public float climbCost = 1;
    [ShowIf("navigationType", NavigationType.Ground)]
    [SerializeField] public int fallStep = 2;
    [ShowIf("navigationType", NavigationType.Ground)]
    [SerializeField] public float fallCost = 1;

    //air properties
    [ShowIf("navigationType", NavigationType.Air)]
    [SerializeField] public int nominalHeight = 3;
    [ShowIf("navigationType", NavigationType.Air)]
    [SerializeField] public int minHeight = 1;
    [ShowIf("navigationType", NavigationType.Air)]
    [SerializeField] public int spreadDistance = 1;
    [ShowIf("navigationType", NavigationType.Air)]
    [SerializeField] public int maxStep = 2;
    [ShowIf("navigationType", NavigationType.Air)]
    [SerializeField] public float stepCost = 1;
}
