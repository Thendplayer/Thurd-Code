using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager instance = null;

    public List<bool> L_collectedHearts = new List<bool>();
    public List<bool> L_levelsUnlockeds = new List<bool>();

    public uint v_numOfFragmentsForHealthContainer;
    public uint v_currentNumOfHealthContainers;

    public uint v_currentHealth;

    #region Singelton
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }

        #region Initialize Hearts
        for (int i = 0; i < v_currentNumOfHealthContainers; i++)
        {
            L_collectedHearts.Add(false);
        }
        #endregion
    }
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) //DEBUG
        {
            ReloadLevel();
        }
        if (Input.GetKeyDown(KeyCode.F1)) //DEBUG
        {
            LoadScene(LevelManager.instance.v_levelNum - 1);
        }
        if (Input.GetKeyDown(KeyCode.F2)) //DEBUG
        {
            LoadNextLevel();
        }
    }

    #region SceneManager

    public void LoadScene(int lv)
    {
        if (SceneManager.sceneCountInBuildSettings > lv && lv >= 0)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(lv);
        }
    }

    public void LoadScene(string lv)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(lv);
    }

    public void ReloadLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        if (SceneManager.sceneCountInBuildSettings > LevelManager.instance.v_levelNum + 1)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(LevelManager.instance.v_levelNum + 1);
        }
    }

    public void LoadPreviousLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(LevelManager.instance.v_levelNum - 1);
    }

    #endregion

    #region LifeManager

    public void AddHeartContainer()
    {
        v_currentHealth += 4;
        v_currentNumOfHealthContainers++;
    }

    public void SubstractHeartContainer()
    {
        v_currentHealth -= 4;
        v_currentNumOfHealthContainers--;
    }

    public void AddFragmentOfHeart()
    {
        LevelManager.instance.v_currentHeartFragmentsCollected++;

        if (LevelManager.instance.v_currentHeartFragmentsCollected == LevelManager.instance.v_numberOfHeartsSpawned)
        {
            AddHeartContainer();
        }
    }

    public void SubstractFragmentOfHeart()
    {
        if (LevelManager.instance.v_currentHeartFragmentsCollected != 0)
        {
            LevelManager.instance.v_currentHeartFragmentsCollected--;
        }
    }

    public void DecreaseCurrentHealth()
    {
        v_currentHealth--;
    }

    #endregion
}
