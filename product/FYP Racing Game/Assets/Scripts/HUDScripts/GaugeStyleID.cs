using UnityEngine;

public class GaugeStyle : MonoBehaviour
{
    [SerializeField]
    private int styleID;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public int getStyleID()
    {
        return styleID;
    }

    public void disableStyle()
    {
        gameObject.SetActive(false);
    }

    public void enableStyle()
    {
        gameObject.SetActive(true);
    }
}
