using UnityEngine;
using System.Collections;

public class TriggerMover : MonoBehaviour {

    public float interval = 10f;
    private float totalTime = 0f;
    private Vector3 destinatePosition;
    public float speed = 1.0f;
    private float rotateAngle;
    public bool rotateOnly = false;

	// Use this for initialization
	void Start () {
        this.totalTime = 0;
        this.destinatePosition = this.transform.localPosition;
        this.rotateAngle = Random.Range(15, 60);
        this.transform.Rotate(new Vector3(0, 0, Random.Range(0, 360)));
	}
	
	// Update is called once per frame
	void Update () {
        if (!this.rotateOnly)
        {
            this.totalTime += Time.deltaTime;
            if (this.totalTime > this.interval)
            {
                for (var i = 0; i < 10; i++)
                {
                    this.destinatePosition = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(0.5f, 1.5f), this.transform.localPosition.z);
                    if (!((-0.5 < this.destinatePosition.x) && (this.destinatePosition.x < 0.5)))
                    {
                        break;
                    }

                }
                this.totalTime = 0f;
            }
            var step = (1.0f / this.speed) * Time.deltaTime;
            this.transform.localPosition = Vector3.RotateTowards(this.transform.localPosition, this.destinatePosition, step, 500.0f);
        }

        transform.Rotate(Vector3.up * this.rotateAngle * Time.deltaTime);
    }

    public bool IsMoving 
    { 
        get
        {
            return (Vector3.Distance(this.transform.localPosition, this.destinatePosition) > 0.1f);
        }
    }
}
