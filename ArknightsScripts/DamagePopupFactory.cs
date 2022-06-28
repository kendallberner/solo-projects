using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopupFactory : MonoBehaviour
{
    static GameObject damagePopupPrefab;

    void Start()
    {
        damagePopupPrefab = Prefabs.GetPrefabByName("DamagePopup");
    }

    public static void Create(float damage, Vector3 position, DAMAGE_TYPE damageType)
    {
        GameObject damagePopupGO = Instantiate(damagePopupPrefab, position, Quaternion.identity);
        DamagePopup damagePopup = damagePopupGO.GetComponent<DamagePopup>();
        TextMeshPro textMesh = damagePopupGO.GetComponent<TextMeshPro>();

        Color popupColor;
        if (damageType == DAMAGE_TYPE.ARTS)
            ColorUtility.TryParseHtmlString("#0094FF", out popupColor);
        else if (damageType == DAMAGE_TYPE.PHYSICAL)
            ColorUtility.TryParseHtmlString("#FF5600", out popupColor);
        else //PURE DAMAGE
            ColorUtility.TryParseHtmlString("#FFFFFF", out popupColor);
        textMesh.color = popupColor;

        damagePopup.textMesh.SetText(Mathf.Round(damage).ToString());
        damagePopupGO.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        damagePopup.targetPosition = damagePopupGO.transform.position + new Vector3(0, 10, 0);

        damagePopup.growthspan *= damagePopup.lifetime;
        damagePopup.holdspan *= damagePopup.lifetime;
        damagePopup.deathspan *= damagePopup.lifetime;
    }
}
