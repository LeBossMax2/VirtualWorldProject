using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Police : Car
{
    public List<NavMeshAgent> agentsToTrack = new List<NavMeshAgent> ();
    public bool onDuty = false;

    protected override void SetDestination()
    {
        if (onDuty)
        {
            agent.SetDestination(agentsToTrack[0].nextPosition);
        }
        else if (agentsToTrack.Count != 0)
        {
            onDuty = true;
            agent.SetDestination(agentsToTrack[0].nextPosition);
        }
        else 
        {
            while (true)
            {
                int a = Random.Range(0, City.width);
                int b = Random.Range(0, City.height);
                float val = Random.Range(0.0f, 1.0f);
                if (val*val < City.map[a,b])
                {
                    agent.SetDestination(new Vector3(a, 0, b)); 
                    break;
                }
            }
        }
    }
        
    protected override IEnumerator RandomMove()
    {
        while (!agent.isOnNavMesh)
        {
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }

        while (true)
        {
            SetDestination();
            
            do
            {
                yield return new WaitForSeconds(0.2f);
                if (agentsToTrack.Count != 0)
                {
                    ReajustDestination();
                }
            }
            while (agent.remainingDistance >= 0.2);

            if (onDuty && agent.remainingDistance <= 0.2)
            {
                agentsToTrack.RemoveAt(0);
                onDuty = false;
            }
        }
    }

    public void CallPolice(NavMeshAgent agent)
    {
        Debug.Log("POLICE");
        agentsToTrack.Add(agent);
        SetDestination();
    }

    private void ReajustDestination()
    {
        agent.SetDestination(agentsToTrack[0].nextPosition);
    }

}
