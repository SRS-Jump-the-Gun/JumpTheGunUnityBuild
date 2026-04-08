using UnityEngine;

public class Parry : MonoBehaviour
{
    [SerializeField] public GameObject source;
    [SerializeField] public Quaternion direction;

    public void Init(Transform transform)
    {
        source = transform.gameObject;
        direction = transform.rotation;
    }
}
