using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField, Range(1, 4)] int startingLives = 4;
    [SerializeField, Min(1)] int levelsToWin = 5;

    public GameObject heart0, heart1, heart2, heart3;

    public bool IsPlaying { get; private set; }
    public int Lives { get; private set; }
    public int Level { get; private set; } = 1;
    public bool IsGameOver { get; private set; }
    public bool Won { get; private set; }

    AlienFleet fleet;
    Spaceship ship;
    GUIStyle titleStyle;
    GUIStyle bodyStyle;
    GUIStyle textStyle;
    GUIStyle buttonStyle;
    int displayedLives = -1;
    float nextDamageTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        if (Instance == null)
            new GameObject("Game Manager").AddComponent<GameManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        fleet = FindObjectOfType<AlienFleet>();
        ship = FindObjectOfType<Spaceship>();
        BindHeartIcons();
        Lives = startingLives;
        RefreshLivesDisplay();
    }

    void BindHeartIcons()
    {
        if (heart0 == null) heart0 = GameObject.Find("Heart");
        if (heart1 == null) heart1 = GameObject.Find("Heart (1)");
        if (heart2 == null) heart2 = GameObject.Find("Heart (2)");
        if (heart3 == null) heart3 = GameObject.Find("Heart (3)");
    }

    void RefreshLivesDisplay()
    {
        if (displayedLives == Lives) return;
        displayedLives = Lives;
        SetHeartVisible(heart0, Lives >= 1);
        SetHeartVisible(heart1, Lives >= 2);
        SetHeartVisible(heart2, Lives >= 3);
        SetHeartVisible(heart3, Lives >= 4);
    }

    static void SetHeartVisible(GameObject heart, bool visible)
    {
        if (heart != null && heart.activeSelf != visible)
            heart.SetActive(visible);
    }

    public void LoseLife()
    {
        if (!IsPlaying || Time.time < nextDamageTime) return;
        nextDamageTime = Time.time + 1.25f;
        Lives = Mathf.Max(0, Lives - 1);
        RefreshLivesDisplay();

        if (Lives == 0)
        {
            EndGame(false);
            return;
        }

        if (ship != null)
            ship.transform.position = new Vector3(0f, -8f, ship.transform.position.z);
    }

    

    public void WaveCleared()
    {
        if (!IsPlaying) return;
        if (Level >= 5)
        {
            EndGame(true);
            return;
        }

        Level++;
        if (fleet != null) fleet.AdvanceWave();
    }

    public void EndGame(bool won)
    {
        IsPlaying = false;
        IsGameOver = true;
        Won = won;
    }

    void OnGUI()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 42,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.white;
            bodyStyle = new GUIStyle(titleStyle)
            {
                fontSize = 22,
                fontStyle = FontStyle.Normal
            };

            textStyle = new GUIStyle(bodyStyle)
            {
                fontSize = 18,
                wordWrap = true
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };
        }

        GUI.Label(new Rect(12, 10, 300, 35), "LIVES  " + Lives, bodyStyle);
        GUI.Label(new Rect(Screen.width - 260, 10, 245, 35), "LEVEL  " + Level, bodyStyle);
        if (IsPlaying) return;

        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);
        string heading = IsGameOver ? (Won ? "SECTOR CLEARED!" : "GAME OVER") : "SPACE INVADERS";
        string description = IsGameOver? (Won ? "Hooray! The galaxy is saved! Thanks to you!" : "We have been beaten.") : "The aliens are about to take over the entire galaxy! Help defeat these invaders!";
        string help = "Arrow keys / A D to move     Space to fire";
        GUI.Label(new Rect(0, Screen.height * 0.34f, Screen.width, 70), heading, titleStyle);
        GUI.Label(new Rect(0, Screen.height * 0.40f, Screen.width, 60), description, textStyle);
        GUI.Label(new Rect(0, Screen.height * 0.47f, Screen.width, 50), help, bodyStyle);

        string buttonLabel = IsGameOver ? "Retry" : "Start Playing";
        float buttonWidth = Mathf.Min(260f, Screen.width - 40f);
        Rect buttonRect = new Rect(
            (Screen.width - buttonWidth) * 0.5f,
            Screen.height * 0.58f,
            buttonWidth,
            58f
        );
        bool clicked = GUI.Button(buttonRect, buttonLabel, buttonStyle);
        Keyboard keyboard = Keyboard.current;
        bool pressedEnter = keyboard != null && keyboard.enterKey.wasPressedThisFrame;
        if (clicked || pressedEnter)
            Restart();
    }

    void Restart()
    {
        Lives = startingLives;
        Level = 1;
        IsGameOver = false;
        Won = false;
        IsPlaying = true;
        nextDamageTime = 0f;
        RefreshLivesDisplay();

        if (fleet == null) fleet = FindObjectOfType<AlienFleet>();
        if (fleet != null)
        {
            foreach (GameObject alien in fleet.aliens)
                if (alien != null) Destroy(alien);
            fleet.aliens.Clear();
            fleet.ResetFleet();
            fleet.SpawnWave();
        }

        if (ship == null) ship = FindObjectOfType<Spaceship>();
        if (ship != null) ship.transform.position = new Vector3(0f, -8f, 0f);

        foreach (Dart dart in FindObjectsOfType<Dart>())
            Destroy(dart.gameObject);
        foreach (AlienBullet bullet in FindObjectsOfType<AlienBullet>())
            Destroy(bullet.gameObject);
    }
}

