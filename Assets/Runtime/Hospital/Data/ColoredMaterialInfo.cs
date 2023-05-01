using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiverDie.Hospital.Data
{
    [Serializable]
    public class ColoredMaterialInfo
    {
        public float HueDelta = 0;
        public List<Material> Materials = new();
    }
}
