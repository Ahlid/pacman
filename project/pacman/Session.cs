using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pacman
{
    public class Session
    {
        public string Username { get; set; }
        public int Round { get; set; }
        public int Score { get; set; }
        public List<IClient> Peers { get; }
        public enum Status { PENDING, QUEUED, RUNNING, DIED, ENDED }
        public Status SessionStatus { get; set; }
        public int MsecPerRound { get; set; }
        public IGame game { get; set; }


        public Session(IGame game, int msecPerRound = 20)
        {
            SessionStatus = Status.PENDING;
            Round = 0;
            Score = 0;
            MsecPerRound = msecPerRound;
            this.game = game;
        }
    }
}
