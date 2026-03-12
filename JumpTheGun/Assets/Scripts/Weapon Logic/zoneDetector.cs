using UnityEngine;

public class zoneDetector : MonoBehaviour
{

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collider active");
        if (other.CompareTag("Enemy") )
        {
            //Debug.Log("HITTING" + other.name);
            Destroy(other.gameObject);
            //other.GetComponent<scriptname>().takeDamage();
            //Debug.Log("Colliding!!!");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("Collider active");

        if (other.CompareTag("Enemy") )
        {
            //Debug.Log("HITTINGGGG" + other.name);
            Destroy(other.gameObject);
           // Debug.Log("Colliding!!!");

            //other.GetComponent<scriptname>().takeDamage();
        }
    }
}
