using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;

public class VoronoiDemo : MonoBehaviour
{
	public GameObject roadPrefab;
    public Material land;
    public Texture2D tx;

	// Cell count
    public const int NPOINTS = 1000;

	// Image resolution
    public const int WIDTH = 400;
    public const int HEIGHT = 400;

    private List<Vector2> m_points;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;

    private float [,] createMap() 
    {
        float [,] map = new float[WIDTH, HEIGHT];
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
                map[i, j] = Mathf.Clamp(Mathf.PerlinNoise(0.02f * i + 0.43f, 0.02f * j + 0.22f) * 2.0f - 0.5f, 0.0f, 1.0f);
        return map;
    }

	void Start ()
	{
        float[,] map = createMap();
        Color[] pixels = createPixelMap(map);

        /* Create random points points */
		m_points = new List<Vector2> ();
		List<uint> colors = new List<uint> ();
		/* Randomly pick vertices */
		for (int i = 0; i < NPOINTS; i++)
        {
            colors.Add(0);
            Vector2 vec = RandomPoint(map);
            m_points.Add(vec);
        }
        /* Generate Graphs */
        Voronoi v = new Voronoi(m_points, colors, new Rect(0, 0, WIDTH, HEIGHT));
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
			CreateRoad(left, right);
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
		tx = new Texture2D(WIDTH, HEIGHT);
		land.SetTexture("_MainTex", tx);
		tx.SetPixels(pixels);
		tx.Apply();

	}

    private void CreateRoad(Vector2 left, Vector2 right)
    {
		Vector2 delta = right - left;

		Vector3 dir = new Vector3(delta.x, 0, delta.y).normalized;
		GameObject road = Instantiate(roadPrefab, transform.position + new Vector3(left.x, 0, left.y), transform.rotation * Quaternion.LookRotation(new Vector3(delta.x, 0, delta.y)), transform);
		road.transform.localScale = new Vector3(1, 1, delta.magnitude);
	}

    private static Vector2 RandomPoint(float[,] map)
	{
		List<System.Tuple<Vector2, float>> candidates = new List<System.Tuple<Vector2, float>>();
		for (int i = 0; i < 256; i++)
        {
			Vector2 pos = new Vector2(Random.Range(0, WIDTH), Random.Range(0, HEIGHT));
			candidates.Add(System.Tuple.Create(pos, map[(int)pos.x, (int)pos.y]));
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
        Color[] pixels = new Color[WIDTH * HEIGHT];
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
            {
                pixels[i + j * WIDTH] = Color.Lerp(Color.white, Color.black, map[i, j]);
            }
        return pixels;
    }
    private void DrawPoint(Color[] pixels, Vector2 p, Color c) {
		if (p.x < WIDTH && p.x >= 0 && p.y < HEIGHT && p.y >= 0) 
		    pixels[(int)p.x + (int)p.y * WIDTH]=c;
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
            if (x0 >= 0 && x0 < WIDTH && y0 >= 0 && y0 < HEIGHT)
    			pixels[x0 + y0 * WIDTH] = c;

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
}