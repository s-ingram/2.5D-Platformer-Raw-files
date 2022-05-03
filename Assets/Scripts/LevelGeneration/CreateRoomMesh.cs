using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRoomMesh : MonoBehaviour
{
	Mesh mesh;
	MeshFilter meshFilter;

	List<MeshFilter> meshFilterList = new List<MeshFilter>();


	void Awake()
	{
		meshFilter = gameObject.AddComponent<MeshFilter>();

		GenerateRoomMesh();
	}

	public void GenerateRoomMesh()
	{
		Transform floors = gameObject.transform.Find("Floors").transform;

		foreach (Transform floor in floors)
		{
			MeshFilter floorMeshFilter = floor.GetComponent<MeshFilter>();
			SpriteRenderer floorSpriteRenderer = floor.GetComponentInChildren<SpriteRenderer>();

			if (floorMeshFilter != null)
			{
				floorMeshFilter.mesh = ConvertSpriteToMesh(floorSpriteRenderer.sprite);
				meshFilterList.Add(floorMeshFilter);
			}
		}

		MeshFilter[] meshFilters = meshFilterList.ToArray();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];

		for (int i = 0; i < meshFilters.Length; i++)
		{
			combine[i].mesh = meshFilters[i].sharedMesh;

			Matrix4x4 matrix = meshFilters[i].transform.localToWorldMatrix;
			matrix[0, 3] = meshFilters[i].transform.localPosition.x;
			matrix[1, 3] = meshFilters[i].transform.localPosition.y;
			matrix[2, 3] = meshFilters[i].transform.localPosition.z;

			combine[i].transform = matrix;
		}

		meshFilter.mesh = new Mesh();
		meshFilter.mesh.CombineMeshes(combine);
	}

	private Mesh ConvertSpriteToMesh(Sprite sprite)
	{
		Mesh mesh = new Mesh();
		mesh.vertices = Array.ConvertAll(sprite.vertices, i => (Vector3) i);
		mesh.uv = sprite.uv;
		mesh.triangles = Array.ConvertAll(sprite.triangles, i => (int) i);

		return mesh;
	}
}