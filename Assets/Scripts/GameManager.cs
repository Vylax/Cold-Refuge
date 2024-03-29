using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Utils;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    private static GameManager instance;

    // Property to access the singleton instance
    public static GameManager Instance
    {
        get
        {
            // If the instance is null, try to find an existing GameManager in the scene
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                // If no GameManager is found, create a new GameObject and add the GameManager script to it
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("GameManager");
                    instance = singletonObject.AddComponent<GameManager>();
                }
            }

            // Return the instance
            return instance;
        }
    }

    private void Awake()
    {
        // Ensure there is only one instance of GameManager in the scene
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        LoadSettings();

        if (spinWheelPrefab == null)
        {
            // Load the SpinWheel prefab from Resources folder
            spinWheelPrefab = Resources.Load<GameObject>("Prefabs/SpinWheel");

            if (spinWheelPrefab == null)
            {
                Debug.LogError("SpinWheel prefab not found in Resources/Prefabs/SpinWheel.");
            }
        }
    }

    // Game state
    public LogInState logInState = LogInState.Disconnected;
    public CurrScreen currScreen = CurrScreen.Connection;

    public string username = "";

    public int consecutiveLoginDays = -1;
    public bool dailyClaimed = false;
    public int dailyUnlockItemId = -1;
    public bool unlockAnimationPlaying = false;

    public bool buttonActive = true;
    public float buttonCooldown = 2f;

    public float serverTimeout = 10f;

    public GameObject spinWheelPrefab;

    public ScoreSystem scoreSystem;

    private void TryConnect()
    {
        LockButton();
        logInState = LogInState.Connecting;
        StartCoroutine(LogIn(usernameField, passwordField));
    }

    private void TryRegister()
    {
        LockButton();
        StartCoroutine(Register(usernameField, emailField, passwordField));
    }

    private void TryGetDaily()
    {
        LockButton();
        StartCoroutine(GetDaily(username));
    }

    private void TryUnlockReward()
    {
        Instance.GetComponent<AdDisplay>().TryShowAd(() => RewardUnlocked());
    }

    public void RewardUnlocked()
    {
        // TODO Play unlock animation here
        SpawnAndInitializeSpinWheel();

        dailyClaimed = true;
    }

    private IEnumerator LogIn(string username, string password)
    {
        string post_url = $"{logInURL}?username={username}&password={password}";

        // Post the URL to the site and create a download object to get the result.
        WWW hs_post = new WWW(post_url);

        float timeout = serverTimeout; // Set the timeout duration to 10 seconds

        // Wait for either the download to complete or the timeout to occur
        while (!hs_post.isDone && timeout > 0f)
        {
            yield return null;
            timeout -= Time.deltaTime;
        }

        if (timeout <= 0f)
        {
            Debug.LogWarning("Login request timed out");
            logInState = LogInState.Disconnected;
        }
        else
        {
            // Download completed within the timeout duration
            if (!hs_post.text.Contains("compte"))
            {
                Debug.LogWarning("There was an error: " + hs_post.text);
                logInState = LogInState.Disconnected;
            }
            else
            {
                Debug.Log("Logged In successfully");
                logInState = LogInState.Connected;
                this.username = username;
                Connect();
            }
        }

        UnlockButton();
    }

    private IEnumerator Register(string username, string email, string password)
    {
        string post_url = $"{registerURL}?username={username}&email={email}&password={password}";

        // Post the URL to the site and create a download object to get the result.
        WWW hs_post = new WWW(post_url);

        float timeout = serverTimeout; // Set the timeout duration to 10 seconds

        // Wait for either the download to complete or the timeout to occur
        while (!hs_post.isDone && timeout > 0f)
        {
            yield return null;
            timeout -= Time.deltaTime;
        }

        if (timeout <= 0f)
        {
            Debug.LogWarning("Register request timed out");
            logInState = LogInState.Registering;
        }
        else
        {
            // Download completed within the timeout duration
            if (!hs_post.text.Contains("compte"))
            {
                Debug.LogWarning("There was an error: " + hs_post.text);
                logInState = LogInState.Registering;
            }
            else
            {
                Debug.Log("Registered In successfully");
                logInState = LogInState.Connected;
                this.username = username;
                Connect();
            }
        }

        UnlockButton();
    }

    private IEnumerator GetDaily(string username)
    {
        string post_url = $"{dailyURL}?username={username}";

        // Post the URL to the site and create a download object to get the result.
        WWW hs_post = new WWW(post_url);

        float timeout = serverTimeout; // Set the timeout duration to 10 seconds

        // Wait for either the download to complete or the timeout to occur
        while (!hs_post.isDone && timeout > 0f)
        {
            yield return null;
            timeout -= Time.deltaTime;
        }

        if (timeout <= 0f)
        {
            Debug.LogWarning("Daily reward request timed out");
        }
        else
        {
            // Download completed within the timeout duration
            if (!hs_post.text.Contains("entry"))
            {
                Debug.LogWarning("There was an error: " + hs_post.text);
                UnlockButton();
            }
            else
            {
                Debug.Log("Registered In successfully");
                ProcessDailyQueryOutput(hs_post.text);
            }
        }
    }

    void ProcessDailyQueryOutput(string output)
    {
        // Check if the beginning of the string is "New"
        bool isNew = output.StartsWith("New");

        // Extract the username, date, and streak using regex
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(output, @"for user (.+?): Day (\d{4}-\d{2}-\d{2}), Streak (\d+)$");

        if (match.Success)
        {
            // Extract values from the regex match
            string username = match.Groups[1].Value;
            string dateString = match.Groups[2].Value;
            string streakString = match.Groups[3].Value;

            // Convert streakString to an actual integer
            int streak;
            if (int.TryParse(streakString, out streak))
            {
                // Call the method NewDaily
                DisplayDaily(isNew, streak);
            }
            else
            {
                Debug.LogError("Error parsing streak value from PHP output.");
            }
        }
        else
        {
            Debug.LogError("Error extracting data from PHP output.");
        }
        UnlockButton();
    }

    void DisplayDaily(bool isNew, int streak)
    {
        Debug.Log($"isNew: {isNew}, streak: {streak}");
        currScreen = CurrScreen.DailyReward;

        consecutiveLoginDays = streak;
    }


    private void LockButton()
    {
        buttonActive = false;
    }

    private void UnlockButton()
    {
        buttonActive = true;
    }

    private void Connect()
    {
        // update the current screen and eventually query the server for the player's data
        currScreen = CurrScreen.MainMenu;

        // TODO query the server for the player's data here
    }

    private float musicVolume = 50f; // Default value
    private float oldMusicVolume = 50f;
    private bool isMusicMuted = false;

    private float sfxVolume = 50f; // Default value
    private float oldSfxVolume = 50f;
    private bool isSfxMuted = false;

    public float MusicVolume { get { return isMusicMuted ? 0f : musicVolume; } }
    public bool IsMusicMuted { get { return isMusicMuted; } }

    public float SfxVolume { get { return isSfxMuted ? 0f : sfxVolume; } }
    public bool IsSfxMuted { get { return isSfxMuted; } }

    private void SaveSettings()
    {
        // Save music and sfx volumes to PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    private void LoadSettings()
    {
        // Load music and sfx volumes from PlayerPrefs
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 50f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 50f);
    }

    int currentScore;
    int highScore;

    public void GameOver()
    {
        // TODO handle player death here

        // Get the high score from the ScoreSystem
        int[] highScoreArray = scoreSystem.GetHighScore();
        currentScore = highScoreArray[0];
        highScore = highScoreArray[1];

        // Pause game
        Time.timeScale = 0f;

        // Set screen to game over screen
        currScreen = CurrScreen.GameOver;
    }

    public void RestartGame()
    {
        // restart the game

        // Unpause game
        Time.timeScale = 1f;

        // Set screen to game screen
        currScreen = CurrScreen.Game;

        // Reset score
        scoreSystem.ResetScore();

        // Reset Scene
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Reload the current scene
        SceneManager.LoadScene(currentSceneIndex);

        //GameManager.Instance.GetComponent<WorldGenerator>().StartGen();
    }

    // UI

    private Vector3 camPosConnecting = new Vector3(0, 2, -10);
    private float camSizeConnecting = 2f;

    private Vector3 camPosMainMenu = new Vector3(1.5f, -0.5f, -10);
    private float camSizeMainMenu = 6f;

    private Vector3 camPosCharacter = new Vector3(2.5f, -2.25f, -10);
    private float camSizeCharacter = 3.15f;

    private Vector3 camPosGame = new Vector3(-7, -0.5f, -10);
    private float camSizeGame = 12.5f;

    private Vector3 camPosDaily = new Vector3(-4.8f, -1.5f, -10);
    private float camSizeDaily = 2.75f;

    private Vector3 camPosSettings = new Vector3(7, -3.75f, -10);
    private float camSizeSettings = 3.25f;

    private float camDefaultSize = 7f;

    public Vector3 camTargetPosition;
    public float camTargetSize;

    public float camSpeed = 1f;

    public SceneLoader sceneLoader;

    private void Start()
    {
        Camera.main.transform.position = camPosConnecting;
        Camera.main.orthographicSize = camSizeConnecting;
        camTargetPosition = camPosConnecting;
        camTargetSize = camSizeConnecting;

        sceneLoader = FindObjectOfType<SceneLoader>();
        scoreSystem = GetComponent<ScoreSystem>();
    }

    private void Update()
    {
        //check if current scene is still "UItest"
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "UItest")
        {
            // Depending on the current screen, update the camera target position and size
            if (currScreen == CurrScreen.Connection)
            {
                camTargetPosition = camPosConnecting;
                camTargetSize = camSizeConnecting;
            }
            else if (currScreen == CurrScreen.Character)
            {
                camTargetPosition = camPosCharacter;
                camTargetSize = camSizeCharacter;
            }
            else if (currScreen == CurrScreen.Game)
            {
                camTargetPosition = camPosGame;
                camTargetSize = camSizeGame;
            }
            else if (currScreen == CurrScreen.DailyReward)
            {
                camTargetPosition = camPosDaily;
                camTargetSize = camSizeDaily;
            }
            else if (currScreen == CurrScreen.Settings)
            {
                camTargetPosition = camPosSettings;
                camTargetSize = camSizeSettings;
            }
            else if (currScreen == CurrScreen.MainMenu)
            {
                camTargetPosition = camPosMainMenu;
                camTargetSize = camSizeMainMenu;
            }

            // Smoothly move the camera to the target position and size
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, camTargetPosition, camSpeed * Time.deltaTime);
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, camTargetSize, camSpeed * Time.deltaTime);

            if(currScreen != CurrScreen.Connection && currScreen != CurrScreen.MainMenu)
            {
                if (Input.GetKeyDown(KeyCode.Escape) && !unlockAnimationPlaying)
                {
                    currScreen = CurrScreen.MainMenu;
                }
            }
        }
        else
        {
            // reset camera size
            Camera.main.orthographicSize = camDefaultSize;
        }
    }

    void SpawnAndInitializeSpinWheel()
    {
        GameObject spinWheelObject = Instantiate(spinWheelPrefab);
        SpinWheel spinWheel = spinWheelObject.GetComponentInChildren<SpinWheel>();

        if (spinWheel != null)
        {
            unlockAnimationPlaying = true;
            List<int> randomIntegers = GenerateRandomIntegers();
            List<Sprite> spriteList = new List<Sprite>();

            for (int i = 0; i < randomIntegers.Count; i++)
            {
                Sprite sprite = LoadItemSprite(randomIntegers[i]);
                spriteList.Add(sprite);
            }

            Transform roulette = spinWheelObject.transform.Find("Canvas").Find("Roulette");

            if (roulette != null)
            {
                for (int i = 1; i <= 12; i++)
                {
                    Transform slot = roulette.Find("slot" + i);

                    if (slot != null)
                    {
                        Transform itemTransform = slot.Find("item");

                        if (itemTransform != null)
                        {
                             Image itemImage = itemTransform.GetComponent<Image>();

                            if (itemImage != null && i <= spriteList.Count)
                            {
                                itemImage.sprite = spriteList[i - 1];
                                itemTransform.gameObject.name = $"{randomIntegers[i - 1]}";
                            }
                            else
                            {
                                Debug.LogError($"'itemImage' not found as a component of 'item' in 'slot{i}'.");
                            }
                        }
                        else
                        {
                            Debug.LogError($"'item' not found as a child of 'slot{i}'.");
                        }
                    }else
                    {
                        Debug.LogError($"'slot{i}' not found as a child of 'Roulette'.");
                    }
                }
            }else
            {
                Debug.LogError("Roulette not found as a child of the instantiated object.");
            }

            spinWheel.Init(randomIntegers);
        }
        else
        {
            Debug.LogError("SpinWheel component not found on the instantiated object.");
        }
    }

    public IEnumerator SpinWin(int reward, GameObject wheelParent)
    {
        yield return new WaitForSeconds(2f);
        Destroy(wheelParent);
        dailyUnlockItemId = reward;
        unlockAnimationPlaying = false;
    }

    private string usernameField = "";
    private string emailField = "";
    private string passwordField = "";

    public Color color = new Color(0.3529412f, 0.5960785f, 0.7686275f);

    private void OnGUI()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        if(currScreen == CurrScreen.Connection)
        {
            // If the player is not logged in, display the login/registration screen
            if (logInState == LogInState.Disconnected)
            {

                // Set the first section to take 2/3 of the height
                float firstSectionHeight = screenHeight * (2.0f / 3.0f);
                float secondSectionHeight = screenHeight - firstSectionHeight;

                // Adjust font size based on screen height
                int fontSize = Mathf.RoundToInt(screenHeight / 20f);
                GUI.skin.label.fontSize = fontSize;
                GUI.skin.textField.fontSize = fontSize;
                GUI.skin.button.fontSize = fontSize;

                // First Section - "Log in"
                GUI.BeginGroup(new Rect(0, 0, screenWidth, firstSectionHeight));

                // Scale the element height and spacing based on screen height
                float elementHeight = screenHeight / 15f;
                float elementSpacing = screenHeight / 50f;

                usernameField = GUI.TextField(new Rect(10, elementSpacing * 2 + elementHeight, screenWidth - 20, elementHeight), usernameField);

                passwordField = GUI.PasswordField(new Rect(10, elementSpacing * 4 + elementHeight * 3, screenWidth - 20, elementHeight), passwordField, '*');

                GUI.contentColor = color;
                GUI.Label(new Rect(10, elementSpacing, screenWidth - 20, elementHeight), "Username:");
                GUI.Label(new Rect(10, elementSpacing * 3 + elementHeight * 2, screenWidth - 20, elementHeight), "Password:");
                GUI.Label(new Rect(10, elementSpacing * 8 + elementHeight * 6, screenWidth - 20, elementHeight), "OR", GUI.skin.label);
                GUI.contentColor = Color.white;

                if (buttonActive && GUI.Button(new Rect(10, elementSpacing * 6 + elementHeight * 5, screenWidth - 20, elementHeight * 1.5f), "Log In"))
                {
                    // Handle login button click
                    Debug.Log("Log In Clicked");
                    TryConnect();
                }

                GUI.EndGroup();

                // Second Section - "Create an account"
                GUI.BeginGroup(new Rect(0, firstSectionHeight, screenWidth, secondSectionHeight));

                if (GUI.Button(new Rect(10, 10, screenWidth - 20, secondSectionHeight - 20), "Create an Account"))
                {
                    // Handle create account button click
                    Debug.Log("Create Account Clicked");
                    logInState = LogInState.Registering;
                }

                GUI.EndGroup();
            }
            else if (logInState == LogInState.Registering)
            {
                // Set the section to take the entire height
                float sectionHeight = screenHeight;

                // Adjust font size based on screen height
                int fontSize = Mathf.RoundToInt(screenHeight / 20f);
                GUI.skin.label.fontSize = fontSize;
                GUI.skin.textField.fontSize = fontSize;
                GUI.skin.button.fontSize = fontSize;

                // Registration Section
                GUI.BeginGroup(new Rect(0, 0, screenWidth, sectionHeight));

                // Scale the element height and spacing based on screen height
                float elementHeight = screenHeight / 15f;
                float elementSpacing = screenHeight / 50f;

                GUI.Label(new Rect(10, elementSpacing, screenWidth - 20, elementHeight), "Username:");
                usernameField = GUI.TextField(new Rect(10, elementSpacing * 2 + elementHeight, screenWidth - 20, elementHeight), usernameField);

                GUI.Label(new Rect(10, elementSpacing * 4 + elementHeight * 3, screenWidth - 20, elementHeight), "Email:");
                emailField = GUI.TextField(new Rect(10, elementSpacing * 5 + elementHeight * 4, screenWidth - 20, elementHeight), emailField);

                GUI.Label(new Rect(10, elementSpacing * 7 + elementHeight * 6, screenWidth - 20, elementHeight), "Password:");
                passwordField = GUI.PasswordField(new Rect(10, elementSpacing * 8 + elementHeight * 7, screenWidth - 20, elementHeight), passwordField, '*');

                if (buttonActive && GUI.Button(new Rect(10, elementSpacing * 10 + elementHeight * 9, screenWidth - 20, elementHeight * 1.5f), "Register"))
                {
                    // Handle registration button click
                    Debug.Log("Register Clicked");
                    TryRegister();
                }

                GUI.EndGroup();
            }
        }
        else if (currScreen == CurrScreen.MainMenu)
        {
            // Adjust font size based on screen height
            int fontSize = Mathf.RoundToInt(screenHeight / 20f);
            GUI.skin.button.fontSize = fontSize;

            // Main Menu Section
            GUI.BeginGroup(new Rect(0, 0, screenWidth, screenHeight));

            // Scale the element height and spacing based on screen height
            float buttonHeight = screenHeight / 15f;
            float buttonSpacing = screenHeight / 50f;

            // Button to play the game
            if (GUI.Button(new Rect(10, buttonSpacing, screenWidth - 20, buttonHeight), "Play Game"))
            {
                // Handle play game button click
                Debug.Log("Play Game Clicked");
                
                currScreen = CurrScreen.Game;

                sceneLoader.LoadSceneWithFade("enemyTest");
            }

            // Button to open character menu
            if (GUI.Button(new Rect(10, buttonSpacing * 2 + buttonHeight, screenWidth - 20, buttonHeight), "Character Menu"))
            {
                // Handle character menu button click
                Debug.Log("Character Menu Clicked");
                currScreen = CurrScreen.Character;
            }

            // Button to open daily reward menu
            if (buttonActive && GUI.Button(new Rect(10, buttonSpacing * 3 + buttonHeight * 2, screenWidth - 20, buttonHeight), "Daily Reward"))
            {
                // Handle daily reward button click
                Debug.Log("Daily Reward Clicked");
                TryGetDaily();
            }

            // Button to open settings
            if (GUI.Button(new Rect(10, buttonSpacing * 4 + buttonHeight * 3, screenWidth - 20, buttonHeight), "Settings"))
            {
                // Handle settings button click
                Debug.Log("Settings Clicked");
                currScreen = CurrScreen.Settings;
            }

            GUI.EndGroup();
        }
        else if (currScreen == CurrScreen.Settings)
        {
            // Adjust font size based on screen height
            int fontSize = Mathf.RoundToInt(screenHeight / 20f);
            GUI.skin.label.fontSize = fontSize;
            GUI.skin.box.fontSize = fontSize;
            GUI.skin.horizontalSlider.fixedHeight = 50;
            GUI.skin.horizontalSliderThumb.fixedHeight = 50;
            GUI.skin.horizontalSliderThumb.fixedWidth = 50;
            //GUI.skin.horizontalSlider.fontSize = fontSize;
            //GUI.skin.toggle.fontSize = fontSize;

            // Settings Menu Section
            GUI.BeginGroup(new Rect(0, 0, screenWidth, screenHeight));

            // Audio Settings Section
            float elementHeight = screenHeight / 15f;
            float elementSpacing = screenHeight / 50f;

            // Music Volume Slider
            GUI.Box(new Rect(10, elementSpacing, screenWidth - 20, elementHeight), "Music Volume");
            musicVolume = GUI.HorizontalSlider(new Rect(10, elementSpacing * 2 + elementHeight, screenWidth - 20, 500), musicVolume, 0f, 100f);
            GUI.Box(new Rect(10, elementSpacing * 3 + elementHeight * 2, screenWidth - 20, elementHeight), "Volume: " + Mathf.RoundToInt(musicVolume));

            // SFX Volume Slider
            GUI.Box(new Rect(10, elementSpacing * 6 + elementHeight * 5, screenWidth - 20, elementHeight), "SFX Volume");
            sfxVolume = GUI.HorizontalSlider(new Rect(10, elementSpacing * 7 + elementHeight * 6, screenWidth - 20, 500), sfxVolume, 0f, 100f);
            GUI.Box(new Rect(10, elementSpacing * 8 + elementHeight * 7, screenWidth - 20, elementHeight), "Volume: " + Mathf.RoundToInt(sfxVolume));

            GUI.EndGroup();

            if(oldMusicVolume != musicVolume || oldSfxVolume != sfxVolume)
            {
                oldMusicVolume = musicVolume;
                oldSfxVolume = sfxVolume;
                SaveSettings();
            }
        }
        else if(currScreen == CurrScreen.DailyReward && !unlockAnimationPlaying && !AdDisplay.adStarted)
        {
            // Adjust font size based on screen height
            int fontSize = Mathf.RoundToInt(screenHeight / 25f);
            GUI.skin.label.fontSize = fontSize;
            GUI.skin.box.fontSize = fontSize;
            GUI.skin.button.fontSize = fontSize;

            // Daily Reward Section
            GUI.BeginGroup(new Rect(0, 0, screenWidth, screenHeight));

            float boxHeight = screenHeight / 10f;
            float buttonHeight = screenHeight / 15f;
            float elementSpacing = screenHeight / 50f;

            // Display consecutive login days
            GUI.Box(new Rect(10, elementSpacing, screenWidth - 20, boxHeight), "Login streak: " + consecutiveLoginDays);

            // Unlock Reward Button
            if(!dailyClaimed)
            {
                if (GUI.Button(new Rect(10, elementSpacing * 2 + boxHeight, screenWidth - 20, buttonHeight), "Claim today's reward"))
                {
                    TryUnlockReward();
                }
            }
            else
            {
                // Display "Reward already claimed" message
                GUI.Box(new Rect(10, elementSpacing * 2 + boxHeight, screenWidth - 20, boxHeight), "Reward already claimed");

                // Display the item icon of the daily reward item
                Texture itemTexture = LoadItemTexture(dailyUnlockItemId);
                if (itemTexture != null)
                {
                    float iconSize = screenHeight / 15f;
                    float textureAspect = (float)itemTexture.width / itemTexture.height;

                    float drawWidth = iconSize;
                    float drawHeight = iconSize / textureAspect;

                    GUI.DrawTexture(new Rect((screenWidth - drawWidth) / 2, elementSpacing * 3 + boxHeight * 2, drawWidth, drawHeight), itemTexture);
                }

            }


            GUI.EndGroup();
        }
        else if (currScreen == CurrScreen.GameOver)
        {
            // Adjust font size based on screen height
            int fontSize = Mathf.RoundToInt(screenHeight / 20f);
            GUI.skin.label.fontSize = fontSize;
            GUI.skin.box.fontSize = fontSize;
            GUI.skin.button.fontSize = fontSize;

            // Game Over Screen
            GUI.BeginGroup(new Rect(0, 0, screenWidth, screenHeight));

            float boxHeight = screenHeight / 10f;
            float buttonHeight = screenHeight / 15f;
            float elementSpacing = screenHeight / 50f;

            // Display High Score
            GUI.Box(new Rect(10, elementSpacing, screenWidth - 20, boxHeight), "HighScore: " + highScore);

            // Display Current Score
            GUI.Box(new Rect(10, elementSpacing * 2 + boxHeight, screenWidth - 20, boxHeight), "Score: " + currentScore);

            // Play Again Button
            if (GUI.Button(new Rect(10, elementSpacing * 3 + boxHeight * 2, screenWidth - 20, buttonHeight), "Play Again"))
            {
                // Add logic to restart the game
                Debug.Log("Play Again button clicked");
                RestartGame();
            }

            // Return to Main Menu Button
            if (GUI.Button(new Rect(10, elementSpacing * 4 + boxHeight * 3, screenWidth - 20, buttonHeight), "Main Menu"))
            {
                // Add logic to return to the main menu
                Debug.Log("Return to Main Menu button clicked");
                currScreen = CurrScreen.MainMenu;

                // Unpause game
                Time.timeScale = 1f;

                // Reset score
                scoreSystem.ResetScore();

                SceneManager.LoadScene("UItest");
            }

            GUI.EndGroup();
        }
    }

}
