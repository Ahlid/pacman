using System;
using Shared;

namespace Server.RaftCommands
{
    [Serializable]
    public class SetPlay : RaftCommand
    {

        public Uri Address { get; set; }
        public Play Play { get; set; }

        public override void Execute(RaftServer server, bool AsLeader)
        {
           server.plays[Address] = Play;
        }
    }
}