using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PfNode : MonoBehaviour
{
	public List<(float distance, PfNode node)> connectedNodes = new List<(float distance, PfNode node)>();
	public bool toBeDeleted = true;

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, 0.5f);
		foreach ((float, PfNode) node in connectedNodes)
			Gizmos.DrawLine(this.transform.position, node.Item2.transform.position);
	}

	public void AddNode(float distance, PfNode node)
	{
		connectedNodes.Add((distance, node));
	}

	public int Size()
	{
		return connectedNodes.Count;
	}

	public void DeleteNode()
	{
		Destroy(this);
	}
}
