using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrderManager : MonoBehaviour
{
    public static OrderManager instance;
    private int currentOrders = 0, totalOrders, currentTotalOrders;
    public GameObject[] Blocks;
    private GameObject currentOrder;
    private int customOrder = -1;

    // text objects
    private TextMeshProUGUI currentOrderText;
    private TextMeshProUGUI totalOrdersText;

    private void Awake()
    {
        currentOrderText = GameObject.Find("LevelCanvas/CurrentOrder").GetComponent<TextMeshProUGUI>();
        totalOrdersText = GameObject.Find("LevelCanvas/TotalOrders").GetComponent<TextMeshProUGUI>();

        if (instance == null)
        {
            instance = this;
        }

        if (SceneManager.GetActiveScene().name == "Level 1")
        {
            totalOrders = currentTotalOrders = 2;
            currentOrderText.enabled = false;
            totalOrdersText.enabled = false;
            gameObject.SetActive(false);
        }

        if (SceneManager.GetActiveScene().name == "Level 2")
        {
            totalOrders = currentTotalOrders = 2;
        }

        if (SceneManager.GetActiveScene().name == "Level 3")
        {
            totalOrders = currentTotalOrders = 2;
            // set the initial custom order
            customOrder = 7;
        }

        if (SceneManager.GetActiveScene().name == "Level 4")
        {
            totalOrders = currentTotalOrders = 2;
            // set the initial custom order
            customOrder = 7;
        }

        if (SceneManager.GetActiveScene().name == "Level 5")
        {
            totalOrders = currentTotalOrders = 2;
            // set the initial custom order
            customOrder = 7;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // remove the bomb block and negative block from the order pool
        for (int i = 0; i < SpawnBlock.instance.Blocks.Length; i++)
        {
            if (!SpawnBlock.instance.Blocks[i].name.Contains("Bomb") && !SpawnBlock.instance.Blocks[i].name.Contains("Negative"))
                Blocks = Blocks.Append(SpawnBlock.instance.Blocks[i]).ToArray();
        }
    }

    public void EnableOrders()
    {
        currentOrderText.enabled = true;
        totalOrdersText.enabled = true;
    }

    public void UpdateOrder(string completedOrder)
    {
        // if an order is completed
        if (currentOrder.name == completedOrder)
        {
            // update the displayed order count
            currentOrders--;
            currentOrderText.text = "Order:\tX" + currentOrders;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentOrders <= 0)
        {
            // update the total order count & the delete the previous current order
            if (currentOrder != null)
            {
                // update the current total orders
                currentTotalOrders--;

                // delete the previous current order
                Destroy(currentOrder);
            }

            // set the current orders to 5
            currentOrders = 5;

            currentOrderText.text = "Order:\tX" + currentOrders;

            // update the total orders text
            totalOrdersText.text = "Completed Orders: " + currentTotalOrders + "/" + totalOrders;

            // create the current order
            if (customOrder == -1)
            {
                // create a random order
                currentOrder = Instantiate(Blocks[Random.Range(0, Blocks.Length)], gameObject.transform.position, Quaternion.identity);
            }
            else
            {
                // create a custom order
                currentOrder = Instantiate(Blocks[customOrder], gameObject.transform.position, Quaternion.identity);

                // increase the customOrder variable by 1 to move onto the next order in the list of blocks
                if (customOrder != Blocks.Length - 1)
                {
                    customOrder++;
                }
            }

            // delete the next block's scripts
            MonoBehaviour[] scripts = currentOrder.transform.GetChild(0).GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                Destroy(script);
            }

            // make the current order a child of the order manager
            currentOrder.transform.parent = gameObject.transform;
        }

        if (currentTotalOrders <= 0)
        {
            // pause the game
            PauseManager.instance.PauseGame();

            // show the continue button
            buttonUI.instance.ToggleButton(2);

            // disable the order manager
            gameObject.SetActive(false);
        }
    }
}
