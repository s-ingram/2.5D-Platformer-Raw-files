using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
	public static void ConnectNodes(List<PfNode> nodes)
	{
		foreach (PfNode node in nodes)
			foreach (PfNode otherNode in nodes)
				if (Physics2D.Linecast(node.transform.position, otherNode.transform.position, LayerMask.GetMask("Walls")).collider == null && node != otherNode)
					node.AddNode(Vector3.Distance(node.transform.position, otherNode.transform.position), otherNode);
	}

	public static void RemoveUnconnectedNodes(List<PfNode> nodes)
	{
		for (int i = nodes.Count - 1; i >= 0; i--)
			if (nodes[i].Size() == 0)
			{
				nodes[i].DeleteNode();
				nodes.RemoveAt(i);
			}
	}

	public static void DijkstrasAlgorithm()
	{

	}
}
