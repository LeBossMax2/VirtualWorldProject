using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Car : MonoBehaviour
{
    public bool isWeak;
    public bool isThug;

    public VoronoiDemo City { get; set; }
    protected NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(RandomMove());
    }

    protected virtual IEnumerator RandomMove()
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
            while (agent.remainingDistance >= 0.2);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("COLLIDE");
        if (isWeak)
        {
            if (collision.collider.tag == "Car")
            {
                Debug.Log("test");
                StopCoroutine(RandomMove());
                agent.isStopped=true;
                City.CallAmbulance(agent);
                City.CallPolice(collision.collider.GetComponentInParent<NavMeshAgent>());
                Car script = collision.collider.GetComponentInParent<Car>();
                script.isThug = true;
            }
            if (collision.collider.tag == "Ambulance")
            {
                Debug.Log("Ca marche");
                StartCoroutine(RandomMove());
                agent.isStopped=false;
            }

        }
        else 
        {
            if (isThug && collision.collider.tag == "Police")
            {
                Debug.Log("Uh");
                Destroy(this);
            }
        }

    }

    protected virtual void SetDestination()
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
