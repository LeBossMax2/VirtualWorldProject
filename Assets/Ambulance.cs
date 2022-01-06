using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ambulance : Car
{
    public List<NavMeshAgent> agentsToHeal = new List<NavMeshAgent> ();
    public bool onDuty = false;

    protected override void SetDestination()
    {
        if (onDuty)
        {}
        else if (agentsToHeal.Count != 0)
        {
            onDuty = true;
            agent.SetDestination(agentsToHeal[0].nextPosition);
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
                yield return new WaitForSeconds(Random.Range(0.2f, 1.0f));
            }
            while (agent.remainingDistance >= 0.02);

            if (onDuty && agent.remainingDistance <= 0.02)
            {
                agentsToHeal.RemoveAt(0);
                onDuty = false;
            }
        }
    }

    public void CallAmbulance(NavMeshAgent agent)
    {
        Debug.Log("AMBULANCE");
        agentsToHeal.Add(agent);
        SetDestination();
    }

}
