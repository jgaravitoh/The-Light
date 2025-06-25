// ===============================
// RhythmGameInputManager.cs
// ===============================

using System;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGameInputManager : MonoBehaviour
{
    [Serializable]
    public class LaneKeyBinding
    {
        public int lane;
        public KeyCode key;
    }

    public List<LaneKeyBinding> keyBindings = new();
    public event Action<int> OnLaneInput;

    private void Update()
    {
        foreach (var binding in keyBindings)
        {
            if (Input.GetKeyDown(binding.key))
                OnLaneInput?.Invoke(binding.lane);
        }
    }
}