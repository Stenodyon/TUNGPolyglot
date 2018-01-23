using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Polyglot
{
    // TODO: Board management packets

    public class BoardManager : BuildListener
    {
        private Dictionary<int, GameObject> pendingID;
        private Dictionary<int, NetBoard> boards;

        private Placer placer;

        private static int IDCounter = 0;

        public BoardManager()
        {
            pendingID = new Dictionary<int, GameObject>();
            boards = new Dictionary<int, NetBoard>();

            placer = new Placer();
        }

        public void OnNewLocalBoard(GameObject board)
        {
            NetComponent netComp = board.AddComponent<NetComponent>();
            netComp.localID = NewUniqueID();
            pendingID.Add(netComp.localID, board);
            //TODO: Send NewBoard packet
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
        }
    }
}
