using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrderManager : MonoBehaviour
{
    public static OrderManager instance;
    private int currentOrders = 0, totalOrders;
    public GameObject[] Blocks;
    private GameObject currentOrder;

    // text objects
    private TextMeshProUGUI currentOrderText;

    private void Awake()
    {
        Blocks = SpawnBlock.instance.Blocks;

        if (instance == null)
        {
            instance = this;
        }

        if (SceneManager.GetActiveScene().name == "Level 1")
        {
            totalOrders = 5;
            currentOrderText = GameObject.Find("Canvas/CurrentOrder").GetComponent<TextMeshProUGUI>();
            currentOrderText.enabled = false;
            gameObject.SetActive(false);
        }
    }

    public void EnableOrders()
    {
        currentOrderText.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentOrders == 0)
        {
            // set the current orders to 5
            currentOrders = 5;

            currentOrderText.text = "Order:  X" + currentOrders;

            // create the current Order
            currentOrder = Instantiate(Blocks[Random.Range(0, Blocks.Length)], gameObject.transform.position, Quaternion.identity);

            // delete the next block's scripts
            MonoBehaviour[] scripts = currentOrder.transform.GetChild(0).GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                Destroy(script);
            }
        }
    }
}
