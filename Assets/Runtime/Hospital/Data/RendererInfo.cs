using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LiverDie.Hospital.Data
{
    [Serializable]
    public class RendererInfo
    {
        public Renderer Renderer = null!;
        public List<Material> Materials = new();
        public List<int> MaterialIndices = new List<int>() { 0 };
    }
}
