using System;
using System.Collections;
using TMPro;
using UnityEngine;
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
                "Worker: To adjust the crate use the left or right arrow keys, and the up arrow to turn it clockwise or z to turn it counterclockwise. Try it out!"
            };

            RestartDialogue();
        }

        if (SceneManager.GetActiveScene().name == "Level 2")
        {
            dialogLines = new string[] {
                "Worker: There are two new blocks in the level.",
                "Worker: A bomb block which destroys all adjacent blocks.",
                "Worker: And a negative block which removes the squares which it overlaps.",
                "Worker: The negative block only activates when it hits the bottom of the game board, or all of its squares overlap other squares."
            };

            RestartDialogue();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && !PauseManager.instance.IsPaused)
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
