using UnityEngine;

public class ClickScript : MonoBehaviour
{
    public string id = "NO_ID";
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

    void OnMouseDown()
    {
        gm.ClickCard(this.id);
    }
}
