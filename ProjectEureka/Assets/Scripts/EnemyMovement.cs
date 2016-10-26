using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour {

	public int speed = 2;

	private Grid grid;
	private Rigidbody rb;

	private bool isOver = true;

	void Start () {
	
		grid = GetComponentInParent<Grid> ();
		rb = GetComponent<Rigidbody>();  

	}

	void Update () {
	
			if (grid.pathNodes.Count > 0) {
				Vector3 step;
				if (grid.pathNodes.Count < 2) {
					step = grid.pathNodes [0].pos;
				} else {
					step = grid.pathNodes [grid.pathNodes.Count - 2].pos;
				}
				Vector3 offSet = step - transform.position;
				rb.transform.position += offSet.normalized * speed * Time.deltaTime;
			}

	}

}
