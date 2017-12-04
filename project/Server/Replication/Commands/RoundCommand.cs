using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    [Serializable]
    public class RoundCommand : ICommand
    {

        private Dictionary<Uri, Play> plays;

        public RoundCommand(Dictionary<Uri, Play> plays)
        {
            this.plays = plays;
        }

        public void Execute(ServerContext context)
        {
            Console.WriteLine("number players:" + context.stateMachine
                .Stage.GetPlayers().Count);
     
            foreach (Uri address in plays.Keys)
            {
                IPlayer player = context.stateMachine
                    .Stage.GetPlayers().First(p => p.Address.ToString() == address.ToString());
                context.stateMachine.SetPlay(player, plays[address]);
            }

            if (context.CurrentRole == ServerContext.Role.Leader)
            {
                List<Shared.Action> actionList = context.stateMachine.NextRound();

                IClient client;
                for (int i = context.sessionClients.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        client = context.sessionClients.ElementAt(i);
                        client.SendRound(actionList, context.playerList, context.stateMachine.Round);
                        Console.WriteLine(String.Format("Sending stage to client: {0}, at: {1}", client.Username, client.Address));
                        Console.WriteLine(String.Format("Round Nº{0}", context.stateMachine.Round));
                    }
                    catch (Exception)
                    {
                        context.sessionClients.RemoveAt(i);


                        // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.

                        /*todo:
                         * qual a estrategia a adoptar aqui para tentar reconectar com o cliente?
                         * 
                         * Dectar falhas de clientes, lidar com falsos positivos.
                         * 
                         * Caso não seja pssível contactar o cliente, na próxima ronda deve de ir uma acção em que o player 
                         * está morto, e deve ser removido do jogo.
                         * E deve ser apresentado no chat UMA MENSAGEM no chat a indicar que o jogador saiu do jogo
                         * 
                         * garantimos a possibilidade de um cliente voltar a entrar no jogo?
                         * 
                         */
                    }
                }

                context.Timer.Start(); //Restart the timer
            }
        }
    }
}
