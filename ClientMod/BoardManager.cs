using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using PolyglotCommon;
using PiTung.Console;

namespace Polyglot
{
    // TODO: Board management packets

    public class BoardManager : BuildListener
    {
        private Dictionary<int, GameObject> pendingID;
        private Dictionary<int, NetBoard> boards;

        private Placer placer;

        private static int IDCounter = 0;
        private ClientConnection client;

        public BoardManager(ClientConnection client)
        {
            this.client = client;
            pendingID = new Dictionary<int, GameObject>();
            boards = new Dictionary<int, NetBoard>();

            placer = new Placer();

            IGConsole.RegisterCommand(new Command_lsboard(this), Polyglot.INSTANCE);
        }

        public int GetParent(GameObject board)
        {
            Transform parent = board.transform.parent;
            if(parent != null)
            {
                NetComponent netComp = parent.GetComponent<NetComponent>();
                if (netComp != null)
                    return netComp.globalID;
            }
            return -1;
        }

        public int GetParent(int ID)
        {
            NetBoard board;
            if(boards.TryGetValue(ID, out board))
                return GetParent(board.Obj);
            return -1;
        }

        public Transform GetParentBoard(Transform obj)
        {
            if(obj.parent != null)
            {
                CircuitBoard board = obj.parent.GetComponent<CircuitBoard>();
                if (board != null)
                    return obj.parent;
                return GetParentBoard(obj.parent);
            }
            return null;
        }

        private void OnNewLocalBoard(GameObject board)
        {
            int parentID = -1;
            Transform parentBoard = GetParentBoard(board.transform);
            if(parentBoard != null)
            {
                NetComponent parentComp = parentBoard.GetComponent<NetComponent>();
                if (parentComp != null)
                    parentID = parentComp.globalID;
            }
            NetComponent netComp = board.AddComponent<NetComponent>();
            netComp.localID = NewUniqueID();
            pendingID.Add(netComp.localID, board);
            CircuitBoard comp = board.GetComponent<CircuitBoard>();
            client.SendPacket(new NewBoard
            {
                ID = netComp.localID,
                Parent = parentID,
                Width = comp.x,
                Height = comp.z,
                Position = board.transform.position,
                Angles = board.transform.eulerAngles
            });
            IGConsole.Log($"Spawned board {netComp.localID}[{parentID}]");
        }

        public void OnNewRemoteBoard(NewBoard packet)
        {
            IGConsole.Log($"New board {packet.ID}[{packet.Parent}]");
            Transform parent = null;
            if(packet.Parent != -1)
            {
                NetBoard parentBoard;
                if (boards.TryGetValue(packet.Parent, out parentBoard))
                    parent = parentBoard.Obj.transform;
                else
                    IGConsole.Error("Could not find parent board");
            }
            GameObject board = placer.Board(packet.Width, packet.Height, packet.Position, Quaternion.Euler(packet.Angles), parent);
            NetComponent comp = board.AddComponent<NetComponent>();
            comp.globalID = packet.ID;
            boards.Add(packet.ID, new NetBoard(packet.ID, board));
        }

        private void OnLocalMovedBoard(int ID)
        {
            NetBoard board;
            if(boards.TryGetValue(ID, out board))
            {
                Transform transform = board.Obj.transform;
                int parent = GetParent(board.Obj);
                client.SendPacket(new MovedBoard
                {
                    ID = ID,
                    Parent = parent,
                    Position = transform.position,
                    Rotation = transform.eulerAngles
                });
            }
        }

        public void OnRemoteMovedBoard(MovedBoard packet)
        {
            Transform parent = null;
            NetBoard parentBoard;
            if(boards.TryGetValue(packet.Parent, out parentBoard))
                parent = parentBoard.Obj.transform;
            NetBoard board;
            if(boards.TryGetValue(packet.ID, out board))
            {
                placer.MoveBoard(board.Obj, packet.Position, Quaternion.Euler(packet.Rotation), parent);
            }
        }

        public void DeleteRemoteBoard(int ID)
        {
            NetBoard board;
            if (boards.TryGetValue(ID, out board))
            {
                if(board.Obj != null)
                    placer.DeleteBoard(board.Obj);
                boards.Remove(ID);
            }
            else
            {
                IGConsole.Error("Could not find board to delete");
            }
        }

        private int NewUniqueID()
        {
            return IDCounter++;
        }

        public void GlobalIDAttribution(int localID, int globalID)
        {
            GameObject pendingBoard;
            if(pendingID.TryGetValue(localID, out pendingBoard))
            {
                pendingID.Remove(localID);
                boards.Add(globalID, new NetBoard(globalID, pendingBoard));
                NetComponent comp = pendingBoard.GetComponent<NetComponent>();
                comp.globalID = globalID;
            }
        }

        protected override void OnPlaceBoard(GameObject board)
        {
            NetComponent netComp = board.GetComponent<NetComponent>();
            if (netComp == null)
                OnNewLocalBoard(board);
            else if(netComp.globalID != -1)
                OnLocalMovedBoard(netComp.globalID);
        }

        protected override void OnDeleteBoard(GameObject board)
        {
            NetComponent comp = board.GetComponent<NetComponent>();
            if(comp != null)
            {
                IGConsole.Log("Found netcomp");
                if(comp.globalID != -1)
                    client.SendPacket(new DeleteBoard { ID = comp.globalID });
            }
        }

        private class Command_lsboard : Command
        {
            public override string Name => "lsboard";
            public override string Usage => $"{Name}";

            private BoardManager manager;

            public Command_lsboard(BoardManager manager)
            {
                this.manager = manager;
            }

            public override bool Execute(IEnumerable<string> arguments)
            {
                foreach(var entry in manager.boards)
                {
                    IGConsole.Log($"{entry.Key} -> {entry.Value.Obj.name}");
                }
                return true;
            }
        }
    }
}
