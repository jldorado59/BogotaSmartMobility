using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public float Speed;
    public float Radio;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetAxis("Horizontal") != 0)
            this.transform.position += Vector3.right * Input.GetAxis("Horizontal") * Speed * Time.deltaTime;

        if (Input.GetAxis("Vertical") != 0)
            this.transform.position += Vector3.forward * Speed * Input.GetAxis("Vertical") * Time.deltaTime;
    }
}
