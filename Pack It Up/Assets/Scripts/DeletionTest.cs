using UnityEngine;
using UnityEngine.SceneManagement;

public class DeletionTest : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        //delete object if it has no children
        if (transform.childCount == 0)
        {
            if (SceneManager.GetActiveScene().name != "MainGame")
            {
                // send the deleted object's name to the order manager
                OrderManager.instance.UpdateOrder(transform.parent.gameObject.name);
            }

            Destroy(transform.parent.gameObject);
        }
    }
}
