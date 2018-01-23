using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polyglot
{
    public class Placer
    {
        private BoardPlacer boardPlacer = null;
        private MethodInfo _SetChildCircuitsMegaMeshStatus = null;

        public Placer()
        {
            Console.RegisterCommand(new Command_tryplace(this));
            SceneManager.activeSceneChanged += GameplayInit;
        }

        private void GameplayInit(Scene arg0, Scene arg1)
        {
            if (arg1.name != "gameplay")
                return;
            SceneManager.activeSceneChanged -= GameplayInit;
            boardPlacer = GameObject.FindObjectOfType<BoardPlacer>();
            if (!boardPlacer)
            {
                Console.Error("Could not find BoardPlacer");
                return;
            }
            Console.Log("Board placer found");
            _SetChildCircuitsMegaMeshStatus = boardPlacer.GetType()
                .GetMethod("SetChildCircuitsMegaMeshStatus", BindingFlags.NonPublic | BindingFlags.Instance);
            if (_SetChildCircuitsMegaMeshStatus == null)
            {
                Console.Error("Could not get SCCMMS method");
                return;
            }
            Console.Log("Got SCCMMS method");
        }

        public GameObject Board(int width, int height,
            Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject board = UnityEngine.Object.Instantiate(boardPlacer.BoardPrefab, position, rotation, parent);
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

            public override void Execute(IEnumerable<string> arguments)
            {
                if(SceneManager.GetActiveScene().name != "gameplay")
                {
                    Console.Error("Only usable on gameplay");
                    return;
                }
                placer.Board(4, 4, new Vector3(0f, 1f, 0f), Quaternion.identity);
            }
        }
    }
}
