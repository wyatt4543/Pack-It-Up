using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    public static OrderManager instance;
    private int currentOrders = 0, totalOrders, currentTotalOrders;
    public List<GameObject> Blocks;
    private List<GameObject> tempBlocks = new List<GameObject>();
    private GameObject tempOrder;
    private RectTransform currentOrder;
    private int customOrder = -1;

    // orders objects
    private TextMeshProUGUI currentOrderText;
    private TextMeshProUGUI totalOrdersText;
    private Image ordersList;
    private RectTransform ordersHolder;

    private void Awake()
    {
        currentOrderText = GameObject.Find("LevelCanvas/CurrentOrder").GetComponent<TextMeshProUGUI>();
        totalOrdersText = GameObject.Find("LevelCanvas/TotalOrders").GetComponent<TextMeshProUGUI>();
        ordersList = GameObject.Find("LevelCanvas/OrdersList").GetComponent<Image>();
        ordersHolder = GameObject.Find("LevelCanvas/Orders").GetComponent<RectTransform>();

        if (instance == null)
        {
            instance = this;
        }

        if (SceneManager.GetActiveScene().name == "Level 1")
        {
            totalOrders = currentTotalOrders = 2;
            currentOrderText.enabled = false;
            totalOrdersText.enabled = false;
            ordersList.enabled = false;
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

        if (SceneManager.GetActiveScene().name == "Level 6")
        {
            totalOrders = currentTotalOrders = 3;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // remove the bomb block and negative block from the order pool
        for (int i = 0; i < SpawnBlock.instance.Blocks.Length; i++)
        {
            foreach (GameObject Block in Blocks)
            {
                if (SpawnBlock.instance.Blocks[i].name.Contains(Block.name) && !SpawnBlock.instance.Blocks[i].name.Contains("Bomb") && !SpawnBlock.instance.Blocks[i].name.Contains("Negative"))
                {
                    tempBlocks.Add(Block);
                }
            }
        }

        // finish the new block pool
        Blocks = tempBlocks;

        // delete the temp list
        tempBlocks = null;
    }

    public void EnableOrders()
    {
        currentOrderText.enabled = true;
        totalOrdersText.enabled = true;
        ordersList.enabled = true;
    }

    public void UpdateOrder(string completedOrder)
    {
        // if an order is completed
        if (completedOrder.Contains(currentOrder.name))
        {
            // update the displayed order count
            currentOrders--;
            currentOrderText.text = "X" + currentOrders;
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
                Destroy(currentOrder.gameObject);
            }

            // set the current orders to 5
            currentOrders = 5;

            currentOrderText.text = "X" + currentOrders;

            // update the total orders text
            totalOrdersText.text = "Completed Orders: " + currentTotalOrders + "/" + totalOrders;

            // create the current order
            if (customOrder == -1)
            {
                // assign a random number to the block index
                int blockIndex = Random.Range(0, Blocks.Count);

                // create a random order
                tempOrder = Instantiate(Blocks[blockIndex], new Vector2(0, 0), Quaternion.identity);
                
                // get the block name based on index
                tempOrder.name = Blocks[blockIndex].name;
            }
            else
            {
                // create a custom order
                tempOrder = Instantiate(Blocks[customOrder], new Vector2(0, 0), Quaternion.identity);

                // get the block name based on index
                tempOrder.name = Blocks[customOrder].name;

                // increase the customOrder variable by 1 to move onto the next order in the list of blocks
                if (customOrder != Blocks.Count - 1)
                {
                    customOrder++;
                }
            }

            // make the current order a child of the order holder
            currentOrder = tempOrder.GetComponent<RectTransform>();
            currentOrder.SetParent(ordersHolder);
            currentOrder.anchoredPosition = Vector2.zero;
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
