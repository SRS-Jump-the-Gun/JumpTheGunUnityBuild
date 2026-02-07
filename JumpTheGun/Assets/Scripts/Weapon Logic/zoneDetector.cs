using UnityEngine;

public class zoneDetector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && Input.GetMouseButtonDown(0))
        {
            Destroy(other.gameObject);
            //other.GetComponent<scriptname>().takeDamage();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && Input.GetMouseButtonDown(0))
        {
            Destroy(other.gameObject);
            //other.GetComponent<scriptname>().takeDamage();
        }
    }
}
