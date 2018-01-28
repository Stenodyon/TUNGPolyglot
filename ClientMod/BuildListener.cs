using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using PiTung_Bootstrap.Console;
using UnityEngine;
using Harmony;
using System.Reflection.Emit;

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

        [HarmonyPatch(typeof(StuffPlacer), "PlaceOnBoard")]
        private class PlaceOnBoardPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> original)
            {
                MethodInfo method = typeof(BuildListener).GetMethod("ItemPlaced", BindingFlags.Static | BindingFlags.NonPublic);
                if (method == null)
                    throw new NullReferenceException("Could not get BuildListener.ItemPlaced");
                List<CodeInstruction> _original = original.ToList();
                _original.Insert(_original.Count - 1, new CodeInstruction(OpCodes.Ldloc_S, 0x04)); // index 4 is the gameobject
                _original.Insert(_original.Count - 1, new CodeInstruction(OpCodes.Call, method));
                return _original;
            }
        }

        [HarmonyPatch(typeof(StuffPlacer), "PlaceOnOther")]
        private class PlaceOnOtherPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> original)
            {
                MethodInfo method = typeof(BuildListener).GetMethod("ItemPlaced", BindingFlags.Static | BindingFlags.NonPublic);
                if (method == null)
                    throw new NullReferenceException("Could not get BuildListener.ItemPlaced");
                List<CodeInstruction> _original = original.ToList();
                _original.Insert(_original.Count - 1, new CodeInstruction(OpCodes.Ldloc_0)); // local variable 0 is the gameobject
                _original.Insert(_original.Count - 1, new CodeInstruction(OpCodes.Call, method));
                return _original;
            }
        }

        [HarmonyPatch(typeof(StuffDeleter), "DeleteThing")]
        private class DeleteThingPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> original)
            {
                MethodInfo method = typeof(BuildListener).GetMethod("ItemDeleted", BindingFlags.Static | BindingFlags.NonPublic);
                if (method == null)
                    throw new NullReferenceException("Could not get BuildListener.ItemDeleted");
                List<CodeInstruction> _original = original.ToList();
                _original.Insert(_original.Count - 1, new CodeInstruction(OpCodes.Ldarg_0)); // argument 0 is the gameobject
                _original.Insert(_original.Count - 1, new CodeInstruction(OpCodes.Call, method));
                return _original;
            }
        }

        private static void ItemPlaced(GameObject obj)
        {
            foreach (var listener in instances)
                listener.OnPlaceObject(obj);
        }

        private static void ItemDeleted(GameObject obj)
        {
            if (obj.tag == "CircuitBoard")
            {
                foreach (var listener in instances)
                    listener.OnDeleteBoard(obj);
            }
            else if (obj.tag == "Wire")
            {
                foreach (var listener in instances)
                    listener.OnDeleteWire(obj);
            }
            else
            {
                foreach (var listener in instances)
                    listener.OnDeleteObject(obj);
            }
        }

        protected virtual void OnPlaceBoard(GameObject board) {}
        protected virtual void OnDeleteBoard(GameObject board) {}
        protected virtual void OnPlaceObject(GameObject obj) {}
        protected virtual void OnDeleteObject(GameObject obj) {}
        protected virtual void OnPlaceWire(GameObject wire) {}
        protected virtual void OnDeleteWire(GameObject wire) {}
    }
}
