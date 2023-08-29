using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraDragging : MonoBehaviour
{
    Vector3 origin;
    Vector3 difference;

    Camera mainCam;

    // DRAG
    bool isDragging;
    Vector3 GetMousePos => mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

    // SCROLL
    Vector2 wheelDelta;
    [SerializeField] float zoomScale = .5f;
    readonly float minCamSize = 4f;
    readonly float maxCamSize = 25f;


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
        if (isDragging)
        {
            difference = GetMousePos - transform.position;
            transform.position = origin - difference;
        }

        #region Scrolling
        wheelDelta = Mouse.current.scroll.ReadValue().normalized;
        mainCam.orthographicSize = Mathf.Clamp(mainCam.orthographicSize - wheelDelta.y * zoomScale, minCamSize, maxCamSize);
        #endregion
    }
}
