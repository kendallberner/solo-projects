using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
    public static Prefabs instance;

    [Header("Other")]
    public GameObject damagePopup;

    [Header("Enemies")]
    public GameObject avenger;
    public GameObject caster;
    public GameObject cocktailThrower;
    public GameObject corruptedKnight;
    public GameObject enemy;
    public GameObject heavyDefender;
    public GameObject logger;
    public GameObject originiumSlug;
    public GameObject smolEnemy;
    public GameObject soldier;


    private static Dictionary<string, GameObject> prefabsByName;

    private void Awake()
    {
        instance = this;

        prefabsByName = new Dictionary<string, GameObject>();

        prefabsByName.Add("Avenger", avenger);
        prefabsByName.Add("Caster", caster);
        prefabsByName.Add("CocktailThrower", cocktailThrower);
        prefabsByName.Add("CorruptedKnight", corruptedKnight);
        prefabsByName.Add("Enemy", enemy);
        prefabsByName.Add("HeavyDefender", heavyDefender);
        prefabsByName.Add("Logger", logger);
        prefabsByName.Add("OriginiumSlug", originiumSlug);
        prefabsByName.Add("SmolEnemy", smolEnemy);
        prefabsByName.Add("Soldier", soldier);

        prefabsByName.Add("DamagePopup", damagePopup);
    }

    public static GameObject GetPrefabByName(string name)
    {
        return prefabsByName[name];
    }
}
