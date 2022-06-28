using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public List<NameToPrefab> prefabsByName;

    BuildManager buildManager;

    private Dictionary<string, HeroInfo> heroes = new Dictionary<string, HeroInfo>();
    private Dictionary<string, HeroInfo> heroesAwaitingRedeployment = new Dictionary<string, HeroInfo>();

    [Serializable]
    public class NameToPrefab
    {
        public string name;
        public GameObject prefab;
    }

    private class HeroInfo
    {
        public HeroInfo(GameObject prefab, GameObject gameObject, GameObject redeployment)
        {
            this.prefab = prefab;
            this.gameObject = gameObject;
            this.redeployment = redeployment;
        }

        public GameObject prefab;
        public GameObject gameObject;
        public GameObject redeployment;
        public Text redeploymentText;
        public Image redeploymentImage;
        public float redeploymentTime;
        public float timeRemaining;
    }

    private void Awake()
    {
        foreach (NameToPrefab prefabByName in prefabsByName)
        {
            GameObject go = Instantiate(prefabByName.prefab, Vector3.zero, Quaternion.identity);
            go.SetActive(false);

            GameObject shopGO = GameObject.Find("Shop" + prefabByName.name);

            foreach(Transform child in shopGO.transform)
            {
                GameObject childGO = child.gameObject;
                if(childGO.name == "Redeployment")
                {
                    HeroInfo heroInfo = new HeroInfo(prefabByName.prefab, go, childGO);
                    foreach (Transform redeploymentChild in child)
                    {
                        if (redeploymentChild.name == "Text")
                            heroInfo.redeploymentText = redeploymentChild.gameObject.GetComponent<Text>();
                        if (redeploymentChild.name == "Image")
                            heroInfo.redeploymentImage = redeploymentChild.gameObject.GetComponent<Image>();
                    }
                    heroes.Add(prefabByName.name, heroInfo);
                    childGO.SetActive(false);
                }
                else if(childGO.name == "DeploymentCostBackground")
                {
                    childGO.GetComponentInChildren<Text>().text = go.GetComponent<Hero>().cost.ToString();
                }
            }
        }
    }

    private void Start()
    {
        buildManager = BuildManager.instance;
    }

    private void Update()
    {
        List<string> keysToBeRemoved = new List<string>();

        foreach (string key in heroesAwaitingRedeployment.Keys)
        {
            HeroInfo heroInfo = heroesAwaitingRedeployment[key];

            heroInfo.timeRemaining -= Time.deltaTime;
            heroInfo.redeploymentText.text = string.Format("{0:F1}", heroInfo.timeRemaining);
            heroInfo.redeploymentImage.fillAmount = 1 - (heroInfo.timeRemaining / heroInfo.redeploymentTime);
            if (heroInfo.timeRemaining <= 0f)
            {
                keysToBeRemoved.Add(key);
                heroInfo.redeployment.SetActive(false);
            }
        }

        foreach(string key in keysToBeRemoved)
        {
            heroesAwaitingRedeployment.Remove(key);
        }
    }

    public void SelectHero(string name)
    {
        if(!heroesAwaitingRedeployment.ContainsKey(name))
            buildManager.SelectHeroToPlace(heroes[name].gameObject);
    }

    public void BeginRedeployment(string name)
    {
        HeroInfo heroInfo = heroes[name];
        heroInfo.redeployment.SetActive(true);
        heroInfo.redeploymentTime = heroInfo.gameObject.GetComponent<Hero>().redeploymentTime;
        heroInfo.timeRemaining = heroInfo.redeploymentTime;

        heroesAwaitingRedeployment.Add(name, heroInfo);
    }
}
