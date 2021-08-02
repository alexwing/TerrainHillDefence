using System.Collections.Generic;
using UnityEngine;

public static class SceneConfig
{
    public static bool Debug = false;

    public static class SOLDIER
    {
    public readonly static float AttackRange = 150f;
    public readonly static float AttackRamdomRange = 30f;


    public readonly static float SoldierVelocity = 3f;

    public readonly static int SoldierFrameRate = 25;
    public readonly static int FindEnemyFrameRate = 2;
    }

}
