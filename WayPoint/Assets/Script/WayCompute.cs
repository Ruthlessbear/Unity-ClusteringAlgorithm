using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class WayCompute
{
    public static Vector3[] GetCorners(this NavMeshAgent agent, Vector3 des)
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(des, path);
        return path.corners;
    }
}
