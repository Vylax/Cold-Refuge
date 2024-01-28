using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static Utils;

public class AccountManager : MonoBehaviour
{
    public GameStatus status = GameStatus.MainMenu;

    public string[] buttonsText = {
        "Play",// Play
        "Character",// Character
        "Daily reward",// Daily reward
        "Settings"
    };

    // Toute ces valeures sont relatives à la taille de l'écran
    private float buttonHeight = 3 / 16f;
    private float buttonSpacing = 1 / 20f;
    private float buttonWidth = 1 / 3f;

    private static AccountManager _instance;

    public GameObject player;

    public LogInState logInState = LogInState.Disconnected;

    private string username = "_Unregistered_";
    string usernameField = "";
    string passwordField = "";

    private float buttonCooldown = 3f;
    private bool buttonActive = true;

    private bool showInfo = false;
    private string infoText = "";
    private IEnumerator showInfoInstance;

    List<Score> scores = new List<Score>();

    private float startTime = 0f;

    private Vector2 scrollPosition = Vector2.zero;

    public static AccountManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AccountManager");
                go.AddComponent<AccountManager>();
            }

            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
    }

    private void OnGUI()
    {
        switch (status)
        {
            case GameStatus.MainMenu:
                // 1/4 * 3/4 de la hauteur de l'ecran pour chaque boutons
                // 1/5 * 1/4 de la hauteur de l'ecran pour chaques espaces
                // 1/3 de la largeur de l'écran pour tout
                for (int i = 0; i < 4; i++)
                {
                    if (GUI.Button(new Rect(Screen.width * buttonWidth, (Screen.height * (buttonHeight * i + buttonSpacing * (i + 1))), Screen.width * buttonWidth, Screen.height * buttonHeight), buttonsText[i]))
                    {
                        switch (i)
                        {
                            case 0:
                                //Setup game here
                                StartGame();
                                break;
                            case 1:
                                //display leaderboard
                                //StartCoroutine(GetLeaderboard());
                                //status = GameStatus.Leaderboard;
                                break;
                            default:

                                break;
                        }
                    }
                }

                //login/register interface
                GUI.Box(new Rect(Screen.width * (2 / 3f + 1 / 32f), 30, Screen.width * (1 / 3f - 2 / 32f), Screen.height - 60), "");

                if (logInState == LogInState.Disconnected)
                {
                    Regex rgx = new Regex("[^a-zA-Z0-9 -]");

                    GUI.Label(new Rect(Screen.width * (2 / 3f + 2 / 32f), (Screen.height * (buttonHeight * 1 + buttonSpacing * (1 + 1))), Screen.width * (1 / 3f - 4 / 32f), Screen.height * buttonHeight / 2f), "Username:");
                    usernameField = GUI.TextField(new Rect(Screen.width * (2 / 3f + 2 / 32f), (Screen.height * (buttonHeight * (1 + 1 / 2f) + buttonSpacing * (1 + 1))), Screen.width * (1 / 3f - 4 / 32f), Screen.height * buttonHeight / 2f), usernameField, 15);
                    usernameField = rgx.Replace(usernameField, "");

                    GUI.Label(new Rect(Screen.width * (2 / 3f + 2 / 32f), (Screen.height * (buttonHeight * 2 + buttonSpacing * (2 + 1))), Screen.width * (1 / 3f - 4 / 32f), Screen.height * buttonHeight / 2f), "Password:");
                    passwordField = GUI.PasswordField(new Rect(Screen.width * (2 / 3f + 2 / 32f), (Screen.height * (buttonHeight * (2 + 1 / 2f) + buttonSpacing * (2 + 1))), Screen.width * (1 / 3f - 4 / 32f), Screen.height * buttonHeight / 2f), passwordField, '*', 15);
                    passwordField = rgx.Replace(passwordField, "");

                    if (buttonActive)
                    {
                        if (GUI.Button(new Rect(Screen.width * (2 / 3f + 2 / 32f), (Screen.height * (buttonHeight * (3 - 1 / 8f) + buttonSpacing * (3 + 1))), Screen.width * (1 / 3f - 4 / 32f), Screen.height * buttonHeight / 2f), "Log In"))
                        {
                            StartCoroutine(LogIn(usernameField, passwordField));
                            StartCoroutine(LockButton());
                        }
                        if (GUI.Button(new Rect(Screen.width * (2 / 3f + 2 / 32f), (Screen.height * (buttonHeight * (3 + 1 / 2f) + buttonSpacing * (3 + 1))), Screen.width * (1 / 3f - 4 / 32f), Screen.height * buttonHeight / 2f), "Register"))
                        {
                            //TODO StartCoroutine(Register(usernameField, passwordField));
                            StartCoroutine(LockButton());
                        }
                    }
                    else
                    {
                        GUI.Box(new Rect(Screen.width * (2 / 3f + 2 / 32f), (Screen.height * (buttonHeight * (3 + 1 / 4f) + buttonSpacing * (3 + 1))), Screen.width * (1 / 3f - 4 / 32f), Screen.height * buttonHeight / 2f), "Please wait...");
                    }
                }
                else
                {

                }

                break;
            case GameStatus.InGame:
                //GUI.Box(new Rect(10, 10, 300, 75), $"Health: {Mathf.RoundToInt(player.GetComponent<Player>().health)}\nScore:{Mathf.FloorToInt(player.transform.position.x)}");
                break;
            case GameStatus.Pause:

                break;
            case GameStatus.GameDone:
                //Display score
                //Display leaderboard at player's position
                //start another game button
                //try again on same seed button
                //Go to main menu button
                //Mathf.RoundToInt(player.GetComponent<Player>().health)

                GUI.Box(new Rect(Screen.width * buttonWidth, (Screen.height * (buttonHeight * 0 + buttonSpacing * (0 + 1))), Screen.width * buttonWidth, Screen.height * buttonHeight), $"Score:{Mathf.FloorToInt(player.transform.position.x)}");

                //debug
                if (GUI.Button(new Rect(Screen.width * buttonWidth, (Screen.height * (buttonHeight * 1 + buttonSpacing * (1 + 1))), Screen.width * buttonWidth, Screen.height * buttonHeight), $"Rejouer sur une nouvelle seed"))
                {
                    ResetGame(false);
                }

                if (GUI.Button(new Rect(Screen.width * buttonWidth, (Screen.height * (buttonHeight * 2 + buttonSpacing * (2 + 1))), Screen.width * buttonWidth, Screen.height * buttonHeight), $"Rejouer sur la même seed"))
                {
                    ResetGame(true);
                }

                if (GUI.Button(new Rect(Screen.width * buttonWidth, (Screen.height * (buttonHeight * 3 + buttonSpacing * (3 + 1))), Screen.width * buttonWidth, Screen.height * buttonHeight), $"Retourner au menu"))
                {
                    status = GameStatus.MainMenu;
                }
                break;
            case GameStatus.IsDead:

                break;
            case GameStatus.Leaderboard:

                /*Rect position = new Rect(Screen.width * buttonWidth, 10, Screen.width * buttonWidth + 30, Screen.height - 60);
                scrollPosition = GUI.BeginScrollView(position, scrollPosition, new Rect(0, 0, Screen.width * buttonWidth, (60) * scores.Count));

                for (int i = 0; i < scores.Count; i++)
                {


                    GUI.Box(new Rect(0, ((60 * i)), Screen.width * buttonWidth, 50), $"{(i + 1)}: {scores[i].username} | {scores[i].score} pts | seed:({scores[i].seedX},{scores[i].seedY}) | set seed: {(scores[i].set_seed == 1 ? "yes" : "no")}");

                }

                GUI.EndScrollView();
                if (GUI.Button(new Rect(10, 10, 200, 100), "Menu"))
                {
                    status = GameStatus.MainMenu;
                }*/

                break;
        }
    }

    private void StartGame()
    {
        startTime = Time.time;
        Debug.Log($"Game started at: {startTime}");
        status = GameStatus.InGame;
    }

    private void ResetGame(bool sameSeed = false)
    {
        Debug.Log($"Game was reset at: {Time.time}");

        startTime = Time.time;
        Debug.Log($"Game started at: {startTime}");
        status = GameStatus.InGame;
    }

    public void EndGame()
    {
        Debug.Log($"Game ended at: {Time.time}");

        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

        //sendscore etc.... TODO
        //StartCoroutine(SendScore(Mathf.FloorToInt(player.transform.position.x)));
        //then
        status = GameStatus.GameDone;
    }

    private string secretKey = "DeTouteFaconINTv_C_Overrated890138647460aaa";
    private string addScoreURL = "http://vylax.fr/unity/jeu_blaze/sendscore.php";
    private string registerURL = "http://vylax.fr/coldrefuge/register.php";
    private string logInURL = "http://vylax.fr/unity/coldrefuge/connexion.php";
    private string leaderboardURL = "http://vylax.fr/unity/jeu_blaze/displayscore.php";

    IEnumerator SendScore(int score)
    {

        string post_url = "TODO change this";//$"{addScoreURL}?username={username}&score={score}&seedX={PG.seedX}&seedY={PG.seedY}&set_seed={(PG.setSeed ? 1 : 0)}&key={secretKey}";

        // Post the URL to the site and create a download object to get the result.
        WWW hs_post = new WWW(post_url);
        yield return hs_post; // Wait until the download is done

        if (!hs_post.text.Contains("Les donn"))
        {
            Debug.LogWarning("There was an error sending score: " + hs_post.text);
        }
        else
        {
            Debug.Log("score sent successfully");
        }
    }

    /*IEnumerator GetLeaderboard()
    {

        string post_url = $"{leaderboardURL}?key={secretKey}";

        WWW hs_post = new WWW(post_url);
        yield return hs_post; // Wait until the download is done

        if (hs_post.text.Contains("la clef"))
        {
            infoText = "There was an error while loading the leaderboard";
            Notification();
            Debug.LogWarning(infoText);
        }
        else
        {
            Debug.Log("leaderboard loaded successfully");
            scores = new List<Score>();

            string rawData = hs_post.text;
            while (rawData.Length > 0)
            {
                string temp = GetUntilOrEmpty(rawData);
                rawData = rawData.Remove(0, temp.Length + 1);
                string[] temp2 = temp.Split(';');
                scores.Add(new Score(temp2[0], int.Parse(temp2[1]), float.Parse(temp2[2]), float.Parse(temp2[3]), int.Parse(temp2[4])));
            }
        }
    }*/

    IEnumerator Register(string username, string email, string password)
    {

        string post_url = $"{registerURL}?username={username}?email={email}&password={password}";

        // Post the URL to the site and create a download object to get the result.
        WWW hs_post = new WWW(post_url);
        yield return hs_post; // Wait until the download is done

        if (!hs_post.text.Contains("compte"))
        {
            infoText = "There was an error: " + hs_post.text;
            Notification();
            Debug.LogWarning(infoText);
        }
        else
        {
            infoText = "Registered successfully";
            Notification();
            Debug.Log(infoText);
            logInState = LogInState.Disconnected;
        }
    }

    IEnumerator LogIn(string username, string password)
    {

        string post_url = $"{logInURL}?username={username}&password={password}";

        // Post the URL to the site and create a download object to get the result.
        WWW hs_post = new WWW(post_url);
        yield return hs_post; // Wait until the download is done

        if (!hs_post.text.Contains("compte"))
        {
            infoText = "There was an error: " + hs_post.text;
            Notification();
            Debug.LogWarning(infoText);
        }
        else
        {
            infoText = "Logged In successfully";
            Notification();
            Debug.Log(infoText);
            logInState = LogInState.Connected;
            this.username = username;
        }
    }

    private IEnumerator LockButton()
    {
        if (buttonActive)
        {
            buttonActive = false;
            yield return new WaitForSeconds(buttonCooldown);
            buttonActive = true;
        }
        yield return null;
    }

    private void Notification()
    {
        StopCoroutine(showInfoInstance);
        showInfoInstance = ShowInfo();
        StartCoroutine(showInfoInstance);
    }

    private IEnumerator ShowInfo()
    {
        showInfo = true;
        yield return new WaitForSeconds(5f);
        showInfo = false;
    }
}
