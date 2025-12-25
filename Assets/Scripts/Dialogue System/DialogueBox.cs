using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using System.Globalization;

public enum GameState
{
    Overworld,
    BattleScene
}






[Serializable]
public class DialogueContent
{
    public Dialogue dialogue;
    //public bool choice = false;

    public DialogueContent(Dialogue dialogue)
    {
        this.dialogue = dialogue;

    }
    public bool WillBeChoices()
    {
        if (dialogue == null) { return false; }
        if (dialogue.DialogueChoices == null) { return false; }
        if (dialogue.DialogueChoices.Length == 0) { return false; }
        if (dialogue.GetUsableChoicesList() == null) { return false; }
        if (dialogue.GetUsableChoicesList().Length <= 1) { return false; }
        return true;
    }
}


public class LineInfo
{
    public string lineId;
    public string line;
    public string talkerName;
    public string portraitPath;
    public bool skipAtEnd;
    public bool voiced = false;
    public AudioClip audioClip;

    public LineInfo(string lineId)
    {
        this.lineId = lineId;
        this.line = LanguageData.GetDataById(lineId).GetValueByKey("line");
        this.talkerName = LanguageData.GetDataById(lineId).GetValueByKey("talkerName");
        this.skipAtEnd = line.Contains("<skipLine>");
        this.portraitPath = LanguageData.GetDataById(lineId).GetValueByKey("portraitPath");
        if (bool.TryParse(LanguageData.GetDataById(lineId).GetValueByKey("voiced"), out bool result))
        {
            voiced = result;
        }



        if (string.IsNullOrEmpty(line))
        {
            line = $"{lineId} ID is missing";
        }
    }



}



public class DialogueBox : MonoBehaviour
{

