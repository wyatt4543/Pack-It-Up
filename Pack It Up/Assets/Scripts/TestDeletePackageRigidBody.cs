using UnityEngine;

public class TestDeletePackageRigidBody : MonoBehaviour
{
    [SerializeField] private Rigidbody2D packageRigidBody;
    private Animator workerAnimator;
    private GameObject chatBubble;
    private Vector2 topBoxPostion = new Vector2(1.04f, 20.6f);
    private float packageFlySpeed = 50.0f;

    private void Awake()
    {
        workerAnimator = GameObject.Find("Worker").GetComponent<Animator>();
        chatBubble = GameObject.Find("GameBoard/ChatBubble");
    }

    // delete the rigidbody2d of this package upon entering a trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(packageRigidBody);
        
        // snap the package into place
        transform.position = new Vector2(transform.position.x, 21.625f);

        // make the worker throw the package
        workerAnimator.SetTrigger("Throw");

        // make the package fly towards the box
        while (Vector3.Distance(transform.position, topBoxPostion) > 0.01f)
        {
            // move the package to the box
            transform.position = Vector3.MoveTowards(transform.position, topBoxPostion, packageFlySpeed * Time.deltaTime);
        }

        GameObject nextBlock = transform.GetChild(0).gameObject;

        // double the next block's size
        nextBlock.transform.localScale = new Vector3(0.5f, 0.5f, 1);

        // make the chat bubble the parent of the next block
        nextBlock.transform.parent = chatBubble.transform;

        // move the next block to the chat bubble
        nextBlock.transform.localPosition = new Vector2(-0.6f, 0);

        // remove the box flaps
        if (nextBlock.name.Contains("BoxBlock"))
        {
            foreach (Transform square in nextBlock.transform.GetChild(0))
            {
                if (square.name == "BoxFlap")
                {
                    Destroy(square.gameObject);
                }
            }
        }

        // when at the box destroy itself
        Destroy(gameObject);
    }
}
