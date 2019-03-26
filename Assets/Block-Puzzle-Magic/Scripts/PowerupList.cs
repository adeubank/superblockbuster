using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerupList", menuName = "BlockPuzzle/PowerupList", order = 2)]
public class PowerupList : ScriptableObject
{
    public List<PowerupBlockSpawn> powerupBlockSpawns;
}

[Serializable]
public class PowerupBlockSpawn : ShapeBlockSpawn
{
    public GameObject powerupActivationSprite;
    public GameObject powerupBlockIcon;
}