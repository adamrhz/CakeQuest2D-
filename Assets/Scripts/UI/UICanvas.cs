using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UICanvas : MonoBehaviour
{

    private static UICanvas _singleton;
    public static UICanvas Singleton
    {
        get
        {
            if (_singleton == null)
            {
                // Load the MusicPlayer prefab from Resources
                GameObject canvasPrefab = Resources.Load<GameObject>("UICanvas");
                if (canvasPrefab != null)
                {
                    GameObject canvasInstance = Instantiate(canvasPrefab);
                    Singleton = canvasInstance.GetComponent<UICanvas>();
                    // Debug.Log("UICanvas Instantiated");
                }
                else
                {
                    Debug.LogError("UICanvas prefab not found in Resources.");
                }
            }
            return _singleton;
        }
        private set
        {
            if (_singleton == null)
            {
                _singleton = value;
            }
            else if (_singleton != value)
            {
                Debug.LogWarning($"{nameof(UICanvas)} instance already exists. Destroying duplicate!");
                Destroy(value.gameObject);
            }
        }
    }

    public static bool Exists()
    {
        return _singleton != null;
    }

    public static bool showBorder = true;
    [SerializeField] UIBorder border;
    [SerializeField] QuestList questList;
    [SerializeField] PartyList partyList;
    [SerializeField] DialogueBox dialogueBox;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] GameObject VideoPauseMenu;
    [SerializeField] ActionIndicatorUI actionIndicator;


    [SerializeField] SkipPanel SkipPanel;

    void Awake()
    {
        if (_singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(Singleton.gameObject);
        }
        else if (_singleton != this)
        {
            Destroy(this.gameObject);
        }
        SetSkipPanel(0);
        SetActionIndicatorUI(false);
    }
    public static void SetSkipPanel(float ratio)
    {
        Singleton.SkipPanel?.SetSkipPanel(ratio);
        
    }
    public static void SetVideoForPlayer(VideoClip clip)
    {
        Singleton?.SetVideoClip(clip);
    }
    public void SetActionIndicatorUI(bool show, string indicationText = "interact")
    {
        actionIndicator?.AppearIndicator(show, indicationText);
    }

    public void SetVideoClip(VideoClip clip)
    {
        videoPlayer.clip = clip;
    }

    public static void PlayVideoRec()
    {
        Singleton.PlayVideoRequest();
    }

    public void PlayVideoRequest()
    {


        if (!videoPlayer.isPlaying)
        {
            dialogueBox.DontGoNext();
            Character.Player.ToggleCutsceneState();
            videoPlayer.prepareCompleted += delegate
            {
                StartCoroutine(StartAnimatedCutscene());
            };
            videoPlayer.Prepare();
        }


    }

    public IEnumerator StartAnimatedCutscene()
    {

        ForceBordersOff();
        yield return dialogueBox.WaitForResume();

        FadeScreen.SetColor(Color.black);
        yield return FadeScreen.Singleton.StartFadeAnimation(true, .8f);
        yield return ShowVideo(true);


        MusicPlayer.Stop();

        InputManager.inputManager.OnPausedPressed = null;
        videoPlayer.loopPointReached += EndVideo;
        InputManager.inputManager.OnPausedPressed += delegate { TogglePauseScreen(); };
        PlayVideo();
        yield return new WaitForSeconds(.15f);
        yield return FadeScreen.Singleton.StartFadeAnimation(false);
        yield return new WaitForSeconds(.1f); //Let the video start yaknow;

    }
    public void AddNavigateEventToPlayer(bool addOrRemove)
    {
        Controller battleCharacterComponent = InputManager.inputManager;
        if (addOrRemove)
        {
            battleCharacterComponent.OnMovementPressed += NavigateMenu;
            battleCharacterComponent.OnSecretSelectPressed += VideoPauseMenu.GetComponent<ChoiceMenu>().TriggerSelected;

        }
        else
        {
            battleCharacterComponent.OnMovementPressed -= NavigateMenu;
            battleCharacterComponent.OnSecretSelectPressed -= VideoPauseMenu.GetComponent<ChoiceMenu>().TriggerSelected;

        }
    }
    public void NavigateMenu(Vector2 direction)
    {
        ChoiceMenu menu = VideoPauseMenu.GetComponent<ChoiceMenu>();
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

    public void TogglePauseScreen()
    {
        TogglePauseScreen(!VideoPauseMenu.activeSelf);
    }

    public void TogglePauseScreen(bool on)
    {
        if (on)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }
        VideoPauseMenu.GetComponent<ChoiceMenu>().DefaultSelect();
        VideoPauseMenu.SetActive(on);
        AddNavigateEventToPlayer(on);
    }

    public IEnumerator EndAnimatedCutscene()
    {


        yield return new WaitForSeconds(.1f); //Let the video end yaknow;



        yield return FadeScreen.Singleton.StartFadeAnimation(true);
        yield return ShowVideo(false);
        yield return dialogueBox.Resume();
        MusicPlayer.Resume();
        videoPlayer.Stop();
        yield return FadeScreen.Singleton.StartFadeAnimation(false);
        Character.Player?.TogglePreviousState();
    }

    public void EndVideo()
    {
        TogglePauseScreen(false);
        videoPlayer.loopPointReached -= EndVideo;
        InputManager.inputManager.OnPausedPressed = null;
        videoPlayer.Pause();
        StartCoroutine(EndAnimatedCutscene());
    }

    public void EndVideo(VideoPlayer source)
    {
        EndVideo();
    }

    public void PlayVideo()
    {
        videoPlayer.targetCamera = Camera.main;
        videoPlayer.Play();
    }

    private IEnumerator ShowVideo(bool showVideo)
    {

        float target = showVideo ? 1 : 0;
        float start = videoPlayer.targetCameraAlpha;
        if (target != start)
        {

            float duration = 0;
            while (duration < .5f)
            {
                videoPlayer.targetCameraAlpha = Mathf.Lerp(start, target, duration / .5f);
                duration += Time.deltaTime;
                yield return null;
            }
            videoPlayer.targetCameraAlpha = target;

        }

    }

    public static void StartDialogueDelayed(Dialogue newDialogue, GameObject playerObject = null, GameObject originObject = null, GameState state = GameState.Overworld)
    {
        Singleton?.dialogueBox.StartDialogueDelayed(newDialogue, playerObject, originObject, state);
    }
    public async static void StartDialogue(Dialogue dialogue, GameObject playerObject = null, GameObject originObject = null, GameState state = GameState.Overworld)
    {
        Singleton?.dialogueBox.StartDialogue(dialogue, playerObject, originObject, state);
    }
    public static void ForceStopDialogue()
    {
        Singleton?.dialogueBox.ForceStop();
    }

    public static bool DialogueBoxIsActive()
    {
        return Singleton.dialogueBox.IsActive();
    }

    public static void CancelCurrentDialogue()
    {

        Singleton?.dialogueBox.CancelDialogue();
    }
    public static void TurnBordersOn(bool on)
    {
        Singleton?.BorderAppear(on);
    }

    public static void ForceBordersOff()
    {
        Singleton?.border.Appear(false);
        Singleton?.questList.Appear(false);
        Singleton?.partyList.Appear(false);
        Singleton?.SetActionIndicatorUI(false);
    }

    public void BorderAppear(bool on)
    {
        showBorder = on;
        StartCoroutine("ShowBorderDelayed", on);
    }


    public IEnumerator ShowBorderDelayed(bool on)
    {
        yield return new WaitForSeconds(.5f);
        if(showBorder == on && Timeline.IsInCutscene != on)
        {
            if(on == false || Timeline.IsInCutscene == false)
            {
                Singleton?.border.Appear(on);
                Singleton?.questList.Appear(on);
                Singleton?.partyList.Appear(on);
            }
        }
    }



    public static void UpdateQuestList()
    {
        Singleton?.questList?.ResetList();
    }

    public static void UpdatePartyList()
    {
        Singleton?.partyList?.UpdateList();
    }
}
