using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Shared
{

    /// <summary>
    /// Interface that Client will have to implement
    /// </summary>
   
    public interface IClient : IChat
    {
     
        /// <summary>
        /// Unique identifiable name of a client
        /// </summary>
        string Username { get; set; }
        /// <summary>
        /// Channel address of the client.
        /// </summary>
        string Address { get; set; }
        /// <summary>
        /// Number of the round that the client will play.
        /// </summary>
        int Round { get; set; }


        //Sends a immutable stage. The round is used to identify the round. 
        //If two are received for some reason, the client will only accept/display
        //the last round received.


        string GetState(int round);
        /// <summary>
        /// Client receives a immutable stage from the server.
        /// </summary>
        /// <param name="stage">Stage representation.</param>
        /// <param name="round">Round number.</param>
        void SendRoundStage(List<Action> actions, int score, int round);
        /// <summary>
        /// Client receives indication that the hame has started.
        /// </summary>
        /// <param name="stage">Stage representation.</param>
        void Start(IStage stage);
        /// <summary>
        /// Client receives information that the game has ended and receives also the winner.
        /// </summary>
        /// <param name="player"></param>
        void End(IPlayer player);
        /// <summary>
        /// Client received indication if he is waiting for other players or he is queued for
        /// the next game.
        /// </summary>
        /// <param name="message">info</param>
        void LobbyInfo(string message);
        /// <summary>
        /// Client receives a map of clients usernames and adresses in the game.
        /// </summary>
        /// <param name="clients">clients</param>
        void sendPlayersOnGame(Dictionary<string, string> clients);
        /// <summary>
        /// Client receives the recent player that joined the game.
        /// </summary>
        /// <param name="username">new player username</param>
        /// <param name="address">new player address</param>
        void sendNewPlayer(string username, string address);

        void GameOver();
    }
}