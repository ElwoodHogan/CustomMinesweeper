using UnityEngine;

public class CameraAI : MonoBehaviour
{
    [SerializeField] Vector3 mouseLastFrame;
    [SerializeField] Camera cam;
    [SerializeField] float baseCamSpeed;
    [SerializeField] Transform grid;
    [SerializeField] Transform backGround;
    [SerializeField] GameObject MainCanvas;
    private void Update()
    {
        if (!Application.isFocused) return;
        Vector2 mouseChange = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - mouseLastFrame) * SettingMenuAI.SM.GetPS();
        if (Input.GetMouseButton(2)) transform.Translate(-mouseChange);  //Pans the camera
        mouseLastFrame = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.mouseScrollDelta.y != 0) cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - ((Input.mouseScrollDelta.y / 1.5f) * SettingMenuAI.SM.GetSS()), 2, SettingMenuAI.SM.GetCC() ? 20 : 999);

        transform.Translate(new Vector3(Input.GetAxisRaw("Horizontal") * SettingMenuAI.SM.GetMS() * baseCamSpeed, Input.GetAxisRaw("Vertical") * SettingMenuAI.SM.GetMS() * baseCamSpeed, 0));

        grid.position = transform.position.Floor();
        backGround.position = transform.position.Change(0, 0, 10);
    }
}
