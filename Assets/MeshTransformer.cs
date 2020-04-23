using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class MeshTransformer : MonoBehaviour
{
    public MeshFilter transformedMesh;
    public Transform targetMesh;
    Vector3[] meshVerticesCollision;
    Vector3[] meshVerticesWorld;
    Vector3[] meshVertices;
    Vector2[] meshUV;
    int[] meshTriangles;
    public Mesh mesh;
    public MeshFilter meshFilter;
    SphereCollider sphereCollider;

    public float maxStrength = 1f;
    public float strength = 0.1f;
    public bool extrude = true;
    public Boolean updateMesh = false;
    public Boolean checkCollision = false;
    // Start is called before the first frame update
    void Start()
    {

    }
    // Start is called before the first frame update
    void OnEnable()
    {
        sphereCollider = GetComponent<SphereCollider>();
        meshFilter = targetMesh.GetComponent<MeshFilter>();
        mesh = meshFilter.sharedMesh;
        meshVertices = mesh.vertices;
        meshVerticesWorld = LocalToWorld(meshVertices);
        meshUV = mesh.uv;
        meshTriangles = mesh.triangles;
    }

    void UpdateMesh()
    {
        mesh.vertices = meshVertices;
        mesh.uv = meshUV;
        mesh.triangles = meshTriangles;
        meshFilter.sharedMesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        if (updateMesh)
        {
            updateMesh = false;
            UpdateMesh();
        }

        if (checkCollision)
        {
            checkCollision = false;

            transformedMesh.sharedMesh = GetNewMesh();
            UpdateMesh();

        }

    }
    public Mesh GetNewMesh()
    {
        Dictionary<int, Vector3> goodVertices = new Dictionary<int, Vector3>();
        List<Vector3> badVertices = new List<Vector3>();

        foreach (var indice in meshTriangles)
        {
            bool goodVertice = true;
            var vertice = GetVertice(indice);
            
            float distance = Vector3.Distance(LocalToWorld(vertice), transform.position + sphereCollider.center);
            float radius = sphereCollider.radius * transform.localScale.x;

            if (distance <= radius)
                goodVertice = false;

            if (goodVertice)
                goodVertices[indice] = vertice;
            else
                badVertices.Add(vertice);
        }

        for(int x = 0; x < meshVertices.Length; ++x)
        {
            if (badVertices.Contains(meshVertices[x]))
            {
                float modifier = maxStrength * (extrude ? 1f : -1f) * strength;
                Vector3 dir = transform.position - LocalToWorld(meshVertices[x]);
                meshVertices[x] += dir * modifier;
            }
        }

        Mesh newMesh = new Mesh() {
            vertices = meshVertices,
            triangles = meshTriangles,
            uv = meshUV
        };

        return newMesh;
    }

    public Vector3[] LocalToWorld(Vector3[] input)
    {
        List<Vector3> worldVectors = new List<Vector3>();
        Vector3 targetPosition = targetMesh.position;

        foreach (var vector in input)
            worldVectors.Add(new Vector3(targetPosition.x + vector.x, targetPosition.y + vector.y, targetPosition.z + vector.z));

        return worldVectors.ToArray();
    }

    public Vector3 LocalToWorld(Vector3 input)
    {
        Vector3 targetPosition = targetMesh.position;

        return new Vector3(targetPosition.x + input.x, targetPosition.y + input.y, targetPosition.z + input.z);
    }

    public Vector3 GetVertice(int indice)
    {
        return meshVertices[indice];
    }
}