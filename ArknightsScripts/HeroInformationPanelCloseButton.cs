using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HeroInformationPanelCloseButton : MonoBehaviour
{
    protected HeroInformationPanel heroPanel;

    private void Start()
    {
        heroPanel = HeroInformationPanel.instance;
    }

    public void OnMouseDown()
    {
        Time.timeScale = 1f;
        heroPanel.gameObject.SetActive(false);
    }
}
