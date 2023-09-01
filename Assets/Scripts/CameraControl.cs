using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class CameraControl : MonoBehaviour
{
    public static CameraControl Instance { get; private set; }

    public event EventHandler<OnDragEventArgs> OnDrag;
    public class OnDragEventArgs : EventArgs
    {
        public bool isDragging;
    }
    Camera mainCam;

    // DRAG
    Vector3 originMouseWorldPos;
    Vector3 originMouseViewportPos;
    Vector3 mouseCamDifference;
    Vector3 GetMouseWorldPos => mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    Vector3 GetMouseViewportPos => mainCam.ScreenToViewportPoint(Mouse.current.position.ReadValue());

    bool isDragging;
    [SerializeField] float minDragChkDist = 0.05f;


    // SCROLL
    Vector2 wheelDelta;
    [SerializeField] float zoomScale = .5f;
    readonly float minCamSize = 3f;
    readonly float maxCamSize = 8f;


    void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
    }

    public void OnDrag_InputSystem(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            originMouseWorldPos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            originMouseViewportPos = mainCam.ScreenToViewportPoint(Mouse.current.position.ReadValue());
        }
        if (ctx.canceled)
        {
            OnDrag?.Invoke(this, new OnDragEventArgs { isDragging = false });
        }
        isDragging = ctx.started || ctx.performed;
    }

    void LateUpdate()
    {
        if (isDragging)
        {
            mouseCamDifference = GetMouseWorldPos - transform.position;

            // check minimum drag distance, deactivate mouse click for cell opening.
            if ((GetMouseViewportPos - originMouseViewportPos).magnitude >= minDragChkDist) // 0.04f is enough? maybe. 
            {
                // NOTICE : OnDrag event is called EVERYTIME the drag distance is over minDragChkDist
                OnDrag?.Invoke(this, new OnDragEventArgs { isDragging = true });
            }

            transform.position = originMouseWorldPos - mouseCamDifference;
        }

        #region Scrolling
        wheelDelta = Mouse.current.scroll.ReadValue().normalized;
        mainCam.orthographicSize = Mathf.Clamp(mainCam.orthographicSize - wheelDelta.y * zoomScale, minCamSize, maxCamSize);
        #endregion
    }
}
