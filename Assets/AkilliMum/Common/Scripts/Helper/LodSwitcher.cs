using UnityEngine;

namespace AkilliMum
{
    public class LodSwitcher : MonoBehaviour
    {
        public GameObject[] Lods;
        private void CloseAll()
        {
            foreach (var lod in Lods)
            {
                lod.SetActive(false);
            }
        }

        public void Open(int place)
        {
            CloseAll();
            Lods[place].SetActive(true);
        }
    }
}