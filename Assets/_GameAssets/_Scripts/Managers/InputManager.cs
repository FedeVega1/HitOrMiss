using UnityEngine;

public interface IClickeable
{
    public void OnClick();
}

public class InputManager : MonoBehaviour
{
    Camera _MainCamera;
    Camera MainCamera
    {
        get
        {
            if (_MainCamera == null) _MainCamera = Camera.main;
            return _MainCamera;
        }
    }

    public bool EnableInput { get; set; }

    void Update()
    {
        if (!EnableInput || Utilities.MouseOverUI() || !Input.GetMouseButtonDown(0)) return;

        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, LayerMask.GetMask("ClickeableObjects")))
        {
            IClickeable target = hit.transform.GetComponent<IClickeable>();
            if (target != null)
            {
                target.OnClick();
                Debug.DrawLine(ray.origin, hit.point, Color.green, 1);
                return;
            }

            Debug.DrawLine(ray.origin, hit.point, Color.red, 1);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.red, Time.deltaTime);
        }
    }
}
