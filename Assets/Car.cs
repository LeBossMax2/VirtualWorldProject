using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Car : MonoBehaviour
{
    public bool isWeak;

    public VoronoiDemo City { get; set; }
    private NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(RandomMove());
    }

    IEnumerator RandomMove()
    {
        while (!agent.isOnNavMesh)
        {
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }

        while (true)
        {
            agent.SetDestination(new Vector3(Random.Range(0, City.width), 0, Random.Range(0, City.height)));

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
            if (collision.collider.tag == "Car" || collision.collider.tag == "Police")
            {
                Debug.Log("test");
                agent.speed = 0;

                //StopCoroutine(RandomMove());
                //agent.isStopped = true;
            }
            if (collision.collider.tag == "Ambulance")
            {
                agent.speed = 1.5f;
            }

        }
    }
}