    public Queue<UnityAction> OnDialogueOverAction = new Queue<UnityAction>();
    GameState currentState = GameState.Overworld;
    [Header("References")]
    [SerializeField] GameObject choiceBox;
    [SerializeField] CanvasGroup group;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] GameObject portraitContainer;
    [SerializeField] Image portraitImage;
    [SerializeField] AudioSource voiceClipSource;
    [SerializeField] private GameObject nameTextContainer;
    public UnityEvent<bool> OnAutomaticEvent;

    [Header("Prefabs")]
    [SerializeField] GameObject choicePrefab;



    [Header("Attributes")]
    [SerializeField][Range(0.1f, 1)] float apparitionTime = .4f;
    [SerializeField][Range(5, 60)] int textCharPerSecond = 30;



    [Header("InnerAttributes")]
    bool isShowing;
    bool automaticDialogue = false;
    bool active;
    DialogueContent currentDialogue;
    DialogueEvent[] currentDialogueEventList;
    List<DialogueContent> dialogueWaitingLine = new List<DialogueContent>();
    int dialogueIndex = 0; //which line to currently display in the current dialogue object
    bool dontGoNext = false;
    Coroutine showBoxCoroutine;
    Coroutine setTextCoroutine;
    GameObject player = null;

    GameObject Player
    {
        get
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }
            return player;
        }
        set
        {
            player = value;
        }

    }

    public void DontGoNext()
    {
        dontGoNext = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        group.alpha = 0;
        dialogueIndex = 0;
        ToggleAuto(false);
    }

    public IEnumerator WaitForResume()
    {
        yield return MakeBoxAppear(false);
        AddInteractEventToPlayer(false);
    }



    public IEnumerator Resume()
    {

        if (isShowing)
        {
            dontGoNext = false;
            yield return MakeBoxAppear(true);
            AddInteractEventToPlayer(true);
            Interact();
        }
    }

    public void CancelDialogue()
    {
        Debug.Log("Cancel Dialogue");
        ForceStop();

    }
    public void StartDialogueDelayed(Dialogue dialogue, GameObject playerObject, GameObject originObject, GameState state)
    {
        currentDialogueEventList = dialogue.DialogueEvents;
        if (dialogue.OnOverEvent != null)
        {
            OnDialogueOverAction.Enqueue(dialogue.OnOverEvent.Invoke); // Push the Invoke method of UnityAction

        }
        AddOnOverEvents(dialogue);
        currentState = state;
        DialogueContent newDialogue = new DialogueContent(dialogue);


        if (dialogue.dialogueLineIds != null)
        {
            if (dialogue.dialogueLineIds.Length > 0)
            {

                Debug.Log("Added waiting dialogue");
                dialogueWaitingLine.Add(newDialogue);

            }
        }

    }

    private UnityAction GetEventFromIndex(string eventIndex, DialogueEventType type = DialogueEventType.OnOver)
    {
        if (currentDialogueEventList != null)
        {

            foreach (DialogueEvent dialogueEvent in currentDialogueEventList)
            {
                if (dialogueEvent.IndexValue == eventIndex)
                {
                    if (dialogueEvent.EventType == type)
                    {
                        return dialogueEvent.EventAction.Invoke;
                    }
                }
            }
        }
        return null;
    }

    public void StartDialogue(Dialogue dialogue, GameObject playerObject = null, GameObject originObject = null, GameState state = GameState.Overworld)
    {


        if (dialogue.OnOverEvent != null)
        {
            OnDialogueOverAction.Enqueue(dialogue.OnOverEvent.Invoke); // Push the Invoke method of UnityAction

        }
        currentDialogueEventList = dialogue.DialogueEvents;
        AddOnOverEvents(dialogue);

        currentState = state;

        DialogueContent newDialogue = null;
        if (dialogue.dialogueLineIds != null)
        {
            if (dialogue.dialogueLineIds.Length != 0)
            {
                newDialogue = new DialogueContent(dialogue);
            }
        }
        else
        {

            if (dialogue.DialogueChoices.Length != 0)
            {

                if (dialogue.HasOnePossibleChoice())
                {


                    Dialogue choicedialogue = new Dialogue(dialogue.GetUsableChoicesList()[0].NextDialogue);
                    OnDialogueOverAction.Enqueue(choicedialogue.OnOverEvent.Invoke);
                    AddOnOverEvents(choicedialogue);
                    newDialogue = new DialogueContent(choicedialogue);

                }
            }
        }


        if (newDialogue != null)
        {
            if (newDialogue.dialogue.dialogueLineIds.Length > 0)
            {

                if (active)
                {
                    //Debug.Log("Added Dialogue");
                    dialogueWaitingLine.Add(newDialogue);
                }
                else
                {
                    //Debug.Log("Starting Dialogue");
                    dialogueText.text = "";
                    string lineId = newDialogue.dialogue.dialogueLineIds[0];
                    LineInfo lineInfo = new LineInfo(lineId);
                    SetupLine(lineInfo, false);
                    showBoxCoroutine = StartCoroutine(ShowDialogueBoxAlpha(true));

                    if (playerObject == null)
                    {
                        Player = Character.Player.gameObject;
                    }
                    else
                    {

                        Player = playerObject;
                    }
                    if (currentState == GameState.Overworld)
                    {

                        if (Player)
                        {
                            Player.GetComponent<Character>().ChangeState(new InteractingBehaviour());
                            if (originObject)
                            {
                                Player.GetComponent<Character>().LookAt(originObject);
                            }
                        }
                    }

                    AddInteractEventToPlayer(true);


                    currentDialogue = newDialogue;
                    dialogueIndex = 0;
                    active = true;
                }
            }
        }

    }

    private void AddOnOverEvents(Dialogue dialogue)
    {
        UnityAction action = GetEventFromIndex(dialogue.EventIndex);
        if (action != null)
        {
            Debug.Log($"Added {action.Method.Name}");
            OnDialogueOverAction.Enqueue(action);
        }
    }

    private void TriggerInstantOverEvents(Dialogue dialogue)
    {
        if (dialogue == null) { return; }
        UnityAction action = GetEventFromIndex(dialogue.EventIndex, DialogueEventType.Instant);
        if (action != null)
        {
            action.Invoke();
        }
    }

    public IEnumerator SetPortrait(string portraitPath)
    {

        portraitContainer.gameObject.SetActive(!string.IsNullOrEmpty(portraitPath));

        if (!string.IsNullOrEmpty(portraitPath))
        {
            // Load the sprite from Resources folder
            string fullPath = portraitPath; // Assuming the path is relative to the Resources folder
            ResourceRequest request = Resources.LoadAsync<Sprite>(fullPath);

            while (!request.isDone)
            {
                yield return null;
            }

            Sprite portrait = request.asset as Sprite;

            if (portrait == null)
            {
                // Log an error if the sprite failed to load
                Debug.LogWarning("Failed to load sprite at path: " + fullPath);
                // Optionally, list all loaded sprites for debugging
                portraitContainer.gameObject.SetActive(false);

            }
            else
            {
                // Assign the loaded sprite to the portrait image
                portraitImage.sprite = portrait;
            }
        }

    }

    private void SetupLine(LineInfo lineInfo, bool playVoiceLine = true)
    {
        string portraitPath = lineInfo.portraitPath;
        string talkerName = lineInfo.talkerName;

        StartCoroutine(SetPortrait(portraitPath));


        if (lineInfo.voiced && playVoiceLine)
        {
            // Load the audio from Resources folder
            PlayLineVoiceClip(lineInfo.lineId);
        }



        // Set the active state of the name text container based on whether the talker name is provided
        nameTextContainer.SetActive(!string.IsNullOrEmpty(talkerName));

        // Set the text of the name text component to the provided talker name
        nameText.text = string.IsNullOrEmpty(talkerName) ? "" : talkerName;
    }

    public async void PlayLineVoiceClip(string lineId)
    {


        RAudio.StopAllFromBanks(FMOD.Studio.STOP_MODE.IMMEDIATE, "Voices");
        if (!string.IsNullOrEmpty(lineId))
        {
            RAudio.PlayOneShot(lineId);
            Debug.Log("Playing voice line via RAudio");
        }
        return;


        AudioClip voiceLine = await Utils.GetVoiceLine(lineId);

        if (voiceLine != null)
        {
            // Assign the loaded audio to the audio source
            if (voiceClipSource.clip != voiceLine)
            {


                voiceClipSource?.Stop();
                voiceClipSource.clip = voiceLine;
                voiceClipSource.Play();
            }
        }

    }
    public void DoChoice(DSDialogueChoiceData choice)
    {

        //currentDialogue.choice = false;
        ClearChoiceBox();
        choiceBox.SetActive(false);
        Dialogue dialogue = new Dialogue(choice.NextDialogue);
        OnDialogueOverAction.Enqueue(dialogue.OnOverEvent.Invoke);
        AddOnOverEvents(dialogue);

        if (currentDialogue.dialogue != null)
        {

            dialogueWaitingLine.Insert(0, new DialogueContent(dialogue));
        }

        AddNavigateEventToPlayer(false);
        StartNextDialogueWaiting();
    }


    private void FillChoiceBox(DSDialogueChoiceData[] choices)
    {

        ClearChoiceBox();
        AddInteractEventToPlayer(false);
        choiceBox.SetActive(true);
        for (int i = 0; i < choices.Length; i++)
        {
            int number = i;
            DSDialogueChoiceData choice = choices[number];
            ChoiceMenuButton obj = Instantiate(choicePrefab, choiceBox.transform).GetComponent<ChoiceMenuButton>();
            LineInfo choiceLine = new LineInfo(choices[i].Text);
            obj.GetComponent<TMP_Text>().text = choiceLine.line;
            obj.OnSelected.AddListener(delegate { DoChoice(choice); });
            obj.SetMenu(choiceBox.GetComponent<ChoiceMenu>());
            choiceBox.GetComponent<ChoiceMenu>().AddButton(obj);
            // obj.Select();



        }
        choiceBox.GetComponent<ChoiceMenu>().DefaultSelect();
        AddNavigateEventToPlayer(true);
    }

    public void FlipPortrait()
    {
        float side = Mathf.Sign(portraitContainer.transform.localScale.x);
        float nextSide = side * -1f;
        float xPos = side * 28;




    }
    public void ClearChoiceBox()
    {
        choiceBox.GetComponent<ChoiceMenu>().ResetMenu();
        choiceBox.SetActive(false);
    }

    public void Interact()
    {
        int dialogueLength = currentDialogue?.dialogue?.dialogueLineIds?.Length ?? 0;
        if (showBoxCoroutine != null) { return; }
        if (!active) { return; }
        if (currentDialogue == null) { return; }
        if (currentDialogue.dialogue == null) { return; }

        if (dialogueIndex >= dialogueLength)
        {

            TriggerInstantOverEvents(currentDialogue?.dialogue);

            if (dontGoNext)
            {
                dontGoNext = false;
                return;
            }


            DSDialogueChoiceData[] choicesList = currentDialogue.dialogue.GetUsableChoicesList();
            if (choicesList != null)
            {
                if (choicesList.Length == 1)
                {
                    if (choicesList[0].NextDialogue.ConditionRespected())
                    {
                        dialogueWaitingLine.Insert(0, new DialogueContent(new Dialogue(choicesList[0].NextDialogue)));
                        StartNextDialogueWaiting();
                        return;
                    }
                }
                else
                {
                    Debug.Log("Hello?");
                    FillChoiceBox(choicesList);
                    return;
                }
            }
            else if (dialogueWaitingLine.Count > 0)
            {
                StartNextDialogueWaiting();
                return;
            }



            voiceClipSource?.Stop();
        }
        else
        {
            if (!isShowing) { return; }

            if (CurrentLine() != null)
            {
                NextLine();
                return;

            }
        }
        EndDialogue();
        showBoxCoroutine = StartCoroutine(ShowDialogueBoxAlpha(false));
    }

    private JsonData CurrentLine()
    {
        return LanguageData.GetDataById(currentDialogue.dialogue.dialogueLineIds[dialogueIndex]);

    }

    public void StartNextDialogueWaiting()
    {
        currentDialogue = dialogueWaitingLine[0];
        dialogueWaitingLine.RemoveAt(0);
        dialogueIndex = 0;
        Interact();
    }

    public void EndDialogue()
    {
        if (Player)
        {
            AddInteractEventToPlayer(false);
        }

    }

    public bool Interactable = false;
    public bool NavigationBox = false;
    private void Update()
    {
        if (active && isShowing)
        {

            if (Interactable)
            {
                if (InputManager.Instance.GetButtonDown(ButtonName.Interact))
                {
                    Debug.Log("Hello?");
                    Interact();
                }

                if (InputManager.Instance.GetButtonDown(ButtonName.SecondAct))
                {
                    ToggleAuto();
                }
            }
            else if (NavigationBox)
            {
                if (InputManager.Instance.GetAxis2DDown(ButtonName.Move))
                {
                    NavigateMenu(InputManager.Instance.GetAxis2D(ButtonName.Move));
                }
                if (InputManager.Instance.GetButtonDown(ButtonName.Interact))
                {
                    Debug.Log("Hello?");
                    choiceBox.GetComponent<ChoiceMenu>().TriggerSelected();
                }
            }
        }
    }
    public void AddNavigateEventToPlayer(bool addOrRemove)
    {
        NavigationBox = addOrRemove;
        return;



        //Controller battleCharacterComponent = Player.GetComponent<Controller>();
        //if (addOrRemove)
        //{
        //    Debug.Log("Navigate on");
        //    battleCharacterComponent.OnMovementPressed += NavigateMenu;
        //    battleCharacterComponent.OnSelectPressed += choiceBox.GetComponent<ChoiceMenu>().TriggerSelected;

        //}
        //else
        //{
        //    Debug.Log("Navigate off");
        //    battleCharacterComponent.OnMovementPressed -= NavigateMenu;
        //    battleCharacterComponent.OnSelectPressed -= choiceBox.GetComponent<ChoiceMenu>().TriggerSelected;

        //}
    }

    public void AddInteractEventToPlayer(bool addOrRemove)
    {
        Interactable = addOrRemove;
        //bool contains = battleCharacterComponent.AttackContains(Interact);
        //if (addOrRemove)
        //{

        //    if (!contains)
        //    {
        //        battleCharacterComponent.OnSelectPressed += Interact;
        //        battleCharacterComponent.OnReturnPressed += ToggleAuto;
        //    }

        //}
        //else
        //{
        //    if (contains)
        //    {
        //        battleCharacterComponent.OnSelectPressed -= Interact;
        //        battleCharacterComponent.OnReturnPressed -= ToggleAuto;
        //    }

        //}
    }

    private void ToggleAuto()
    {
        ToggleAuto(!automaticDialogue);
    }


    private void ToggleAuto(bool onOff)
    {

        automaticDialogue = onOff;
        OnAutomaticEvent?.Invoke(automaticDialogue);
        //Debug.Log($"Automatic Dialogue Toggled : {automaticDialogue}");
    }


    public void NavigateMenu(Vector2 direction)
    {
        Debug.Log("Navigating");
        ChoiceMenu menu = choiceBox.GetComponent<ChoiceMenu>();
        if (menu != null)
        {
            if (direction.y > 0)
            {
                menu.PreviousButton();
            }
            if (direction.y < 0)
            {
                menu.NextButton();
            }
        }
    }


    private void NextLine()
    {
        AddInteractEventToPlayer(true);
        DialogueText(currentDialogue.dialogue, dialogueIndex);
    }



    private void ResetBox()
    {


        Interactable = false;
        NavigationBox = false;
        dialogueIndex = 0;
        dialogueText.text = "";
        active = false;
        currentDialogue = null;
    }




    private void DialogueText(Dialogue dialogue, int index)
    {
        if (index < dialogue.dialogueLineIds.Length)
        {

            string lineId = dialogue.dialogueLineIds[index];
            LineInfo lineInfo = new LineInfo(lineId);

            if (dialogue.source != null)
            {
                lineInfo.line = BranchingDialogueStarterObject.GetFormattedLines(dialogue.source, lineInfo.line);

            }



            if (setTextCoroutine != null)
            {
                if (!lineInfo.skipAtEnd)
                {
                    Debug.Log("Linke SKipped WHREN???");
                    StopCoroutine(setTextCoroutine);
                    setTextCoroutine = null;
                    SetDialogueText(lineInfo);
                    dialogueIndex++;
                }

            }
            else
            {
                voiceClipSource.Stop();
                SetupLine(lineInfo);
                setTextCoroutine = StartCoroutine(GraduallySetText(lineInfo));

            }
        }
    }

    private void SetDialogueText(LineInfo info)
    {
        string text = info.line;
        string line = "";
        // Define the pattern to match <anything> or any word without tags
        string pattern = @"<[^>]+>|[^<\s]+";

        // Match all words and tags
        MatchCollection matches = Regex.Matches(text, pattern);

        foreach (Match match in matches)
        {
            string wordOrTag = match.Value;

            // If the text contains a tag, append it instantly
            if (Regex.IsMatch(wordOrTag, @"<[^>]+>"))
            {
                var secmatch = Regex.Match(wordOrTag, @"<waitSec=([\d\.]+)>");
                if (!secmatch.Success)
                {
                    line += wordOrTag;
                }
            }
            else
            {
                // Gradually append each character of the word
                for (int j = 0; j < wordOrTag.Length; j++)
                {
                    line += wordOrTag[j];
                }

                // Add a space after the word if the next match is not a closing tag
                if (match.NextMatch().Success && !match.NextMatch().Value.StartsWith("</"))
                {
                    line += " ";
                }
            }
        }
        dialogueText.SetText(line);
    }

    public bool IsActive()
    {
        return active;
    }

    public void ForceStop()
    {

        if (showBoxCoroutine != null)
        {
            StopCoroutine(showBoxCoroutine);
        }
        if (setTextCoroutine != null)
        {
            StopCoroutine(setTextCoroutine);
        }
        EndDialogue();
        ClearChoiceBox();
        ResetBox();
        isShowing = false;
        group.alpha = 0;



        StartCoroutine(ShowDialogueBoxAlpha(false, true));
    }

    IEnumerator MakeBoxAppear(bool show, bool instant = false)
    {

        float target = show ? 1 : 0;
        float start = group.alpha;
        float duration = 0;
        if (instant)
        {
            duration = apparitionTime;
        }
        while (duration < apparitionTime)
        {
            group.alpha = Mathf.Lerp(start, target, duration / apparitionTime);
            duration += Time.deltaTime;
            yield return null;
        }
        group.alpha = target;


        yield return new WaitForSeconds(apparitionTime);
    }
    IEnumerator ShowDialogueBoxAlpha(bool show, bool instant = false)
    {
        ClearChoiceBox();


        if (isShowing != show)
        {
            isShowing = show;
            if (!isShowing)
            {

                ResetBox();
                while (OnDialogueOverAction.Count > 0)
                {
                    UnityAction currentEvent = OnDialogueOverAction.Dequeue();
                    currentEvent?.Invoke();
                    yield return null;

                }

            }
            yield return MakeBoxAppear(show, instant);

            if (isShowing)
            {
                if (currentDialogue != null)
                {
                    if (!LanguageData.Loaded())
                    {
                        yield return StartCoroutine(LanguageData.LoadAllJsonAsync());
                    }
                    NextLine();
                }
            }
            yield return new WaitForSeconds(.02f);
        }
        showBoxCoroutine = null;
    }

    IEnumerator GraduallySetText(LineInfo info)
    {
        string text = info.line;
        if (!isShowing) { yield break; }
        dialogueText.text = "";
        int index = dialogueIndex;
        yield return TextMatchingCoroutine(text);
        dialogueIndex++;
        bool wasAutomatic = false;
        if (automaticDialogue || info.skipAtEnd)
        {
            wasAutomatic = true;
            if (info.voiced)
            {
                while (voiceClipSource.isPlaying)
                {
                    yield return null;

                    yield return new WaitForSeconds(.2f);
                }
            }
            else
            {
                if (!info.skipAtEnd)
                {
                    yield return new WaitForSeconds(2f);
                }

            }


        }
        bool WillSkipToNext = (automaticDialogue && dialogueIndex == index + 1) || info.skipAtEnd;
        setTextCoroutine = null;

        if (currentDialogue.WillBeChoices())
        {
            Interact();
        }
        else if (WillSkipToNext)
        {
            Interact();
        }


    }

    private IEnumerator TextMatchingCoroutine(string text)
    {
        // Define the pattern to match <anything> or any word without tags
        string pattern = @"<[^>]+>|[^<\s]+";

        // Match all words and tags
        MatchCollection matches = Regex.Matches(text, pattern);

        string line = "";
        foreach (Match match in matches)
        {
            string wordOrTag = match.Value;

            // If the text contains a tag, append it instantly
            if (Regex.IsMatch(wordOrTag, @"<[^>]+>"))
            {
                var secmatch = Regex.Match(wordOrTag, @"<waitSec=([\d\.]+)>");
                if (secmatch.Success)
                {
                    string xValue = secmatch.Groups[1].Value;
                    if (double.TryParse(xValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double x))
                    {
                        yield return new WaitForSeconds((float)x);
                    }
                }
                else
                {
                    line += wordOrTag;
                }
            }
            else
            {
                // Gradually append each character of the word
                for (int j = 0; j < wordOrTag.Length; j++)
                {
                    line += wordOrTag[j];
                    dialogueText.SetText(line);
                    yield return new WaitForSeconds(1f / textCharPerSecond);
                }

                // Add a space after the word if the next match is not a closing tag
                if (match.NextMatch().Success && !match.NextMatch().Value.StartsWith("</"))
                {
                    line += " ";
                }
            }

            dialogueText.SetText(line);
        }

        yield return null;
    }
}




