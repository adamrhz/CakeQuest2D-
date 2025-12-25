using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class BattleManager : StateMachine<BattleState>
{
    private static BattleManager _singleton;
    public static BattleManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
            {

                _singleton = value;
            }
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(BattleManager)} instance already exists. Destroying duplicate!");
                Destroy(value.gameObject);
            }
        }
    }



    public bool FastCombats;

    [SerializeField] TMP_Text DebugBattleStateText;
    [SerializeField] TMP_Text battleControlText;
    [SerializeField] TMP_Text battleIndicationText;



    public bool isObserving = false;
    public BattleCharacter observationTarget;

    public ActorInfoPanel actorInfoPanel;


    public GameObject BattlePrefab;
    [SerializeField] GameObject CursorPrefab;



    public GameObject KOKUSEN;
    public GameObject KOKUSENSPEEDLINES;
    public GameObject KOKUSENAURA;
    public GameObject CookingMiniGamePrefab;




    [SerializeField] GameObject currentBackground;
    [SerializeField] GameObject fadeBackground;



    [SerializeField] GameObject CardUIPrefab;
    [SerializeField] Transform CardUIContainer;
    [SerializeField] Transform PlayerSpawnPoint;
    [SerializeField] Transform EnemySpawnPoint;
    public BattleInfoHolder currentBattleInfo;



    private List<GameObject> currentCursor = new List<GameObject>();
    public BattleTimeline timeline;


    public BattleCharacter currentActor;



    public PlayerStorage infoStorage;
    public CharacterInventory playerInventory;
    public float minimumFadeTime = 1f;
    public static float DestroyTime = 1.4f;
    [SerializeField] Party HeroParty;
    [SerializeField] Party EnemyParty;
    public List<BattleCharacter> HeroPartyActors;
    public List<BattleCharacter> EnemyPartyActors;
    public List<BattleCharacter> Actors;
    int turn = 0;
    int numberOfturnsTotal = 0;
    int numberOfEnemyturnsTotal = 0;
    int numberOfLoops = 0;



    public List<BattleItem> GetPlayerItems()
    {
        List<BattleItem> battleItems = new List<BattleItem>();

        foreach (InventoryItem item in playerInventory.myInventory)
        {
            if (item is BattleItem)
            {
                battleItems.Add((BattleItem)item);
            }
        }


        return battleItems;
    }

    public void SetControlText(string v)
    {
        battleControlText.text = v;
    }
    public void SetIndicationText(string v)
    {
        battleIndicationText.text = v;

    }


    public BattleCharacter GetActor()
    {
        return currentActor;
    }
    public bool NextActorIsSameTeam()
    {
        return Actors[GetNextRealTurnIndex()].GetTeam() == GetActor().GetTeam();
    }
    public bool NextActorCanAct()
    {

        return Actors[GetNextRealTurnIndex()].CanAct();
    }
    public bool IsEnemyTurn()
    {
        return GetActor().GetTeam() == TeamIndex.Enemy;
    }
    public bool NextActorIsPlayer()
    {
        return Actors[GetNextRealTurnIndex()].GetTeam() == TeamIndex.Player;
    }
    public int GetActorIndex(BattleCharacter source)
    {
        return GetPartyOf(source).IndexOf(source);
    }

    private int GetNextRealTurnIndex()
    {
        int firstTurn = turn;
        int nextturn;
        for (nextturn = turn; nextturn <= Actors.Count; nextturn++)
        {


            if (nextturn < Actors.Count)
            {

                if (nextturn != firstTurn)
                {
                    if (!GetActor(nextturn).Entity.isDead)
                    {
                        return nextturn;
                    }
                }
            }
            else
            {

                nextturn = -1;
            }
        }
        return nextturn;
    }

    public bool IsFirstTurn()
    {
        return turn == 0;
    }

    public int GetLoopAmount()
    {
        return numberOfLoops;
    }

    public int GetEnemyTurnAmount()
    {
        return numberOfEnemyturnsTotal;
    }

    public int GetTurnAmount()
    {
        return numberOfturnsTotal;
    }

    private int GetNextTurnIndex()
    {
        int nextturn = turn + 1;
        if (nextturn >= Actors.Count)
        {
            nextturn = 0;
        }
        return nextturn;
    }
    public List<BattleCharacter> GetPossibleTarget()
    {
        return GetPossibleTarget(GetActor().currentCommand);
    }


    public List<BattleCharacter> GetPossibleTarget(Skill a, BattleCharacter Source)
    {
        if (a == null)
        {
            return null;
        }
        Command c = a.GetCommandType();
        c.SetSource(Source);
        return GetPossibleTarget(c);
    }
    public List<BattleCharacter> GetPossibleTarget(Command c)
    {
        if (c == null)
        {
            return new List<BattleCharacter>();
        }

        List<BattleCharacter> possibleTargets = new List<BattleCharacter>();
        TeamIndex sourceTeamIndex = c.Source.GetTeam();

        // Find all characters in the scene

        foreach (BattleCharacter character in Actors)
        {
            TeamIndex characterTeamIndex = character.GetTeam();
            if (c.CanBeTarget(character))
            {

                // Check if the command is friendly or not and add appropriate targets
                if ((c.friendliness == Friendliness.Friendly && characterTeamIndex == sourceTeamIndex) || (c.friendliness == Friendliness.Non_Friendly && characterTeamIndex != sourceTeamIndex) || (c.friendliness == Friendliness.Neutral))
                {
                    possibleTargets.Add(character);
                }
            }
        }

        return possibleTargets;
    }

    public int ObservationTarget()
    {
        if (isObserving)
        {
           return GetEnemyTargetIndex();
        }
        return -1;
    }
    public int GetEnemyTargetIndex()
    {
        if (observationTarget != null)
        {
            if (EnemyPartyActors.Contains(observationTarget))
            {
                return EnemyPartyActors.IndexOf(observationTarget);
            }
        }
        return -1;

    }
    internal bool IsObserving()
    {
        return CurrentState.GetType() == typeof(AnalyzingTargetState);
    }

    public List<BattleCharacter> GetCurrentTarget()
    {
        if (GetActor().currentCommand != null)
        {
            return GetActor().currentCommand.Target;
        }
        return new List<BattleCharacter>();
    }

    public BattleCharacter RandomActor(TeamIndex player)
    {
        foreach (BattleCharacter b in Actors)
        {
            if (b.GetTeam() == player)
            {
                return b;
            }
        }
        return null;
    }


    private BattleCharacter GetActor(int nextturn)
    {
        return nextturn < Actors.Count ? Actors[nextturn] : null;
    }
    public bool IsForcedTurn()
    {
        return GetActor() != Actors[turn];
    }
    public void ForceRecipe(int[] vs)
    {
        int index = vs[0];

        if (index < EnemyPartyActors.Count)
        {

            List<ElementalAttribute> attributes = new List<ElementalAttribute>();

            for (int i = 1; i < vs.Length; i++)
            {
                ElementalAttribute newAttribute = new ElementalAttribute(vs[i], true);
                attributes.Add(newAttribute);
            }




            if (index >= 0)
            {
                BattleCharacter enemy = EnemyPartyActors[vs[0]];
                enemy.SetRecipe(attributes);
            }
            else if (index == -1)
            {
                foreach (BattleCharacter enemy in EnemyPartyActors)
                {

                    enemy.SetRecipe(attributes);
                }
            }
            else if (index == -2)
            {
                foreach (BattleCharacter hero in HeroPartyActors)
                {

                    hero.SetRecipe(attributes);
                }
            }
        }

    }

    internal void RecipeMatchTimer(float v)
    {
        throw new System.NotImplementedException();
    }

    private void Awake()
    {
        Singleton = this;
        Add(new AnalyzingTargetState());
        Add(new ChoosingActionState());
        Add(new PerformActionState());
        Add(new CutsceneState());
        Add(new NothingState());
        Add(new ChoosingSkillState());
        Add(new ChoosingItemState());
        Add(new ChoosingTargetState());
        Add(new SwapingState());
        Add(new CookingState());
    }
    public override void Add(BattleState state)
    {
        state.SetBattleManager(this);
        base.Add(state);
    }


    public override void Set<T>()
    {
        base.Set<T>();
        base.CurrentState.ShowControls();
        if (DebugBattleStateText != null)
        {
            DebugBattleStateText.text = base.CurrentState.GetType().ToString();
        }
    }
    public void AddActor(BattleCharacter actor)
    {
        if (!Actors.Contains(actor))
        {
            Actors.Add(actor);
        }
    }

    private void Start()
    {
        SetupBattle();
    }

    public void SetupBattle()
    {
        ClearStage();
        UICanvas.ForceBordersOff();
        
        SetBattleParty();
        SpawnEveryMember(HeroParty, TeamIndex.Player);
        SpawnEveryMember(EnemyParty, TeamIndex.Enemy);
        SpawnPartyCards();
        SetBattleInfo();

    }

    private void SetBattleParty()
    {
        if (currentBattleInfo)
        {
            if (currentBattleInfo.battleInfo)
            {
                if (currentBattleInfo.battleInfo.FightParty.Count > 0)
                {

                    EnemyParty.SetParty(currentBattleInfo.battleInfo.FightParty);

                }
            }
        }
    }

    public void ClearStage()
    {

        foreach (BattleCharacter obj in Actors)
        {
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }
        }
        Actors.Clear();
    }
    public void SetActorIndex(BattleCharacter source, int targetIndex)
    {
        List<BattleCharacter> party = GetPartyOf(source);

        if (party == null || !party.Contains(source))
        {
            Debug.LogError("The source character is not in the specified party.");
            return;
        }

        // Remove the source character from its current position
        party.Remove(source);

        // Ensure the target index is within the valid range
        targetIndex = Mathf.Clamp(targetIndex, 0, party.Count);

        // Insert the source character at the target index
        party.Insert(targetIndex, source);



        UpdateActorsList();

    }

    public void UpdateActorsList()
    {
        Actors = new List<BattleCharacter>(HeroPartyActors);
        Actors.AddRange(EnemyPartyActors);
    }
    public void SpawnEveryMember(Party party, TeamIndex index)
    {
        foreach (CharacterObject character in party.PartyMembers)
        {
            SpawnCharacter(character, index);
        }
    }
    public void InitializeBattle()
    {
        Actors.Sort((a, b) => b.Entity.Speed.CompareTo(a.Entity.Speed));
        SetActor();
        StartNewTurn();
    }

    public void SetBattleInfo()
    {
        if (currentBattleInfo)
        {
            if (currentBattleInfo.battleInfo)
            {
                PlayOST();
                PlayCutscene();
                SetBackground();
            }
        }

        StartBattle();

    }

    private void SetBackground()
    {
        if (currentBattleInfo.battleInfo.backgroundPrefab != null)
        {
            Destroy(currentBackground.gameObject);
            currentBackground = Instantiate(currentBattleInfo.battleInfo.backgroundPrefab);
        }
    }

    public void PlayCutscene()
    {
        Set<NothingState>();
        timeline.ResetPlayed();
        timeline.SetCutscene(currentBattleInfo.battleInfo.CutsceneForDialogue);
    }

    public void PlayOST()
    {
        if (currentBattleInfo.battleInfo.BattleMusic)
        {
            MusicPlayer.Singleton?.PlaySong(currentBattleInfo.battleInfo.BattleMusic, true);
        }
    }


    private void SpawnPartyCards()
    {
        foreach (BattleCharacter actor in Actors)
        {
            if (actor.GetTeam() == TeamIndex.Player)
            {
                GameObject PartyCard = Instantiate(CardUIPrefab, CardUIContainer);
                PartyCard.GetComponent<PartyCard>().SetPlayerRef(actor);

            }
            else
            {
               // actor.GetComponent<Entity>().OnHealthChange += actor.GetComponent<HealthBarPopUp>().OnHealthChange;
            }
        }
    }


    public void ResetPartyCards()
    {
        foreach (Transform child in CardUIContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void Update()
    {
        CurrentState?.OnUpdate();
        HandleInputs();
    }
    public virtual void HandleInputs()
    {
        if (InputManager.inputManager.GetAxis2DDown(ButtonName.Move))
        {
            CurrentState?.OnNavigate(InputManager.inputManager.GetAxis2D(ButtonName.Move));
        }
        if (InputManager.inputManager.GetButtonDown(ButtonName.Interact))
        {
            CurrentState?.OnSelect();
        }
        if (InputManager.inputManager.GetButtonDown(ButtonName.SecondAct))
        {
            CurrentState?.OnBack();
        }
        if (InputManager.inputManager.GetButtonUp(ButtonName.Interact))
        {
            CurrentState?.OnSelectReleased();
        }
        if (InputManager.inputManager.GetButtonUp(ButtonName.SecondAct))
        {
            CurrentState?.OnBackReleased();
        }
    }
    public void SetCursor(BattleCharacter character, bool resetCursors = true)
    {
        if (resetCursors)
        {
            foreach (GameObject cursor in currentCursor)
            {
                Destroy(cursor);
            }
            currentCursor.Clear();
        }
        if (character)
        {

            GameObject cursor = Instantiate(CursorPrefab, character.transform.position + (Vector3.up * 3.1f), Quaternion.identity, character.transform);
            cursor.GetComponent<Blink>().SetDefaultColor(TeamComponent.TeamColor(character.GetTeam()));
            currentCursor.Add(cursor);
        }
    }




    public Vector3 GetPosition(BattleCharacter battleCharacter)
    {
        Vector3 direction = Vector3.left;
        Vector3 basePosition = PlayerSpawnPoint.position;
        int layerOrder;
        List<BattleCharacter> currentParty = new List<BattleCharacter>();
        if (HeroPartyActors.Contains(battleCharacter))
        {
            currentParty = HeroPartyActors;
            direction = Vector3.left;
            basePosition = PlayerSpawnPoint.position;
        }
        else if (EnemyPartyActors.Contains(battleCharacter))
        {

            currentParty = EnemyPartyActors;
            direction = Vector3.right;
            basePosition = EnemySpawnPoint.position;
        }


        layerOrder = currentParty.IndexOf(battleCharacter) % 2;
        basePosition += direction * currentParty.IndexOf(battleCharacter);
        basePosition += (Vector3.down / 2) * (layerOrder);


        return basePosition;

    }
    public void SpawnCharacter(CharacterObject characterObject, TeamIndex index)
    {
        Vector2 Position = PlayerSpawnPoint.position;
        int FlipIndex = 1;
        GameObject CharacterGameObject = Instantiate(BattlePrefab, Position, Quaternion.identity);
        BattleCharacter battleCharacterObject = CharacterGameObject.GetComponent<BattleCharacter>();
        battleCharacterObject.SetReference(characterObject);
        battleCharacterObject.Entity.OnDead += CheckTeams;
        Actors.Add(CharacterGameObject.GetComponent<BattleCharacter>());
        switch (index)
        {
            case TeamIndex.Player:

                CharacterGameObject.GetComponent<Entity>().LoadReference();
                HeroPartyActors.Add(battleCharacterObject);
                break;
            case TeamIndex.Enemy:
                CharacterGameObject.GetComponent<Entity>().LoadReferenceRefreshed();
                EnemyPartyActors.Add(battleCharacterObject);
                FlipIndex = -1;
                break;

        }
        CharacterGameObject.transform.position = GetPosition(battleCharacterObject);
        battleCharacterObject.Flip(FlipIndex);
        battleCharacterObject.SetTeam(index);
        CharacterGameObject.name = characterObject.characterData.characterName + Actors.Count;









    }

    public List<BattleCharacter> GetPartyOf(BattleCharacter battleCharacter)
    {
        if (HeroPartyActors.Contains(battleCharacter))
        {
            return HeroPartyActors;
        }
        else if (EnemyPartyActors.Contains(battleCharacter))
        {

            return EnemyPartyActors;
        }
        return null;
    }





    public void OnBattleWon()
    {
        currentBattleInfo.ConfirmBattle();
        Debug.Log("Battle Won");
    }

    public void OnBattleLoss()
    {
        Debug.Log("Battle Loss");
    }
    public void CheckTeams()
    {
        Debug.Log("Check Teams");
        if (!CheckTeamAlive(EnemyPartyActors))
        {
            EndBattle(true);
        }
        else if (!CheckTeamAlive(HeroPartyActors))
        {

            EndBattle(false);
        }
    }

    public IEnumerator StartBattleCountDown()
    {
        yield return new WaitForSeconds(.5f);
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(.3f);
        }
        InitializeBattle();
    }
    public void StartBattle()
    {
        StartCoroutine(StartBattleCountDown());
    }

    public void EndBattle(bool won)
    {
        StartCoroutine(EndBattleCountDown(won));
    }
    public bool CheckTeamAlive(Party party)
    {
        foreach (BattleCharacter character in Actors)
        {

            if (party.PartyMembers.Contains(character.GetReference()))
            {
                if (!character.Entity.isDead)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool CheckTeamAlive(List<BattleCharacter> party)
    {
        foreach (BattleCharacter character in party)
        {
            if (!character.Entity.isDead)
            {
                return true;
            }

        }
        return false;
    }

    public void FadeBackground(bool ToBlack, float fadeDuration = .1f, float waitforit = 0)
    {
        StartCoroutine(FadeBackgroundTo(ToBlack, fadeDuration, waitforit));
    }

    public IEnumerator FadeBackgroundTo(bool ToBlack, float fadeDuration = .1f, float waitforit = 0)
    {
        float target = ToBlack ? .8f : 0;
        float start = ToBlack ? 0 : .8f;
        float t = 0;
        SpriteRenderer sr = fadeBackground.GetComponent<SpriteRenderer>();
        if (sr.color.a == target)
        {

            t = fadeDuration;
        }
        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(start, target, (t / fadeDuration));
            sr.color = new Color(0, 0, 0, alpha);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        sr.color = new Color(0, 0, 0, target);

        yield return null;
    }

    public IEnumerator EndBattleCountDown(bool battleWon)
    {
        Time.timeScale = .3f;
        Set<NothingState>();
        foreach (BattleCharacter character in Actors)
        {
            if (character.GetTeam() == TeamIndex.Player)
            {

                character.Entity.Apply();
            }
            yield return null;
        }

        yield return new WaitForSeconds(.5f);
        timeline.ResetPlayed();


        if (battleWon)
        {
            OnBattleWon();
        }
        else
        {
            OnBattleLoss();
        }
        Debug.Log(" BAttle Won : " + battleWon);


        Time.timeScale = 1f;
        yield return new WaitForSeconds(3f);

        MoveToScene();



    }

    public void StartingDialogue()
    {
        Set<CutsceneState>();
        timeline.StartDialogue();
    }

    public void NextTurn()
    {
        turn = GetNextTurnIndex();
        if (turn == 0)
        {
            numberOfLoops++;
        }
        SetActor();

        StartNewTurn();

    }

    public bool CheckCutscene()
    {
        return timeline.HasCutscene();
    }
    public void CutsceneOver()
    {
        StartNewTurn();
    }

    public void DisableOptions(int[] options)
    {


        int index = options[0];

        if (index < HeroPartyActors.Count)
        {





            List<int> op = new List<int>(options);
            op.RemoveAt(0);

            if (index >= 0)
            {
                BattleCharacter Hero = HeroPartyActors[options[0]];
                Hero.SetOptions(op.ToArray());
            }
            else if (index == -1)
            {
                foreach (BattleCharacter Hero in HeroPartyActors)
                {
                    
                Hero.SetOptions(op.ToArray());
            
                }
            }
        }
    }
    public void StartNewTurn()
    {
        if (CheckCutscene())
        {
            timeline.StartCinematic();
        }
        else
        {
            if (GetActor().Entity.isDead)
            {
                GetActor().currentCommand = new DeadCommand();
                GetActor().currentCommand.SetSource(GetActor());
                Set<PerformActionState>();
            }
            else
            {
                foreach(BattleCharacter bc in Actors)
                {
                    if(bc.Entity.isDead == false && bc.GetTeam() != GetActor().GetTeam())
                    {
                        bc.OnEveryTurn();
                    }
                }
                if (GetActor().IsPlayerTeam())
                {

                    Set<ChoosingActionState>();
                }
                else
                {
                    GetActor().currentCommand = GetActor().CreateCommand();
                    GetActor().currentCommand.SetSource(GetActor());
                    GetActor().currentCommand.SetTarget(GetRandomTargets());
                    Set<PerformActionState>();

                }
            }

            if (!GetActor().IsPlayerTeam())
            {
                numberOfEnemyturnsTotal++;
            }
            numberOfturnsTotal++;
        }

    }

    private void SetActor()
    {
        currentActor = Actors[turn];
    }

    public void SetActor(BattleCharacter bc)
    {
        if (Actors.Contains(bc))
        {
            currentActor = Actors[Actors.IndexOf(bc)];
        }

    }

    public void MoveToScene()
    {
        Resources.UnloadUnusedAssets();
        if (infoStorage)
        {

            if (infoStorage.sceneName != SceneManager.GetActiveScene().name)
            {
                infoStorage.forceNextChange = true;
                FadeScreen.AddOnMidFadeEvent(ClearStage);
                FadeScreen.MoveToScene(infoStorage.sceneName);
            }



        }

    }

    private List<BattleCharacter> GetRandomTargets()
    {
        List<BattleCharacter> targets = new List<BattleCharacter>();
        targets.Add(GetPossibleTarget()[Random.Range(0, GetPossibleTarget().Count)]);

        return targets;
    }
}
