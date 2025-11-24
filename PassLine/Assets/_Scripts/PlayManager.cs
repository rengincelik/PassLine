
// using UnityEngine;

// public class GameManager : MonoBehaviour
// {
//     public static GameManager Instance { get; private set; }

//     [Header("Game State")]
//     public bool isPlaying = false;

//     [Header("Score")]
//     public int currentScore = 0;

//     private void Awake()
//     {
//         // Singleton setup
//         if (Instance == null)
//         {
//             Instance = this;
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     private void Start()
//     {
//         StartGame();
//     }

//     public void StartGame()
//     {
//         isPlaying = true;
//         currentScore = 0;
//         Debug.Log("Game Started");
//     }

//     public void AddScore()
//     {
//         currentScore++;
//         Debug.Log("Score: " + currentScore);
//     }

//     public void GameOver()
//     {
//         isPlaying = false;
//         Debug.Log("Game Over! Final Score: " + currentScore);
//     }

//     public void RestartGame()
//     {
//         // Scene'i reload etmek yerine basit restart
//         currentScore = 0;
//         isPlaying = true;
//         Debug.Log("Game Restarted");
//     }
// }
