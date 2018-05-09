using UnityEngine;

public class PlayerControls : MonoBehaviour 
{
    public float speed = 8.0F;
    public float rotateSpeed = 1.0F;

    void Update()
    {
        CharacterController controller = GetComponent<CharacterController>();
        transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed, 0);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        float curSpeed = speed * Input.GetAxis("Vertical");
        controller.SimpleMove(forward * curSpeed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Ray ray = new Ray(transform.position, transform.forward);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 2, -1))
            {
                PokeableLinedef lc = hit.collider.gameObject.GetComponent<PokeableLinedef>();
                if (lc != null)
                    lc.Poke(gameObject);
            }
        }
    }
}
