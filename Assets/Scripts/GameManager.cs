using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Dictionary<string, StageScriptableObject> StageDics { get; private set; } = new();

    private void Awake()
    {
        if(null == Instance)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadScriptableObject("Stages");
    }

    private void LoadScriptableObject(string path)
    {
        var stages = Resources.LoadAll<StageScriptableObject>(path);

        StageDics.Clear();

        foreach (var stage in stages)
        {
            StageDics.Add(stage.name, stage);
        }
    }

}
