using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using PiTung.Console;

namespace Polyglot
{
    public class Placer
    {
        private static BoardPlacer boardPlacer = null;
        private static MethodInfo _SetChildCircuitsMegaMeshStatus = null;

        public Placer()
        {
            IGConsole.RegisterCommand(new Command_tryplace(this), Polyglot.INSTANCE);
            if (SceneManager.GetActiveScene().name == "gameplay")
                Init();
            else
                SceneManager.activeSceneChanged += DelayedInit;
        }

        private void DelayedInit(Scene arg0, Scene arg1)
        {
            if (arg1.name != "gameplay")
                return;
            SceneManager.activeSceneChanged -= DelayedInit;
            Init();
        }

        private void Init()
        {
            if (boardPlacer != null)
                return;
            boardPlacer = GameObject.FindObjectOfType<BoardPlacer>();
            if (!boardPlacer)
            {
                IGConsole.Error("Could not find BoardPlacer");
                return;
            }
            IGConsole.Log("Board placer found");
            _SetChildCircuitsMegaMeshStatus = boardPlacer.GetType()
                .GetMethod("SetChildCircuitsMegaMeshStatus", BindingFlags.NonPublic | BindingFlags.Instance);
            if (_SetChildCircuitsMegaMeshStatus == null)
            {
                IGConsole.Error("Could not get SCCMMS method");
                return;
            }
            IGConsole.Log("Got SCCMMS method");
        }

        public GameObject Board(int width, int height,
            Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject board = UnityEngine.Object.Instantiate(boardPlacer.BoardPrefab, parent);
            board.transform.position = position;
            board.transform.rotation = rotation;
            CircuitBoard circuit = board.GetComponent<CircuitBoard>();
            circuit.x = width;
            circuit.z = height;
            circuit.CreateCuboid();
            StuffPlacer.DestroyIntersectingConnections(board);
            DestroyInvalidWiresOnBoard(board);
            MegaMesh.AddMeshesFrom(board);
            MegaBoardMeshManager.AddBoardsFrom(board);
            SetChildCircuitsMegaMeshStatus(board, true);
            return board;
        }

        public void MoveBoard(GameObject board, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            MegaMesh.RemoveMeshesFrom(board);
            MegaBoardMeshManager.RemoveBoardsFrom(board);
            SetChildCircuitsMegaMeshStatus(board, false);
            board.transform.parent = parent;
            board.transform.position = position;
            board.transform.rotation = rotation;
            StuffPlacer.DestroyIntersectingConnections(board);
            DestroyInvalidWiresOnBoard(board);
            MegaMesh.AddMeshesFrom(board);
            MegaBoardMeshManager.AddBoardsFrom(board);
            SetChildCircuitsMegaMeshStatus(board, true);
        }

        public void DeleteBoard(GameObject board)
        {
            MegaBoardMeshManager.RemoveBoardsFrom(board);
            UnityEngine.Object.Destroy(board);
        }

        private void DestroyInvalidWiresOnBoard(GameObject board)
        {
            foreach(var iiCon in board.GetComponents<InputInputConnection>())
            {
                if (!StuffConnecter.CanConnect(iiCon.gameObject))
                    StuffDeleter.DestroyIIConnection(iiCon);
            }

            foreach(var ioCon in board.GetComponents<InputOutputConnection>())
            {
                if (!StuffConnecter.CanConnect(ioCon.gameObject))
                    StuffDeleter.DestroyIOConnection(ioCon);
            }
        }

        private void SetChildCircuitsMegaMeshStatus(GameObject obj, bool allowed)
        {
            _SetChildCircuitsMegaMeshStatus.Invoke(boardPlacer, new object[] { obj, allowed });
        }

        private class Command_tryplace : Command
        {
            public override string Name => "tryplace";
            public override string Usage => $"{Name}";

            private Placer placer = null;

            public Command_tryplace(Placer placer)
            {
                this.placer = placer;
            }

            public override bool Execute(IEnumerable<string> arguments)
            {
                if(SceneManager.GetActiveScene().name != "gameplay")
                {
                    IGConsole.Error("Only usable on gameplay");
                    return false;
                }
                placer.Board(4, 4, new Vector3(0f, 1f, 0f), Quaternion.identity);
                return true;
            }
        }
    }
}
