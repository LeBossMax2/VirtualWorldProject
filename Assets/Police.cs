using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Police : Car
{
    public List<Car> carsToTrack = new List<Car> ();
    public bool onDuty = false;
    public float onDutySpeed = 5.0f;
    public float offDutySpeed = 3.5f;

    protected override void SetDestination()
    {
        if (onDuty)
        {
            agent.SetDestination(carsToTrack[0].agent.nextPosition);
        }
        else if (carsToTrack.Count != 0)
        {
            onDuty = true;
            agent.speed = onDutySpeed;
            agent.SetDestination(carsToTrack[0].agent.nextPosition);
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
                if (agent.remainingDistance <= 1)
                    yield return null;
                else 
                    yield return new WaitForSeconds(0.2f);

                if (carsToTrack.Count != 0)
                {
                    ReajustDestination();
                }
            }
            while (agent.remainingDistance >= 0.2);

            if (onDuty && agent.remainingDistance <= 0.2)
            {
                onDuty = false;
                agent.speed = offDutySpeed;
            }
        }
    }

    public void CallPolice(Car car)
    {
        Debug.Log("POLICE");
        if (!carsToTrack.Contains(car))
        {
            carsToTrack.Add(car);
        }
        SetDestination();
    }

    public void StopPolice(Car car)
    {
        if (carsToTrack.Contains(car))
        {
            Debug.Log("Removed police");
            carsToTrack.Remove(car);
        }
    }

    private void ReajustDestination()
    {
        agent.SetDestination(carsToTrack[0].agent.nextPosition);
    }

}
