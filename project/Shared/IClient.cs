namespace Shared
{
    /// <summary>
    /// Interface that Client will have to implement
    /// </summary>
    public interface IClient 
    { 
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

        /// <summary>
        /// Client receives a immutable stage from the server.
        /// </summary>
        /// <param name="stage">Stage representation.</param>
        /// <param name="round">Round number.</param>
        void SendRoundStage(IStage stage, int round);
        /// <summary>
        /// Signals the client that the hame has started.
        /// </summary>
        /// <param name="stage">Stage representation.</param>
        void Start(IStage stage);
    }
}