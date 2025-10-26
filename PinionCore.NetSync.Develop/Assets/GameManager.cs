using UnityEngine;

public class GameManager : MonoBehaviour
{
    public UnityEngine.UI.Text scoreText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreText.text = "Score: 0";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
