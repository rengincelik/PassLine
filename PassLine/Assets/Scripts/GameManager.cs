
namespace PassLine.Assets.Scripts
{
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, GameOver }
    public GameState currentState { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentState = GameState.Playing;
    }

    public void GameOver()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.GameOver;
        Debug.Log("Game Over!");

        // 1 saniye bekleyip restart
        Invoke(nameof(RestartGame), 1f);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsPlaying()
    {
        return currentState == GameState.Playing;
    }
}

}

