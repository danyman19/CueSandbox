using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    private GameManager gm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // public GameManager.Data GetData (string id)
    // {
    //     return new GameManager.Data(1, "hi", "hi");
    // }
}