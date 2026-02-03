using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenuCanvasGo;

    private bool isPaused;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _mainMenuCanvasGo.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (MenuInputs.instance.MenuOpenCloseInput)
        {
            if (!isPaused)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }
    }

    #region Pause/Unpause Functions
    
    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        OpenMainMenu();
    }

    public void Unpause()
    {
        isPaused = false;
        Time.timeScale = 1f;

        CloseAllMenus();
    }
    
    #endregion
    
    #region Canvas Activations

    
    public void OpenMainMenu()
    {
        _mainMenuCanvasGo.SetActive(true);
    }
    
    private void CloseAllMenus()
    {
        _mainMenuCanvasGo.SetActive(false);
    }
    
    #endregion
}

