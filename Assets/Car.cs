using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Car : MonoBehaviour
{
    public bool isThug;

    public CityBuilder City { get; set; }
    public NavMeshAgent agent;

    private float startTime;
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
            while (agent.remainingDistance >= 0.2 && Time.time < startTime + 30.0f);
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (isThug && collision.collider.tag == "Police")
        {
            City.StopPolice(this);
            Destroy(gameObject, 0.1f);
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
                agent.SetDestination(City.transform.position + new Vector3(a, 0, b)); 
                startTime = Time.time;
                break;
            }
        }
    }

}
