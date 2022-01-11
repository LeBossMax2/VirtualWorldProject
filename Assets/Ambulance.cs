using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ambulance : Car
{
    public List<Car> carsToHeal = new List<Car> ();
    public bool onDuty = false;
    public Car test;

    public float onDutySpeed = 4.5f;
    public float offDutySpeed = 3.5f;

    protected override void SetDestination()
    {
        if (onDuty)
        {}
        else if (carsToHeal.Count != 0)
        {
            onDuty = true;
            agent.speed = onDutySpeed;
            agent.SetDestination(carsToHeal[0].agent.nextPosition);
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
                if (carsToHeal.Count != 0)
                {
                    ReajustDestination();
                }
            }
            while (agent.remainingDistance >= 0.5);

            if (onDuty && agent.remainingDistance <= 0.5)
            {
                onDuty = false;
                agent.speed = offDutySpeed;
            }
        }
    }

    public void CallAmbulance(Car car)
    {
        Debug.Log("AMBULANCE");
        if (!carsToHeal.Contains(car))
        {
            carsToHeal.Add(car);
        }
        SetDestination();
    }

    public void StopAmbulance(Car car)
    {
        if (carsToHeal.Contains(car))
        {
            Debug.Log("Removed ambulance");
            carsToHeal.Remove(car);
        }
    }

    private void ReajustDestination()
    {
        agent.SetDestination(carsToHeal[0].agent.nextPosition);
    }
}
