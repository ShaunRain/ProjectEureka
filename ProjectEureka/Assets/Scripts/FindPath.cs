using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FindPath : MonoBehaviour {

	private Grid grid;

	// Use this for initialization
	void Start () {
		grid = GetComponent<Grid> ();
	}

	// Update is called once per frame
	void Update () {
		AStarFinding (grid.mover.position, grid.dest.position);
	}

	void AStarFinding(Vector3 start, Vector3 end) {

		Grid.NodeItem startNode = grid.getNodeItem (start);
		Grid.NodeItem endNode = grid.getNodeItem (end);

		List<Grid.NodeItem> openList = new List<Grid.NodeItem> ();
		HashSet<Grid.NodeItem> closeSet = new HashSet<Grid.NodeItem> ();
		openList.Add (startNode);

		while(openList.Count > 0) {

			Grid.NodeItem currNode = openList [0];

			for (int i = 0; i < openList.Count; i++) {
				if (openList [i].fCost <= currNode.fCost &&
				   openList [i].hCost < currNode.hCost) {
					currNode = openList [i];
				}
			}

			openList.Remove (currNode);
			closeSet.Add (currNode);

			if (currNode == endNode) {
				GeneratePath (startNode, endNode);
				return;
			}

			foreach(var item in grid.getNeighborhood(currNode, 1)) {

				if (item.isWall || closeSet.Contains (item)) {
					continue;
				}

				int newG = currNode.gCost + MeasureWithDiagnol (currNode, item);
				// 如果距离更小，或者原来不在开始列表中
				if (newG < item.gCost || !openList.Contains (item)) {
					item.gCost = newG;
					item.hCost = MeasureWithDiagnol (item, endNode);
					item.parent = currNode;
					// 如果节点是新加入的，将它加入打开列表中
					if (!openList.Contains (item)) {
						openList.Add (item);
					}
				}

			}

		}

		GeneratePath (startNode, null);

	}

	//生成路径
	void GeneratePath(Grid.NodeItem start, Grid.NodeItem end) {
		List<Grid.NodeItem> path = new List<Grid.NodeItem> ();
		if (end != null) {
			Grid.NodeItem temp = end;
			while (temp != start) {
				path.Add (temp);
				temp = temp.parent;
			}
			path.Reverse ();
		}
		grid.UpdatePath (path);
	}

	//可以不使用sqrt操作的测量,但不能使用曼哈顿距离
	public int MeasureWithDiagnol(Grid.NodeItem a, Grid.NodeItem b) {

		int hDiff = Mathf.Abs (b.y - a.y);
		int wDiff = Mathf.Abs (b.x - a.x);

		if (hDiff >= wDiff) {
			return 14 * wDiff + 10 * (hDiff - wDiff);
		} else {
			return 14 * hDiff + 10 * (wDiff - hDiff);
		}

	}
}
