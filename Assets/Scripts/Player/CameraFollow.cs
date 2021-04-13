using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target;
    [Space(10)]
    public float Distance = 10;
    public float ZoomSpeed = 500;
    public float VerticalAngle = 30;
    public float HorizontalAngle = 0;

    public float MouseSensitivity = 10;

    [Space(20),SerializeField]
    private float MinVerticalAngle = 0;
    [SerializeField]
    private float MaxVerticalAngle = 90;
    //[SerializeField]
    //private float MinHorizontalAngle = 0;
    //[SerializeField]
    //private float MaxHorizontalAngle = 360;
    [SerializeField]
    private float MinDistance = 3f;
    [SerializeField]
    private float MaxDistance = 20f;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }



    void LateUpdate()
    {
        if (target == null || Camera.main == null)
            return;
        Vector3 targetpos = target.transform.position;

        GetMouseControl();

        float hDis = Distance * Mathf.Cos(VerticalAngle *Mathf.Deg2Rad);
        float z = targetpos.z + hDis * Mathf.Cos(HorizontalAngle * Mathf.Deg2Rad);
        float y = targetpos.y + Distance * Mathf.Sin(VerticalAngle * Mathf.Deg2Rad);
        float x = targetpos.x + hDis * Mathf.Sin(HorizontalAngle * Mathf.Deg2Rad);

        Camera.main.transform.position = new Vector3(x, y, z);
        Camera.main.transform.LookAt(target.transform);
    }

    private void GetMouseControl()
    {
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
        float ms = Input.GetAxis("Mouse ScrollWheel");

        //HorizontalAngle = Mathf.Clamp(HorizontalAngle+ mx * Time.deltaTime * MouseSensitivity,
        //    MinHorizontalAngle, MaxHorizontalAngle);
        //VerticalAngle = Mathf.Clamp(VerticalAngle + -my * Time.deltaTime * MouseSensitivity,
        //    MinVerticalAngle, MaxVerticalAngle);

        HorizontalAngle = HorizontalAngle + mx * Time.deltaTime * MouseSensitivity;
        VerticalAngle = VerticalAngle + -my * Time.deltaTime * MouseSensitivity;
        Distance = Mathf.Clamp(Distance - ms * Time.deltaTime * ZoomSpeed,
            MinDistance, MaxDistance);

    }
}
