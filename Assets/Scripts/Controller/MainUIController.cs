using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainUIController : Singleton<MainUIController>
{
    [SerializeField] private GameObject winningObj;
    public void PlayMap(int index)
    {
        GlobalManager.Instance.SetCurrentMap(index);
        SceneManager.LoadScene("Play", LoadSceneMode.Single);
    }
    public void ShowWinningForm()
    {
        if (winningObj != null)
        {
            winningObj.SetActive(true);
        }
    }
    public void BackToStartMenu()
    {
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }
}
