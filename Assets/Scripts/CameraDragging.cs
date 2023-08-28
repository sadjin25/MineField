using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraDragging : MonoBehaviour
{
    Vector3 origin;
    Vector3 difference;

    Camera mainCam;

    bool isDragging;

    Vector3 GetMousePos => mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

    void Awake()
    {
        mainCam = Camera.main;
    }

    public void OnDrag(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            origin = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
        isDragging = ctx.started || ctx.performed;
    }

    void LateUpdate()
    {
        if (!isDragging) return;
        difference = GetMousePos - transform.position;
        transform.position = origin - difference;
    }
}
