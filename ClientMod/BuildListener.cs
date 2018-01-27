using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiTung_Bootstrap.Console;
using UnityEngine;
using Harmony;

namespace Polyglot
{
    public abstract class BuildListener
    {
        private static List<BuildListener> instances = new List<BuildListener>();

        public BuildListener()
        {
            instances.Add(this);
        }

        [HarmonyPatch(typeof(BoardPlacer), "PlaceBoard")]
        private class PlaceBoardPatch
        {
            private static void Prefix()
            {
                if (BoardPlacer.BoardBeingPlaced == null)
                    IGConsole.Log("null board?");
                foreach(BuildListener listener in instances)
                    listener.OnPlaceBoard(BoardPlacer.BoardBeingPlaced);
            }
        }

        [HarmonyPatch(typeof(BoardPlacer), "CancelPlacement")]
        private class DeleteBoardPatch
        {
            static void Prefix()
            {
                IGConsole.Log("Deleting board");
                if (BoardPlacer.BoardBeingPlaced == null)
                    IGConsole.Log("null board?");
                foreach (BuildListener listener in instances)
                    listener.OnDeleteBoard(BoardPlacer.BoardBeingPlaced);
            }
        }

        protected abstract void OnPlaceBoard(GameObject board);
        protected abstract void OnDeleteBoard(GameObject board);
    }
}
