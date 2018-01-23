using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Polyglot
{
    public class NetBoard
    {
        public int ID { get; private set; }
        public GameObject Obj { get; private set; }
        public int? Parent;

        public NetBoard(int id, GameObject obj, int? parent = null)
        {
            this.ID = id; this.Obj = obj; this.Parent = parent;
        }
    }
}
