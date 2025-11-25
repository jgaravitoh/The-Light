using UnityEngine;
using UnityEngine.Rendering;

namespace AkilliMum
{
    public class FpsLimit : MonoBehaviour
    {
        [Tooltip("Set <0 or 0 for unlimited")]
        public int Limit = 0;
        
        private void Start()
		{
            if (Limit<=0)
                Application.targetFrameRate = int.MaxValue;
            else
                Application.targetFrameRate = Limit;
        }
    }
}