using UnityEngine;

public class test1ARTICLELOADING : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool requestLoadFile = false;
    ArticleSystemManager articleManager;
    void Start()
    {
        articleManager = ArticleSystemManager.Instance;
    }

    void Update()
    {
        if (!requestLoadFile)
        {
            articleManager.Load(); //c�digo de muestra sobre c�mo cargar una secuencia de di�logos
            requestLoadFile = true;
        }
        else if ((articleManager.Table != null))
        {
            Debug.Log("N�mero de art�culos cargados: " + articleManager.Table.Ids.Length);
            Debug.Log("art�culo 1: " + articleManager.Table.Titles[0]);
            Debug.Log("art�culo 2: " + articleManager.Table.Titles[1]);
            gameObject.SetActive(false);
        }
        
    }
}
