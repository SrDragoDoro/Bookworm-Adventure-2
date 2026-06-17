using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct Modifier
{
    public float Multiplier;
    public int Heal;
    public int Damage;
    public SkillType Type;

}

public enum SkillType
{
    None,
    Normal,
    Purification,
    Poison,//->
    Fire//->
}


[CreateAssetMenu(fileName = "ModifiersDataBase", menuName = "Scriptable Objects/ModifiersDataBase")]
public class ModifiersDataBase : SerializedScriptableObject
{
    public Dictionary<Gems, Modifier> DataBase;


    //->consultar, retornar el modificar
    //-> conectarlo con un modifier Manager
    //->modifier leo el modifier , aplico lo que necesito atrabavez del game manager;

    
}
