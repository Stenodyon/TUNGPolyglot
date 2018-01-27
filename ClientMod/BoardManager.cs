using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using PolyglotCommon;

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
            client.SendPacket(new NewBoard
            {
                ID = netComp.localID,
                Parent = parentID,
                Width = comp.x,
                Height = comp.z,
                Position = board.transform.position,
                Angles = board.transform.eulerAngles
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
            placer.Board(packet.Width, packet.Height, packet.Position, Quaternion.Euler(packet.Angles), parent);
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
            }
        }

        protected override void OnPlaceBoard(GameObject board)
        {
            NetComponent netComp = board.GetComponent<NetComponent>();
            if (netComp == null)
                OnNewLocalBoard(board);
        }
    }
}
