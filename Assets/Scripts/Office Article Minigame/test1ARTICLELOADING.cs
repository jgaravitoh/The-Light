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
            articleManager.Load(); //código de muestra sobre cómo cargar una secuencia de diálogos
            requestLoadFile = true;
        }
        else if ((articleManager.Table != null))
        {
            Debug.Log("Número de artículos cargados: " + articleManager.Table.Ids.Length);
            Debug.Log("artículo 1: " + articleManager.Table.Titles[0]);
            Debug.Log("artículo 2: " + articleManager.Table.Titles[1]);
            gameObject.SetActive(false);
        }
        
    }
}
