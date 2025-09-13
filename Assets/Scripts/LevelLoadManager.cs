using UnityEngine;

public class LevelLoadManager : MonoBehaviour
{
    [SerializeField] private GameObject[] LevelPrefabs;
    private GameObject[] LoadedLevels;
    public int currntLevel = 0;

    private void Awake()
    {
        LoadedLevels = new GameObject[LevelPrefabs.Length];
        AddAllLevels();
    }


    [ContextMenu("AddAllLevels")]
    private void AddAllLevels()
    {
        for (int i = 0; i < LevelPrefabs.Length; i++)
        {
            LoadedLevels[i] = Instantiate(LevelPrefabs[i]);
        }
    }

    [ContextMenu("RestartLevel")]
    private void RestartLevel()
    {
        if (LoadedLevels[currntLevel] != null)
        {
            Destroy(LoadedLevels[currntLevel]);
        }
        LoadedLevels[currntLevel] = Instantiate(LevelPrefabs[currntLevel]);
    }
}
