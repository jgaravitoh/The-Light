using UnityEngine;

namespace AkilliMum
{
    public class WebOps :MonoBehaviour
    {
        public void OpenURL(string url)
        {
            Application.OpenURL(url);
        }
    }
}