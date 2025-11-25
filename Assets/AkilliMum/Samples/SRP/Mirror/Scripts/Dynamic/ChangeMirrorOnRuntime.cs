using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AkilliMum.SRP.Mirror
{
	public class ChangeMirrorOnRuntime : MonoBehaviour
	{
        public void _256()
        {
            Change(256);
        }

        public void _1024()
        {
            Change(1024);
        }

        public void _2048()
        {
            Change(2048);
        }

        void Change(int mirrorResolution)
        {
            foreach (MirrorManager mirror in Resources.FindObjectsOfTypeAll<MirrorManager>())
            {
                mirror.ManualSize = mirrorResolution;
                mirror.InitializeMirror();
            }
        }
    }
}