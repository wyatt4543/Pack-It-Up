using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrderManager : MonoBehaviour
{
    public static OrderManager instance;

    // text objects
    private TextMeshProUGUI currentOrderText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (SceneManager.GetActiveScene().name == "Level 1")
        {
            currentOrderText = GameObject.Find("Canvas/CurrentOrder").GetComponent<TextMeshProUGUI>();
            currentOrderText.enabled = false;
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
