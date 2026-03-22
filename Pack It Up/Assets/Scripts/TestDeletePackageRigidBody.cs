using UnityEngine;

public class TestDeletePackageRigidBody : MonoBehaviour
{
    [SerializeField] private Rigidbody2D packageRigidBody;

    // delete the rigidbody2d of this package upon entering a trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(packageRigidBody);
        
        // snap the package into place
        transform.position = new Vector2(transform.position.x, 21.375f);
    }
}
