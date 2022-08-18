using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Lode", menuName = "Biomes/New Lode")]
public class Lode : ScriptableObject
{
    public string LodeName;
    public byte BlockID;
    public float MinHeight;
    public float MaxHeight;
    public float Threshold;
    public List<Octave> Octaves = new List<Octave>();

    public enum eLodePatternType
    {
        Spherical,
        Hemispherical
    }
    public eLodePatternType LodePatternType;
}

[System.Serializable]
public class Octave
{
    public float Scale;
    public Vector3 Offset;
}
