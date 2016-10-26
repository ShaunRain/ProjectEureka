using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

	public GameObject nodeWall;
	public GameObject node;

	//节点半径
	public float nodeRaidus = 0.25f;
	//墙体过滤层
	public LayerMask layerMask;

	public Transform mover;
	public Transform dest;

	public NodeItem[,] gridNodes;
	private int w, h;

	private List<Edge> visibleEdges = new List<Edge>();

	private int OR_LEFT = 0;
	private int OR_UP = 1;
	private int OR_RIGHT= 2;
	private int OR_DOWN = 3;

	private GameObject wallRange;
	private GameObject pathRange;

	public List<NodeItem> pathNodes = new List<NodeItem> ();
	private List<GameObject> pathList = new List<GameObject>();

	void Awake() {
		//init grid
		w = Mathf.RoundToInt (transform.localScale.x * 2);
		h = Mathf.RoundToInt (transform.localScale.y * 2);
		gridNodes = new NodeItem[w, h];

		wallRange = new GameObject ("WallRange");
		pathRange = new GameObject ("PathRange");

		for (int i = 0; i < w; i++) {
			for (int j = 0; j < h; j++) {
				Vector3 pos = new Vector3 (i * 0.5f, j * 0.5f, -0.25f);
				// 通过节点中心发射圆形射线，检测当前位置是否可以通过
				bool isWall = Physics.CheckSphere (pos, nodeRaidus, layerMask);

				gridNodes [i, j] = new NodeItem (isWall, pos, i, j);
				// 如果是墙体，则画出不可行走的区域
				if (isWall) {
					GameObject obj = GameObject.Instantiate (nodeWall, pos, Quaternion.identity) as GameObject;
					obj.transform.SetParent (wallRange.transform);
				}
			}
		}
		initMap();
		randomTheWall ();

	}

	//根据坐标获得一个节点
	public NodeItem getNodeItem(Vector3 pos) {

		int x = Mathf.RoundToInt (pos.x) * 2;
		int y = Mathf.RoundToInt (pos.y) * 2;
		x = Mathf.Clamp (x, 0, w - 1);
		y = Mathf.Clamp (y, 0, h - 1);
		return gridNodes [x, y];

	}

	//获取周围节点
	public List<NodeItem> getNeighborhood(NodeItem node, int layer) {

		int x = node.x;
		int y = node.y;

		List<NodeItem> neighbors = new List<NodeItem> ();
		for(int i = -layer; i <= layer;i++) {
			for (int j = -layer; j <= layer; j++) {
				if (i == 0 && j == 0) {
					continue;
				}
				if (x + i >= 0 && x + i < w && y + j >= 0 && y + j < h) {
					neighbors.Add (gridNodes[x+i,y+j]);
				}
			}
		}
		return neighbors;

	}
		
	//获取周围Wall的数量
	private int getAroundWalls(NodeItem node, int layer) {
		int count = 0;
		foreach(var item in getNeighborhood(node, layer)) {
			if (item.isWall) {
				count++;
			}
		}
		return count;
	}

	private void checkRemove(NodeItem node) {

		int x = node.x;
		int y = node.y;

		if (x - 1 >= 0 
			&& x + 1 < w 
			&& y - 1 >= 0 
			&& y + 1 < h 
			&& node.isWall
			&& !gridNodes [x, y - 1].isWall) {
			if (gridNodes[x - 1, y - 1].isWall && !gridNodes [x - 1, y].isWall) {
				node.isWall = false;
				gridNodes [x - 1, y - 1].isWall = false;
			}
			if(gridNodes[x + 1, y - 1].isWall && !gridNodes [x + 1, y].isWall) {
				node.isWall = false;
				gridNodes [x + 1, y - 1].isWall = false;
			}
		}

	}

	//初始化地图
	//1. random all with wall or floor
	private void initMap() {
		
		gridNodes = new NodeItem[w, h];
		for (int i = 0; i < w; i++) {
			for (int j = 0; j < h; j++) {
				if (i == 0 || j == 0 || i == w - 1 || j == h - 1) {
					gridNodes[i, j] = new NodeItem (true
						, new Vector3 (i * 0.5f, j * 0.5f, -0.25f), i, j);
				} else {
					gridNodes[i, j] = new NodeItem (Random.Range(0.0f, 1.0f) < 0.3f ? true : false
						, new Vector3 (i * 0.5f, j * 0.5f, -0.25f), i, j);
				}
				NodeItem item = gridNodes [i, j];
				if(item.isWall) {
					Edge[] edges = new Edge[4];
					edges [0] = new Edge (new float[]{ item.pos.x - nodeRaidus, item.pos.y - nodeRaidus}
						, new float[]{ item.pos.x + nodeRaidus, item.pos.y - nodeRaidus});
					edges [1] = new Edge (new float[]{ item.pos.x + nodeRaidus, item.pos.y - nodeRaidus}
						, new float[]{ item.pos.x + nodeRaidus, item.pos.y + nodeRaidus});
					edges [2] = new Edge (new float[]{ item.pos.x + nodeRaidus, item.pos.y + nodeRaidus}
						, new float[]{ item.pos.x - nodeRaidus, item.pos.y + nodeRaidus});
					edges [3] = new Edge (new float[]{ item.x - nodeRaidus, item.pos.y + nodeRaidus}
						, new float[]{ item.pos.x - nodeRaidus, item.pos.y - nodeRaidus});
					gridNodes[i,j].edges = edges;
				}
			}
		}

	}

	//随机生成Wall
	private void randomTheWall() {

		//2. rule: switch to wall, if around walls <= 2 || > 5
		for (int r = 0; r < 3; r++) {

			for(int i = 1;i < w-1;i++) {
				for (int j = 1; j < h - 1; j++) {
					NodeItem node = gridNodes [i, j];
					int count = getAroundWalls (node, 1);
					int count2 = getAroundWalls (node, 2);
					if (!node.isWall && (count > 5 || count2 <= 2)) {
						node.isWall = true;
					}
				}
			
			}

		}

		//3. switch to floor, if around walls < 2
		for (int r = 0; r < 3; r++) {

			for(int i = 1;i < w-1;i++) {
				for (int j = 1; j < h - 1; j++) {
					NodeItem node = gridNodes [i, j];
					int count = getAroundWalls (node, 1);
					if (node.isWall && (count < 2)) {
						node.isWall = false;
					}
				}

			}

		}

		//4. remove the / & \
		foreach (var node in gridNodes) {
			if (node.isWall) {
				checkRemove (node);
			}
		}
			
			
		//5. init objects in map
		foreach (var node in gridNodes) {
			if (node.isWall) {
				GameObject obj = GameObject.Instantiate (nodeWall, node.pos, Quaternion.identity) as GameObject;
				obj.transform.SetParent (wallRange.transform);
			}
		}
		
	}

	//清除嵌入墙体的边
	private void ClearInsideEdge() {
		
		for(int i = 1;i < w - 1;i++) {
			for (int j = 1; j < h - 1; j++) {
				NodeItem node = gridNodes [i, j];
				if(node.isWall) {

					if(gridNodes[i, j - 1].isWall) {
						gridNodes[i,j].edges[0].isInAir = false;
					}

					if(gridNodes[i + 1, j].isWall) {
						gridNodes[i,j].edges[1].isInAir = false;
					}

					if(gridNodes[i, j + 1].isWall) {
						gridNodes[i,j].edges[2].isInAir = false;
					}

					if(gridNodes[i - 1, j].isWall) {
						gridNodes[i,j].edges[3].isInAir = false;
					}

				}
			}
		}

	}


	private void CollectAroundEdge(int range, Vector3 pos) {

		NodeItem posNode = getNodeItem(pos);
		if (posNode.x <= 0 || posNode.y >= 0) {
			return;
		}

		List<NodeItem> visibleRange = new List<NodeItem> ();

		int startX;
		int startY;
		int endX;
		int endY;
		startX = -range + posNode.x < 0 ? 0 : -range + posNode.x;
		startY = -range + posNode.y < 0 ? 0 : -range + posNode.y;
		endX = range + posNode.x > w ? w : range + posNode.x;
		endY = range + posNode.y > h ? h : range + posNode.y;
			
		for (int i = startX; i <= endX; i += 1) {
				for (int j = startY; j <= endY; j += 1) {
				visibleRange.Add(gridNodes[posNode.x + i, posNode.y + j]);
			}
		}


				
	}

	//收集可见墙体
	private void CollectVisibleEdge(int orient, int rangeRadius, float rangeRadians, Vector3 pos) {

		NodeItem posNode = getNodeItem(pos);
		float height = Mathf.Cos (rangeRadians) * rangeRadius;
		float bottom = Mathf.Sin (rangeRadians) * rangeRadius;
		int heightInt = Mathf.CeilToInt (height);
		int bottomInt = Mathf.CeilToInt (bottom * 2);

		List<NodeItem> visibleRange = new List<NodeItem> ();
		int sum = bottomInt;
		if (bottomInt >= 2) {
			while (bottomInt > 0) {
				int smaller = bottomInt - 2;
				sum += smaller;
				bottomInt -= 2;
			}
		}
			
	}

	//更新路径
	public void UpdatePath(List<NodeItem> line) {

		this.pathNodes = line;

		int currPathSize = pathList.Count;
		int i = 0;
		while (i < line.Count) {
			if (i < currPathSize) {
				pathList [i].transform.position = line [i].pos;
				pathList [i].SetActive (true);
			} else {
				GameObject obj = GameObject.Instantiate (node, line [i].pos, Quaternion.identity) as GameObject;
				obj.transform.SetParent (pathRange.transform);
				pathList.Add (obj);
			}
			i++;
		}
		for(i = line.Count;i< currPathSize;i++) {
			pathList [i].SetActive (false);
		}

	}

	public class NodeItem {

		public bool isWall;

		//真实位置
		public Vector3 pos;
		//网格坐标
		public int x, y;

		//四个边
		public Edge[] edges;

		//distance from start
		public int gCost;	
		//distance to dest
		public int hCost;	

		public int fCost {

			get {
				return gCost + hCost;
			}

		}

		public NodeItem parent;

		public NodeItem(bool isWall, Vector3 pos, int x, int y) {

			this.isWall = isWall;
			this.pos = pos;
			this.x = x;
			this.y = y;

		}

	}

	public class Edge{

		public float[] p1;
		public float[] p2;

		public Edge prev;
		public Edge next;

		public bool isInAir = true;

		public float distance = 0.0f;

		public Edge(float[] p1, float[] p2) {
			this.p1 = p1;
			this.p2 = p2;
		}

	}


}

