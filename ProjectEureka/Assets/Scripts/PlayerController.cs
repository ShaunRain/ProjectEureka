using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float speed;
	public GameObject map;

	private Rigidbody rb;

	void Start ()
	{
		rb = GetComponent<Rigidbody>();
	}

	void FixedUpdate ()
	{
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		float moveVertical = Input.GetAxisRaw ("Vertical");

		Vector3 movement = new Vector3 (moveHorizontal, moveVertical, 0.0f);

//		Vector3 dest = transform.position + movement * 0.2f;
//		if (!grid.getNodeItem (dest).isWall) {
//			rb.MovePosition(dest);
//		}
		rb.transform.Translate(movement * Time.deltaTime * speed);
//		rb.AddForce (movement * speed);

	}
}
