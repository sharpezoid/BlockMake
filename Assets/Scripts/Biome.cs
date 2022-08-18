using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Biome", menuName = "Biomes/New Biome")]
public class Biome : ScriptableObject
{
    public string BiomeName;
    public List<Lode> Lodes = new List<Lode>();

}
