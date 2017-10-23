namespace Shared
{
    public interface IClient 
    { 
        string Address { get; set; }
        //Sends a immutable stage. The round is used to identify the round. 
        //If two are received for some reason, the client will only accept/display
        //the last round received.
        void sendRoundStage(IStage stage, int round);
        //Signals the client that the game has started
        void start(IStage stage);
    }
}