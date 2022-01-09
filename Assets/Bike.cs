using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bike : Car
{
    public bool isDead = false;

    protected override void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Car" && !isDead)
        {
            StopCoroutine(RandomMove());
            agent.isStopped=true;
            isDead = true;
            if (collision.collider.gameObject.GetComponentInParent<Car> () != null)
            {
                Car script = collision.collider.GetComponentInParent<Car> ();
                City.CallPolice(script);
                script.isThug = true;
            }
            City.CallAmbulance(this);
        }
        else if (isDead && collision.collider.tag == "Ambulance")
        {
            isDead = false;
            StartCoroutine(RandomMove());
            agent.isStopped = false;
            City.StopAmbulance(this);
        }
    }

}
