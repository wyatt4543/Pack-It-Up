using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Dialogue : MonoBehaviour
{
    public static Dialogue instance;
    public TextMeshProUGUI textComponent;
    public TextMeshProUGUI continueText;
    public string[] dialogLines;
    public float textSpeed;

    private int index;

    public bool DialogueActive { get; private set; }

    // input variables
    private PlayerInput playerInput;
    private GameObject Spawner;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (SceneManager.GetActiveScene().name == "Level 1")
        {
            dialogLines = new string[] {
                "Worker: You must be new here! Let me teach you how to stack some crates!",
                "Worker: First, there's these food crates. All of them stack together in a grid-like pattern.",
                "Worker: To adjust the crate use the \"A\" or \"D\" key, and the \"W\" key to turn it clockwise or z to turn it counterclockwise. Try it out!"
            };

            RestartDialogue();
        }

        if (SceneManager.GetActiveScene().name == "Level 2")
        {
            dialogLines = new string[] {
                "Worker: There are two new things in the level.",
                "Worker: A bomb which destroys all adjacent squares.",
                "Worker: And a negative block which removes the squares which it overlaps.",
                "Worker: The negative block only activates when it hits the bottom of the game board, or all of its squares overlap other squares.",
                "Worker: Now, get to completing those orders!"
            };

            RestartDialogue();
        }

        if (SceneManager.GetActiveScene().name == "Level 3")
        {
            dialogLines = new string[] {
                "Worker: There are some new materials that are used for concrete.",
                "Worker: The first is water, and it falls whenever it is placed.",
                "Worker: The second is gravel, it is used as the base ingredient for the concrete.",
                "Worker: The gravel causes crates and other materials around it to fall when it is placed.",
                "Worker: Get to delivering the materials needed for concrete!"
            };

            RestartDialogue();
        }

        if (SceneManager.GetActiveScene().name == "Level 4")
        {
            dialogLines = new string[] {
                "Worker: In this level there is some special deliveries that need to be done.",
                "Worker: Some of the orders need to be put into a box.",
                "Worker: This box covers the items and increases the number of times you have to deliver the square covered by the box.",
                "Worker: There are also crates that need to be delivered the amount of times listed on the box.",
                "Worker: Good luck fulfilling these new orders!"
            };

            RestartDialogue();
        }

        if (SceneManager.GetActiveScene().name == "Level 5")
        {
            dialogLines = new string[] {
                "Worker: In this level there are some weird crates.",
                "Worker: The half crate only delivers the half of the row that it is in.",
                "Worker: The crate with a hand on it is moved and rotated with your mouse.",
                "Worker: Good luck with these new deliveries!"
            };

            RestartDialogue();
        }

        if (SceneManager.GetActiveScene().name == "Level 6")
        {
            dialogLines = new string[] {
                "Worker: This is the final level.",
                "Worker: There is a new split crate that has two separate crates moving as one crate.",
                "Worker: Use what I have taught you to deliver all of the requested orders.",
                "Worker: Good luck!"
            };

            RestartDialogue();
        }
    }

    public void Start()
    {
        // find the spawner game object
        Spawner = GameObject.Find("Spawner");

        // Initialize player input
        playerInput = Spawner.GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        // stop the dialog from hogging control over the clicking
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (playerInput.actions["ContinueDialog"].WasReleasedThisFrame() && !PauseManager.instance.IsPaused)
        {
            if (textComponent.text == dialogLines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = dialogLines[index];
                continueText.enabled = true;
            }
        }
    }

    // reset the dialogue
    public void RestartDialogue() {
        DialogueActive = true;
        textComponent.text = string.Empty;
        continueText.enabled = false;
        StartDialogue();
    }

    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        // type each character in the dialogue 1 character at a time
        foreach (char c in dialogLines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        continueText.enabled = true;
    }

    void NextLine()
    {
        if (index < dialogLines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            continueText.enabled = false;
            StartCoroutine(TypeLine());
        }
        else
        {
            DialogueActive = false;
            gameObject.SetActive(false);
        }
    }
}
