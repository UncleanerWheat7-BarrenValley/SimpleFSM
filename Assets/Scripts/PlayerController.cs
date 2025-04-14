using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(horizontal, 0, vertical) * Time.deltaTime * 5);
    }
}
