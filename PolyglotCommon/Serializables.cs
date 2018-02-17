using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PolyglotCommon
{
    [Serializable]
    public class v3
    {
        public float x, y, z;
        public v3(float x, float y, float z)
        {
            this.x = x; this.y = y; this.z = z;
        }
        public static implicit operator v3(Vector3 cpy)
        {
            return new v3(cpy.x, cpy.y, cpy.z);
        }
        public static implicit operator Vector3(v3 cpy)
        {
            return new Vector3(cpy.x, cpy.y, cpy.z);
        }
    }
}
