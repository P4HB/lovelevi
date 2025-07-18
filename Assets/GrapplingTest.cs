using UnityEngine;
using TMPro; // 텍스트메쉬프로 쓸 경우

public class GrapplingTest : MonoBehaviour
{
    public Transform grappleStartPoint;
    public LineRenderer line;
    public TextMeshProUGUI grappleHintText;  // UI 연결용

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            if (hit.collider.CompareTag("Tree"))
            {
                grappleHintText.text = "Can Grapple!";
            }
            else
            {
                grappleHintText.text = "Can't Grapple";
            }
        }
        else
        {
            grappleHintText.text = "";
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit clickHit, 50f) && clickHit.collider.CompareTag("Tree"))
            {
                line.positionCount = 2;
                line.SetPosition(0, grappleStartPoint.position);
                line.SetPosition(1, clickHit.point);
            }
        }
    }
}
