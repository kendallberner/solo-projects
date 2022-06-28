using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Vector3 positionOffset;

    public Vector3 GetBuildPosition()
    {
        return transform.position + positionOffset;
    }
}
