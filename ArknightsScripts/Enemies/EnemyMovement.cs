using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyMovement : MonoBehaviour
{
    private Waypoint[] path;
    private Vector3 target;
    private int waypointIndex = 0;
    private Enemy enemy;
    private Animator animator;
    private float standbyTime;

    private readonly System.Random rand = new System.Random();

    public void SetPath(Waypoint[] path)
    {
        this.path = path;
    }

    protected void Start()
    {
        enemy = GetComponent<Enemy>();
        animator = enemy.animator;
        target = path[waypointIndex].transform.position + GetDisplacement();
    }

    private void Update()
    {
        if (standbyTime > 0.0f)
            enemy.isWaiting = true;

        if (!enemy.CanMove)
        {
            if(animator != null)
                animator.SetBool("Moving", false);
            standbyTime -= Time.deltaTime;
            if (standbyTime <= 0.0f)
                enemy.isWaiting = false;
            return;
        }

        if (animator != null)
            animator.SetBool("Moving", true);
        Vector3 directionToNextWaypoint = target - transform.position;
        float distanceThisFrame = Mathf.Min(directionToNextWaypoint.magnitude, enemy.GetAdjustedMoveSpeed() * Constants.NODE_WIDTH * Time.deltaTime);

        transform.Translate(directionToNextWaypoint.normalized * distanceThisFrame, Space.World);

        if (Vector3.Distance(transform.position, target) <= .02)
        {
            GetNextWaypoint();
        }
    }

    protected void GetNextWaypoint()
    {
        standbyTime = path[waypointIndex].standbyTime;

        if (waypointIndex >= path.Length - 1)
        {
            enemy.Escape();
            return;
        }

        waypointIndex++;
        target = path[waypointIndex].transform.position + GetDisplacement();
    }

    public float GetDistanceFromEndOfPath()
    {
        float totalDistance = Vector3.Distance(transform.position, path[waypointIndex].transform.position);
        for (var i = waypointIndex; i < path.Length-1; i++)
        {
            totalDistance += Vector3.Distance(path[i].transform.position, path[i+1].transform.position);
        }
        return totalDistance;
    }

    private Vector3 GetDisplacement()
    {
        return new Vector3((float)rand.NextDouble() * .4f * Constants.NODE_WIDTH - .2f * Constants.NODE_WIDTH, 0f, (float)rand.NextDouble() * .4f * Constants.NODE_WIDTH - .2f * Constants.NODE_WIDTH);
    }
}
