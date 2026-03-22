using UnityEngine;

public class TestDeletePackageRigidBody : MonoBehaviour
{
    [SerializeField] private Rigidbody2D packageRigidBody;
    private Animator workerAnimator;

    private void Awake()
    {
        workerAnimator = GameObject.Find("Worker").GetComponent<Animator>();
    }

    // delete the rigidbody2d of this package upon entering a trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(packageRigidBody);
        
        // snap the package into place
        transform.position = new Vector2(transform.position.x, 21.625f);

        // make the worker throw the package
        workerAnimator.SetTrigger("Throw");
    }
}
