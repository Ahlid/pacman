using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Shared
{

    /// <summary>
    /// Interface that Client will have to implement
    /// </summary>

    public interface IClient : IChat
    {

        string GetState(int round);
        /// <summary>
        /// Unique identifiable name of a client
        /// </summary>
        string Username { get; }
        /// <summary>
        /// Channel address of the client.
        /// </summary>
        Uri Address { get; }
        /// <summary>
        /// Number of the round that the client will play.
        /// </summary>
        int Round { get; }

        /// <summary>
        /// Client receives a stage from the server.
        /// </summary>
        /// <param name="stage">Stage representation.</param>
        /// <param name="round">Round number.</param>
        void SendRound(List<Action> actions, int score, int round);
        /// <summary>
        /// Client receives indication that the hame has started.
        /// </summary>
        /// <param name="stage">Stage representation.</param>
        void Start(IStage stage);
        /// <summary>
        /// Player has died
        /// </summary>
        void Died();
        /// <summary>
        /// Client receives information that the game has ended and receives also the winner.
        /// </summary>
        /// <param name="winner"></param>
        void End(IPlayer winner);
    }
}