using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathLine : MonoBehaviour
{
    private Waypoint[] path;
    private Waypoint target;
    private Waypoint rotationTarget;
    private int waypointIndex = 0;
    private TrailRenderer trailRenderer;
    private float moveSpeed;
    private float standByTime;

    protected void Start()
    {
        target = path[waypointIndex];
        trailRenderer = GetComponent<TrailRenderer>();
        moveSpeed = 8f;
    }

    private void Update()
    {
        if (standByTime > 0f)
        {
            //Trace circles on standby spot briefly
            transform.RotateAround(rotationTarget.transform.position, Vector3.up, 2880 * Time.deltaTime);
            standByTime -= Time.deltaTime * 10;

            if (standByTime < 0f)
                transform.SetPositionAndRotation(rotationTarget.transform.position, Quaternion.identity);
        }
        else
        {
            Vector3 directionToNextWaypoint = target.transform.position - transform.position;
            float distanceThisFrame = Mathf.Min(directionToNextWaypoint.magnitude, moveSpeed * Constants.NODE_WIDTH * Time.deltaTime);

            transform.Translate(directionToNextWaypoint.normalized * distanceThisFrame);

            if (Vector3.Distance(transform.position, target.transform.position) <= .05)
            {
                standByTime = target.standbyTime;
                rotationTarget = target;
                if(standByTime > 0f)
                {
                    transform.Translate(-directionToNextWaypoint.normalized * Constants.NODE_WIDTH * .25f);
                }
                GetNextWaypoint();
            }
        }
    }

    protected void GetNextWaypoint()
    {
        if (waypointIndex >= path.Length - 1)
            return;

        waypointIndex++;
        target = path[waypointIndex];
    }

    public void SetPath(Waypoint[] path)
    {
        this.path = path;
    }
}
