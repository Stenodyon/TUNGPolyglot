using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using PolyglotCommon;
using PiTung_Bootstrap.Console;

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
        }

        private void OnNewLocalBoard(GameObject board)
        {
            int parentID = -1;
            if(board.transform.parent != null)
            {
                NetComponent parentComp = board.transform.parent.GetComponent<NetComponent>();
                if (parentComp != null)
                    parentID = parentComp.globalID;
            }
            NetComponent netComp = board.AddComponent<NetComponent>();
            netComp.localID = NewUniqueID();
            pendingID.Add(netComp.localID, board);
            CircuitBoard comp = board.GetComponent<CircuitBoard>();
            v3 position = parentID == -1 ? board.transform.position : board.transform.localPosition;
            v3 rotation = parentID == -1 ? board.transform.eulerAngles : board.transform.localEulerAngles;
            client.SendPacket(new NewBoard
            {
                ID = netComp.localID,
                Parent = parentID,
                Width = comp.x,
                Height = comp.z,
                Position = position,
                Angles = rotation
            });
        }

        public void OnNewRemoteBoard(NewBoard packet)
        {
            Transform parent = null;
            if(packet.Parent != -1)
            {
                NetBoard parentBoard;
                if(boards.TryGetValue(packet.ID, out parentBoard))
                    parent = parentBoard.Obj.transform;
            }
            GameObject board = placer.Board(packet.Width, packet.Height, packet.Position, Quaternion.Euler(packet.Angles), parent);
            NetComponent comp = board.AddComponent<NetComponent>();
            comp.globalID = packet.ID;
            boards.Add(packet.ID, new NetBoard(packet.ID, board));
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
    }
}
