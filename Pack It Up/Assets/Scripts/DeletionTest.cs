using UnityEngine;

public class DeletionTest : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        //delete object if it has no children
        if (transform.childCount == 0)
        {
            Destroy(transform.parent.gameObject);
        }
    }
}
