using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerParry : MonoBehaviour
{
    [SerializeField] private Parry parry;

    private void Update()
    {
        parry.Init(transform);
    }

    void OnParry(InputValue value)
    {
        var pressed = value.Get<float>();
        parry.gameObject.SetActive(pressed > 0);
    }
}