using System.Collections.Generic;
using System.Threading.Tasks;
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

    // delivery truck stuff
    [SerializeField] private GameObject deliveryTruck;
    [SerializeField] private AnimationClip[] deliveryTruckAnimationClips;
    private Animator deliveryTruckAnimator;
    private float deliveryTruckSpeed = 20.0f;
    private Vector2 deliveryTruckOffScreenDestination = new Vector2(-29.65f, -4.5f);
    private Vector2 deliveryTruckOnScreenDestination = new Vector2(-27.5f, -4.5f);
    private Vector2 deliveryTruckDestination;
    private bool moveDeliveryTruck = false;
    private bool triggerDeliveryTruckOpen = false;
    private bool firstTruck = true;
    private bool animationPlaying = false;

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

        // assign the delivery truck animator
        deliveryTruckAnimator = deliveryTruck.GetComponent<Animator>();

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

    // function for moving the delivery truck onto the screen
    private void DeliveryTruckShow()
    {
        // update the delivery truck destination to be on screen
        deliveryTruckDestination = deliveryTruckOnScreenDestination;

        // check if the distance to the destination is really close or not
        if (Vector3.Distance(deliveryTruck.transform.position, deliveryTruckDestination) > 0.01f)
        {
            // move the delivery truck
            deliveryTruck.transform.position = Vector3.MoveTowards(deliveryTruck.transform.position, deliveryTruckDestination, deliveryTruckSpeed * Time.deltaTime);
        }
        else
        {
            // stop the delivery truck
            deliveryTruck.transform.position = deliveryTruckDestination;
            moveDeliveryTruck = false;

            //open the delivery truck
            triggerDeliveryTruckOpen = true;
            deliveryTruckAnimator.SetBool("triggerDeliveryTruckOpen", triggerDeliveryTruckOpen);
        }

        // state that the animation is done playing
        animationPlaying = false;
    }

    // function for moving the delivery truck off the screen & back onto the screen
    private async void DeliveryTruckHideAndShow()
    {
        //close the delivery truck
        triggerDeliveryTruckOpen = false;
        deliveryTruckAnimator.SetBool("triggerDeliveryTruckOpen", triggerDeliveryTruckOpen);

        // wait for the close animation to complete
        await Task.Delay(Mathf.RoundToInt(deliveryTruckAnimationClips[2].length * 1000));

        // update the delivery truck destination to be off screen
        deliveryTruckDestination = deliveryTruckOffScreenDestination;

        // check if the distance to the destination is really close or not
        while (Vector3.Distance(deliveryTruck.transform.position, deliveryTruckDestination) > 0.01f)
        {
            // move the delivery truck
            deliveryTruck.transform.position = Vector3.MoveTowards(deliveryTruck.transform.position, deliveryTruckDestination, deliveryTruckSpeed * Time.deltaTime);

            // wait until the truck is in place
            await Task.Yield();
        }

        // stop the delivery truck
        deliveryTruck.transform.position = deliveryTruckDestination;

        // wait 1 second before moving the truck back on screen
        await Task.Delay(1000);

        DeliveryTruckShow();
    }

    // Update is called once per frame
    void Update()
    {
        // move the delivery truck if it is time to do so
        if (moveDeliveryTruck && !animationPlaying)
        {
            // state that the animation is playing
            animationPlaying = true;

            // check if the delivery truck is the first delivery truck
            if (firstTruck)
            {
                // move the delivery truck onto the screen
                DeliveryTruckShow();
            }
            // if it is on screen
            else
            {
                // move the delivery truck off the screen & back onto the screen
                DeliveryTruckHideAndShow();
            }
        }

        if (currentOrders <= 0)
        {
            // enable movement for the delivery truck
            moveDeliveryTruck = true;

            // update the total order count & the delete the previous current order
            if (currentOrder != null)
            {
                // update the current total orders
                currentTotalOrders--;

                // delete the previous current order
                Destroy(currentOrder.gameObject);

                // set first truck to false
                firstTruck = false;
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
