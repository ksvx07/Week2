using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EndingGame : MonoBehaviour
{
    [SerializeField] private GameObject endingGameUI;
    [SerializeField] private Button quitBtn;

    private void Start()
    {
        quitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        endingGameUI.SetActive(false); // ���� �� UI ��Ȱ��ȭ
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Time.timeScale = 0f; // ���� �Ͻ�����
            endingGameUI.SetActive(true); // ���� UI Ȱ��ȭ
        }
    }
}
