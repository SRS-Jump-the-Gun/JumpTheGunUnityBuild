using UnityEngine;

public class Inventory_test : MonoBehaviour
{
    [SerializeField] GameObject weaponWheelUI;
    private bool isWheelActive = false;

    [SerializeField] float sensitivity = 5f;
    private Vector2 virtualMousePos = Vector2.zero;

    [SerializeField] UnityEngine.UI.Image shotgunIcon;
    [SerializeField] UnityEngine.UI.Image pistolIcon;
    private int currentSelection = 0; // 0 means shotgun, 1 means pistol

    [SerializeField] Shotgun shotgunScript;
    [SerializeField] Pistol pistolScript;

    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            ShowWheel();

            // 1. Capture the 'delta' movement (how much the mouse moved this frame)
            float x = Input.GetAxisRaw("Mouse X");
            float y = Input.GetAxisRaw("Mouse Y");

            // 2. Add it to our virtual position
            virtualMousePos += new Vector2(x, y) * sensitivity;

            // 3. Limit the virtual cursor so it stays within a reasonable "circle"
            virtualMousePos = Vector2.ClampMagnitude(virtualMousePos, 100f);

            // 4. Get the Angle
            if (virtualMousePos.magnitude > 10f) // Deadzone to prevent accidental twitching
            {
                float angle = Mathf.Atan2(virtualMousePos.y, virtualMousePos.x) * Mathf.Rad2Deg;

                // Adjust angle so 0 is "Up" instead of "Right" (Standard Unity math)
                float finalAngle = (angle + 360) % 360;

                UpdateWheelSelection(finalAngle);
            }
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            HideWheel();

            MakeSelection();
        }
    }

    void ShowWheel()
    {
        isWheelActive = true;
        weaponWheelUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
    }

    void HideWheel()
    {
        isWheelActive = false;
        weaponWheelUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
    }

    void UpdateWheelSelection(float angle)
    {
        // Simple example for 2 weapons (Top and Bottom)
        // If angle is between 45 and 135, select Shotgun
        if (angle > 45 && angle < 135)
        {
            resetIcons();

            shotgunIcon.transform.localScale = Vector3.one * 0.25f * 1.2f;
            shotgunIcon.color = Color.yellow;
            currentSelection = 0;
        }
        else if (angle > 225 && angle < 315)
        {
            resetIcons();

            pistolIcon.transform.localScale = Vector3.one * 0.25f * 1.2f;
            pistolIcon.color = Color.yellow;
            currentSelection = 1;
        }
    }

    void resetIcons()
    {
        shotgunIcon.transform.localScale = Vector3.one * 0.25f;
        shotgunIcon.color = Color.white;
        pistolIcon.transform.localScale = Vector3.one * 0.25f;
        pistolIcon.color = Color.white;
    }

    void MakeSelection()
    {
        if (currentSelection == 0)
        {
            disableAllWeapons();
            shotgunScript.enabled = true;
        }
        else if (currentSelection == 1)
        {
            disableAllWeapons();
            pistolScript.enabled = true;
        }
    }

    void disableAllWeapons()
    {
        shotgunScript.enabled = false;
        pistolScript.enabled = false;
    }
}