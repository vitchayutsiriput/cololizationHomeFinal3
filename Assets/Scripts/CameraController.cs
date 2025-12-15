using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    [SerializeField]
    private float xInput;

    [SerializeField]
    private float yInput;

    [SerializeField]
    private int moveSpeed = 20;

    [SerializeField]
    private float zoomModifier;

    public static CameraController instance;

    void Awake()
    {
        instance = this;
        cam = Camera.main;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        MoveByKB();
        ZoomInOut();
    }

    private void MoveByKB()
    {
        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(xInput, yInput, 0f);
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    public void MoveCamera(Vector3 pos)
    {
        transform.position = new Vector3(pos.x, pos.y, -10f);
    }

    public void ZoomInOut()
    {
        zoomModifier = -2f * Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetKey(KeyCode.Z))
            zoomModifier = -0.1f;
        if (Input.GetKey(KeyCode.X))
            zoomModifier = 0.1f;

        cam.orthographicSize += zoomModifier;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 4, 10);
    }
}
