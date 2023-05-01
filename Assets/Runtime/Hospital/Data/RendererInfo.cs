using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiverDie.Hospital.Data
{
    [Serializable]
    public class RendererInfo
    {
        public Renderer Renderer;
        public List<Material> Materials = new();
        public List<int> MaterialIndices = new List<int>() { 0 };
    }
}
