using System;
using System.Collections;
using System.Collections.Generic;

using Ink.Runtime;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    public static event Action<DialogueBox> DialogueContinued;
    public static event Action<DialogueBox, int> ChoiceSelected;

    #region Inspector

    [SerializeField] private TextMeshProUGUI dialogueSpeaker;

    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private Button continueButton;

    [Header("Choices")]

    [SerializeField] private Transform choicesContainer;

    [SerializeField] private Button choiceButtonPrefab;

    #endregion

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    #region Unity Event Functions

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        continueButton.onClick.AddListener(() =>
        {
            DialogueContinued?.Invoke(this);
        });
    }

    private void OnEnable()
    {
        dialogueSpeaker.SetText(string.Empty); // ""
        dialogueText.SetText(string.Empty);
    }

    #endregion

    public void DisplayText(DialogueLine line)
    {
        if (line.speaker != null)
        {
            dialogueSpeaker.SetText(line.speaker);
        }

        dialogueText.SetText(line.text);

        // Read out other information such as a speaker image;

        DisplayButtons(line.choices);
    }

    private void DisplayButtons(List<Choice> choices)
    {
        Selectable newSelection;

        if (choices == null || choices.Count == 0)
        {
            ShowContinueButton(true);
            ShowChoices(false);
            newSelection = continueButton;
        }
        else
        {
            ClearChoices();
            List<Button> choiceButtons = GenerateChoices(choices);

            ShowContinueButton(false);
            ShowChoices(true);
            newSelection = choiceButtons[0];
        }

        StartCoroutine(DelayedSelect(newSelection));
    }

    private IEnumerator DelayedSelect(Selectable newSelection)
    {
        //yield return new WaitForSeconds(0.1f);
        yield return null; // Wait for next Update() / next frame

        newSelection.Select();
    }

    private void ClearChoices()
    {
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private List<Button> GenerateChoices(List<Choice> choices)
    {
        List<Button> choiceButtons = new List<Button>();

        for (int i = 0; i < choices.Count; i++)
        {
            Choice choice = choices[i];

            Button button = Instantiate(choiceButtonPrefab, choicesContainer);

            button.onClick.AddListener(() =>
            {
                ChoiceSelected?.Invoke(this, choice.index);
            });

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.SetText(choice.text);
            button.name = choice.text;

            choiceButtons.Add(button);
        }

        return choiceButtons;
    }

    private void ShowContinueButton(bool show)
    {
        continueButton.gameObject.SetActive(show);
    }

    private void ShowChoices(bool show)
    {
        choicesContainer.gameObject.SetActive(show);
    }
}
