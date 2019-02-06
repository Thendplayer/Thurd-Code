using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Canvas : MonoBehaviour
{
    [SerializeField] private Transform i_heartParentObject;
    [SerializeField] private GameObject i_heartPrefab;
    [SerializeField] private Image i_heartImage;

    private List<Image> L_heartContainers = new List<Image>();
    private List<Image> L_hearts = new List<Image>();

    void Start()
    {
        for (int i = 0; i < Manager.instance.v_currentNumOfHealthContainers; i++)
        {
            GameObject newHeart = Instantiate(i_heartPrefab, i_heartParentObject);
            newHeart.SetActive(true);
            newHeart.transform.position = new Vector3(i_heartPrefab.transform.position.x + i_heartImage.sprite.bounds.size.x * i * 0.2f, i_heartPrefab.transform.position.y, i_heartPrefab.transform.position.z);

            L_heartContainers.Add(newHeart.GetComponent<Image>());

            L_hearts.Add(newHeart.transform.GetChild(0).GetComponent<Image>());

            //HARDCODE UNA MICA SOFT PERQUE ES VEGIN BE ELS CORS AL BOSS
            if (LevelManager.instance.v_levelNum == 3)
            {
                Camera.main.orthographicSize = 4;
            }
            //END OF SOFT-HARDCODE
        }
        i_heartPrefab.SetActive(false);
    }

    void Update()
    {
        if (L_heartContainers.Count < Manager.instance.v_currentNumOfHealthContainers)
        {
            #region Instantiate
            GameObject newHeart = Instantiate(i_heartPrefab, i_heartParentObject);

            newHeart.SetActive(true);

            newHeart.transform.position = new Vector3(i_heartPrefab.transform.position.x + i_heartImage.sprite.bounds.size.x * (Manager.instance.v_currentNumOfHealthContainers - 1) * 0.2f, i_heartPrefab.transform.position.y, i_heartPrefab.transform.position.z);

            L_heartContainers.Add(newHeart.GetComponent<Image>());

            L_hearts.Add(newHeart.transform.GetChild(0).GetComponent<Image>());
            #endregion
        }

        uint l_completeCurrentHearts = Manager.instance.v_currentHealth / 4;
        uint l_heartParts = Manager.instance.v_currentHealth % 4;

        for (int i = 0; i < L_heartContainers.Count; i++)
        {
            if (i < l_completeCurrentHearts)
            {
                L_hearts[i].gameObject.SetActive(true);
                L_hearts[i].fillAmount = 1;
            }
            else
                L_hearts[i].gameObject.SetActive(false);
        }

        if (l_heartParts != 0)
        {
            Image l_currentHeart = L_hearts[(int)l_completeCurrentHearts];
            l_currentHeart.gameObject.SetActive(true);
            l_currentHeart.fillAmount = 0.25f * l_heartParts;
        }
    }
}
