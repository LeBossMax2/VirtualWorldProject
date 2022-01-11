using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using Delaunay;
using Delaunay.Geo;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CityBuilder : MonoBehaviour
{
	public List<Car> carPrefabs = new List<Car>();
	public List<Car> bikePrefabs = new List<Car>();
	public List<GameObject> roadPrefabs = new List<GameObject>();
	public List<GameObject> bikeRoadPrefabs = new List<GameObject>();
	public List<Building> parkPrefabs = new List<Building>();
	public List<Building> buildingPrefabs = new List<Building>();
	public Ambulance ambulancePrefab;
	public Police policePrefab;
	public Material land;

	public int parkCount = 1000;
	public int buildingCount = 1000;
	public int pointCount = 1000;
	public int bikePointCount = 1000;
    public int width = 400;
    public int height = 400;
	public float[,] map;

	private Ambulance ambulance = null;
	private Police police = null;
	private Texture2D tx;

	private List<Vector2> m_points;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;

    private float [,] createMap() 
    {
        float [,] map = new float[width, height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				float v = Mathf.PerlinNoise(0.02f * i + 0.43f, 0.02f * j + 0.22f) * 1.4f - 0.2f;
				map[i, j] = Mathf.Clamp(v * v, 0.0f, 1.0f);
			}
		}
        return map;
    }

	void Start ()
	{
        map = createMap();
        Color[] pixels = createPixelMap(map);

        /* Create random points points */
		m_points = new List<Vector2> ();
		List<uint> colors = new List<uint> ();
		/* Randomly pick vertices */
		for (int i = 0; i < pointCount; i++)
        {
            colors.Add(0);
            Vector2 vec = RandomPoint(map);
            m_points.Add(vec);
        }
        /* Generate Graphs */
        Voronoi v = new Voronoi(m_points, colors, new Rect(0, 0, width, height));
		m_edges = v.VoronoiDiagram();
		m_spanningTree = v.SpanningTree(KruskalType.MINIMUM);
		m_delaunayTriangulation = v.DelaunayTriangulation();

		Color color = Color.blue;
		/* Shows Voronoi diagram */
		for (int i = 0; i < m_edges.Count; i++) {
			LineSegment seg = m_edges [i];				
			Vector2 left = (Vector2)seg.p0;
			Vector2 right = (Vector2)seg.p1;
			DrawLine (pixels,left, right,color);
			CreateRoad(left, right, roadPrefabs, carPrefabs);
		}


		/* Creat random bike road */
		List<Vector2> b_points = new List<Vector2> ();
		List<LineSegment> b_edges;
		List<LineSegment> b_spanningTree;
		List<LineSegment> b_delaunayTriangulation;
		/* Randomly pick vertices */
		for (int i = 0; i < bikePointCount-5; i++)
        {
            colors.Add(0);
            Vector2 vec = RandomPoint(map, x=>1-2*Math.Abs(x-0.5f));
            b_points.Add(vec);
        }
		
        /* Generate Graphs */
        Voronoi vb = new Voronoi(b_points, colors, new Rect(0, 0, width, height));
		b_edges = vb.VoronoiDiagram();
		b_spanningTree = vb.SpanningTree(KruskalType.MINIMUM);
		b_delaunayTriangulation = vb.DelaunayTriangulation();

		/* Shows Voronoi diagram */
		for (int i = 0; i < b_edges.Count; i++) {
			LineSegment seg = b_edges [i];				
			Vector2 left = (Vector2)seg.p0;
			Vector2 right = (Vector2)seg.p1;
			DrawLine (pixels,left, right,color);
			CreateRoad(left, right, bikeRoadPrefabs, bikePrefabs);
		}

		color = Color.red;
		/* Shows Delaunay triangulation */
		/*if (m_delaunayTriangulation != null) {
			for (int i = 0; i < m_delaunayTriangulation.Count; i++) {
					LineSegment seg = m_delaunayTriangulation [i];				
					Vector2 left = (Vector2)seg.p0;
					Vector2 right = (Vector2)seg.p1;
					DrawLine (pixels,left, right,color);
			}
		}*/

        /* Shows spanning tree */
        
		/*color = Color.black;
		if (m_spanningTree != null) {
			for (int i = 0; i< m_spanningTree.Count; i++) {
				LineSegment seg = m_spanningTree [i];				
				Vector2 left = (Vector2)seg.p0;
				Vector2 right = (Vector2)seg.p1;
				DrawLine (pixels,left, right,color);
			}
		}*/
		/* Apply pixels to texture */
		tx = new Texture2D(width, height);
		land.SetTexture("_MainTex", tx);
		tx.SetPixels(pixels);
		tx.Apply();

		StartCoroutine(generateBuildings(map));

		foreach (NavMeshSurface navSirface in GetComponents<NavMeshSurface>())
		{
			navSirface.BuildNavMesh();
		}

	}

	private IEnumerator generateBuildings(float[,] map)
	{
		yield return null;
		for (int b = 0; b < parkCount; b++)
		{
			Building parkPrefab = parkPrefabs[Random.Range(0, parkPrefabs.Count)];
			Vector2 point;
			Vector3 pos;
			Quaternion rotation;
			do
			{
				point = RandomPoint(map, x=>1-2*Math.Abs(x-0.5f));
				pos = transform.position + new Vector3(point.x, 0, point.y);
				float angle = Random.Range(0.0f, 360.0f);
				rotation = transform.rotation * Quaternion.AngleAxis(angle, Vector3.up);
			}
			while (Physics.CheckBox(pos + parkPrefab.collisionArea.center, parkPrefab.collisionArea.extents, rotation, LayerMask.GetMask("Default", "Road"), QueryTriggerInteraction.Ignore));
			Building park = Instantiate(parkPrefab, pos, rotation, transform);
			float v = map[(int)point.x, (int)point.y];
			park.transform.localScale = new Vector3(1, 1, 1);
		}

		for (int b = 0; b < buildingCount; b++)
		{
			Building buildingPrefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Count)];
			Vector2 point;
			Vector3 pos;
			Quaternion rotation;
			do
			{
				point = RandomPoint(map);
				pos = transform.position + new Vector3(point.x, 0, point.y);
				float angle = Random.Range(0.0f, 360.0f);
				rotation = transform.rotation * Quaternion.AngleAxis(angle, Vector3.up);
			}
			while (Physics.CheckBox(pos + buildingPrefab.collisionArea.center, buildingPrefab.collisionArea.extents, rotation, LayerMask.GetMask("Default", "Road"), QueryTriggerInteraction.Ignore));
			Building building = Instantiate(buildingPrefab, pos, rotation, transform);
			float v = map[(int)point.x, (int)point.y];
			building.transform.localScale = new Vector3(1, 1 + Random.Range(0.0f, 6.0f) * v * v * v, 1);
		}
	}

    private void CreateRoad(Vector2 left, Vector2 right, List<GameObject> roadPrefabList, List<Car> vehiclePrefabList)
    {
		Vector2 delta = right - left;
		GameObject roadPrefab = roadPrefabList[Random.Range(0, roadPrefabList.Count)];
		GameObject road = Instantiate(roadPrefab, transform.position + new Vector3(left.x, 0, left.y), transform.rotation * Quaternion.LookRotation(new Vector3(delta.x, 0, delta.y)), transform);
		road.transform.localScale = new Vector3(1, 1, delta.magnitude);

		Car car = Instantiate(vehiclePrefabList[Random.Range(0, vehiclePrefabList.Count)], transform.position + new Vector3(left.x + delta.x / 2, 0.1f, left.y + delta.y / 2), Quaternion.identity);
		car.City = this;

		if (ambulance == null)
		{
			ambulance = Instantiate(ambulancePrefab, transform.position + new Vector3(left.x + delta.x / 3, 0.1f, left.y + delta.y / 3), Quaternion.identity);
			ambulance.City = this;
		}
		else if (police == null)
		{
			police = Instantiate(policePrefab, transform.position + new Vector3(left.x + 5*delta.x /6 , 0.1f, left.y + 5*delta.y / 6), Quaternion.identity);
			police.City = this;
		}
	}
    private Vector2 RandomPoint(float[,] map)
	{
    	return RandomPoint(map, x=>x);
	}

    private Vector2 RandomPoint(float[,] map, Func<float, float> probaFunc)
	{
		List<System.Tuple<Vector2, float>> candidates = new List<System.Tuple<Vector2, float>>();
		for (int i = 0; i < 256; i++)
        {
			Vector2 pos = new Vector2(Random.Range(0.0f, width), Random.Range(0.0f, height));
			candidates.Add(System.Tuple.Create(pos, probaFunc(map[(int)pos.x, (int)pos.y])));
		}

		float totalWeight = candidates.Sum(tuple => tuple.Item2);

		float value = Random.Range(0, totalWeight);

		for (int i = 0; i < 256; i++)
        {
			if (value < candidates[i].Item2)
            {
				return candidates[i].Item1;
			}
			value -= candidates[i].Item2;
		}

		return candidates[candidates.Count - 1].Item1;
	}



    /* Functions to create and draw on a pixel array */
    private Color[] createPixelMap(float[,] map)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                pixels[i + j * width] = Color.Lerp(Color.white, Color.black, map[i, j]);
            }
        return pixels;
    }
    private void DrawPoint(Color[] pixels, Vector2 p, Color c) {
		if (p.x < width && p.x >= 0 && p.y < height && p.y >= 0) 
		    pixels[(int)p.x + (int)p.y * width]=c;
	}
	// Bresenham line algorithm
	private void DrawLine(Color [] pixels, Vector2 p0, Vector2 p1, Color c) {
		int x0 = (int)p0.x;
		int y0 = (int)p0.y;
		int x1 = (int)p1.x;
		int y1 = (int)p1.y;

		int dx = Mathf.Abs(x1-x0);
		int dy = Mathf.Abs(y1-y0);
		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;
		int err = dx-dy;
		while (true) {
            if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
    			pixels[x0 + y0 * width] = c;

			if (x0 == x1 && y0 == y1) break;
			int e2 = 2 * err;
			if (e2 > -dy) {
				err -= dy;
				x0 += sx;
			}
			if (e2 < dx) {
				err += dx;
				y0 += sy;
			}
		}
	}

	public void CallAmbulance(Car car)
	{
		ambulance.CallAmbulance(car);
	}

	public void CallPolice(Car car)
	{
		police.CallPolice(car);
	}

	public void StopAmbulance(Car car)
	{
		ambulance.StopAmbulance(car);
	}

	public void StopPolice(Car car)
	{
		police.StopPolice(car);
	}
}