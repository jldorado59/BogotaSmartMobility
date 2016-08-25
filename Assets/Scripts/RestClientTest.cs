using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Data;
using TriangleNet.Tools;


public class RestClientTest : MonoBehaviour
{
    public GameObject _point;
    public Material _material;
    public int count = 0;
    RootObject root;

    int i=0;
    // Use this for initialization
    void Start()
    {
        IRestClient client = new HttpRestClient("http://localhost:3000", "application/json");

        string info = client.Request("/data", ERestMethod.GET);
   
        root = JsonConvert.DeserializeObject<RootObject>(info);

        Debug.Log(root.features[0].points);
        //client.AsyncRequest("data", ERestMethod.GET, null, Callback);
    }
	
    // Update is called once per frame
    void Update()
    {
        for(int w=0; w < count; w++)
        {
        if (i < root.features.Count)
        {            
            Feature feature = root.features[i];
            int pisos = feature.pisos;
            int coordinates = feature.points.Count; 

            Vector3[] vertex = new Vector3[coordinates * 2];
            Vector3[] vertex2 = new Vector3[coordinates];
            //Vector3[] normals = new Vector3[coordinates * 2];

            InputGeometry geom = new InputGeometry();

            int index = 0;
            for (int k = 0; k < coordinates; k = k + 1)
            {
                float x = (float) feature.points[k].x;
                float z = (float) feature.points[k].z;

                vertex[index] = new Vector3(x, 0, z);
                vertex[index + 1] = new Vector3(x, pisos*0.002f, z);
                vertex2[k] = new Vector3(x, pisos*0.002f, z);

                geom.AddPoint(x, z);
                //normals[index] = -Vector3.forward;
                //normals[index + 1] = -Vector3.forward;

                index = index + 2;
            }

            for (int k = 0; k < coordinates; k = k + 1)
                geom.AddSegment(k, k + 1);

            int triangles = (coordinates - 2) * 2;
            int[] indexes = new int[triangles * 3];

            index = 0;
            for (int k = 0; k < triangles; k = k + 1)
            {
                if (k % 2 == 0)
                {
                    indexes[index] = k;
                    indexes[index + 1] = k + 2;
                    indexes[index + 2] = k + 1;
                }
                else
                {
                    indexes[index] = k + 1;
                    indexes[index + 1] = k + 2;
                    indexes[index + 2] = k;
                }

                index = index + 3;
            }

            ICollection<Triangle> voronoy = Triangulate(geom);
            IEnumerator triangulation = voronoy.GetEnumerator(); 
            int[] indexes2 = new int[voronoy.Count * 3];
            index = 0;
            while (triangulation.MoveNext())
            {
                Triangle item = triangulation.Current as Triangle;
                //Add the identificator of those triangles
                indexes2[index] = item.P2;                                       
                indexes2[index + 1] = item.P1;
                indexes2[index + 2] = item.P0;

                index = index + 3;
            }

            GameObject building = new GameObject();
            building.transform.position = new Vector3((float)feature.centroid.x, 0, (float)feature.centroid.z);
            building.AddComponent<MeshFilter>();
            building.AddComponent<MeshRenderer>();
            building.GetComponent<MeshFilter>().mesh.vertices = vertex;
            building.GetComponent<MeshFilter>().mesh.triangles = indexes;
            //building.GetComponent<MeshFilter>().mesh.normals = normals;               
            building.GetComponent<MeshFilter>().mesh.RecalculateNormals();               
            building.GetComponent<MeshRenderer>().material = _material;           

            GameObject ceiling = new GameObject();
            ceiling.transform.position = new Vector3((float)feature.centroid.x, 0, (float)feature.centroid.z);
            ceiling.AddComponent<MeshFilter>();
            ceiling.AddComponent<MeshRenderer>();
            ceiling.GetComponent<MeshFilter>().mesh.vertices = vertex2;
            ceiling.GetComponent<MeshFilter>().mesh.triangles = indexes2;
            ceiling.GetComponent<MeshFilter>().mesh.RecalculateNormals();               
            ceiling.GetComponent<MeshRenderer>().material = _material; 
        }
        i++;
        }
    }

    void Callback(string info)
    {
        Debug.Log(info);     
    }

    private static ICollection<Triangle> Triangulate(InputGeometry input)
    {
        TriangleNet.Mesh mesh = new TriangleNet.Mesh();     
        mesh.Behavior.ConformingDelaunay = true;
        mesh.Behavior.Algorithm = TriangulationAlgorithm.SweepLine;
        mesh.Behavior.Convex = false;
        mesh.Triangulate(input);
        return mesh.Triangles;
    }  
}
