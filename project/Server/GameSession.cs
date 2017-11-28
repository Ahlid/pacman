using System;
using System.Collections.Generic;
using System.Linq;
using Shared;
using Action = Shared.Action;

namespace Server
{
    public class GameSession : IGameSession
    {
        private static int lastID = 0;

        public List<IClient> Clients { get; set; }
        public IStage Stage { get; set; }
        public int ID { get; set; }
        public int Round { get; set; }
        public Dictionary<IPlayer, Play> PlayerMoves { get; set; }
        public List<global::Shared.Action> Actions { get; set; }
        public bool HasGameStarted { get; set; }
        public int NumberOfPlayers { get; set; }


        public GameSession(int numberOfPlayers)
        {
            lastID ++;
            this.Clients = new List<IClient>();
            this.Stage = new Stage();
            this.ID = lastID;
            this.Round = 0;
            this.PlayerMoves = new Dictionary<IPlayer, Play>();
            this.Actions = new List<Action>();
            this.HasGameStarted = false;
            this.NumberOfPlayers = numberOfPlayers;
        }

        public bool HasGameEnded()
        {

            if (this.Stage.GetPlayers().Count > 0) // if players exist on the game 
            {
                // check if there are any player alive or if the all the coins have been captured.
                return !(this.Stage.GetPlayers().Count(p => p.Alive) > 0) || !(this.Stage.GetCoins().Count > 0);
            }
            else
            {
                return true; // there are not players on the game, they all quit or crashed. 
            }
        }

        public List<global::Shared.Action> PlayRound()
        {
            this.Actions.Clear();
            this.ComputeRound();
            this.broadcastRound();
            this.Round++;
            return this.Actions;
        }

        public void SetPlay(Uri address, Play play, int round)
        {
            PlayerMoves[this.Stage.GetPlayers().First(p => p.Address.ToString() == address.ToString())] = play;
        }

        public IPlayer GetWinningPlayer()
        {
            //if there is only one player alive
            var alivePlayers = this.Stage.GetPlayers().Where(p => p.Alive == true);
            if (alivePlayers.Count() == 1)
            {
                return alivePlayers.First();
            }

            //retornar o que tem mais score
            return alivePlayers.OrderBy(p => p.Score).First();
        }

        public void StartGame()
        {
            this.HasGameStarted = true;
            //todo: configurar a stage
            this.Stage.BuildInitStage(this.NumberOfPlayers);
            foreach (IClient client in this.Clients)
            {
                IPlayer player = new Player();
                player.Address = client.Address;
                player.Alive = true;
                player.Score = 0;
                player.Username = client.Username;

                this.Stage.AddPlayer(player);
                this.PlayerMoves[player] = Play.NONE;
            }

            //todo: infromar os clientes
            this.SendStart();
        }

        public void EndGame()
        {
            broadcastEndGame();
        }

        private void ComputeRound()
        {
            ComputeMovement();
            DetectCollisions();
        }

        private void ComputeMovement()
        {
            //Assume that the playerMoves has a play for every play, with default Play.NONE
            foreach (Player player in this.Stage.GetPlayers())
            {
                Play play = PlayerMoves[player];
                global::Shared.Action action = player.Move(play);
                if (action != null)
                    Actions.Add(action);
                PlayerMoves[player] = Play.NONE;
            }

            //Monsters movement
            foreach (IMonster monster in this.Stage.GetMonsters())
            {
                Action action = monster.Step(this.Stage);
                if (action != null)
                    Actions.Add(action);
            }
        }

        private void DetectCollisions()
        {
            ComputeCollisionsPlayerCoin();
            ComputeCollisionsPlayerWall();
            ComputeCollisionsPlayerMonster();
        }

        private void ComputeCollisionsPlayerCoin()
        {
            foreach (Player player in this.Stage.GetPlayers())
            {
                foreach (Coin coin in this.Stage.GetCoins())
                {
                    bool colliding = player.IsColliding(Player.WIDTH, Player.HEIGHT,
                        coin, Coin.WIDTH, Coin.HEIGHT);

                    if (colliding)
                    {
                        player.Score++;
                        this.Stage.RemoveCoin(coin);
                        this.Actions.Add(new global::Shared.Action()
                        {
                            action = global::Shared.Action.ActionTaken.REMOVE,
                            ID = coin.ID
                        });
                    }
                }
            }
        }

        // todo: check if player is alive, if is alive then do calculus. 
        // if died after computing then send message to him saying: game over
        private void ComputeCollisionsPlayerWall()
        {
            foreach (Player player in this.Stage.GetPlayers())
            {
                foreach (Wall wall in this.Stage.GetWalls())
                {
                    bool colliding = player.IsColliding(Player.WIDTH, Player.HEIGHT,
                        wall, Wall.WIDTH, Wall.HEIGHT);

                    if (colliding)
                    {
                        //TODO: The player gets a gameover and is removed from the list
                        player.Alive = false;
                        Actions.Add(new global::Shared.Action()
                        {
                            action = global::Shared.Action.ActionTaken.REMOVE,
                            ID = player.ID
                        });
                    }
                }
            }
        }

        private void ComputeCollisionsPlayerMonster()
        {
            foreach (Player player in this.Stage.GetPlayers())
            {
                foreach (IMonster monster in this.Stage.GetMonsters())
                {
                    bool colliding = player.IsColliding(Player.WIDTH, Player.HEIGHT,
                        monster, MonsterAware.WIDTH, MonsterAware.HEIGHT);//using MonsterAware for WIDTH needs a better solution

                    if (colliding)
                    {
                        //TODO: the player gets a gameover and is removed from the list
                        player.Alive = false;
                        Actions.Add(new global::Shared.Action()
                        {
                            action = global::Shared.Action.ActionTaken.REMOVE,
                            ID = player.ID
                        });
                    }
                }
            }
        }

        private void SendStart()
        {
            IClient client;

            Dictionary<string, Uri> clientsP2P = new Dictionary<string, Uri>();
            foreach (IClient c in this.Clients)
            {
                clientsP2P[c.Username] = c.Address;
            }
            
            for (int i = this.Clients.Count - 1; i >= 0; i--)
            {
                try
                {
                    client = this.Clients.ElementAt(i);
                    client.Start(this.Stage);
                    //client.SetPeers(clientsP2P);
                }
                catch (Exception e)
                {
                    this.Clients.RemoveAt(i);
                    // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.
                }
            }
        }

        private void broadcastRound()
        {
            IClient client;
            for (int i = this.Clients.Count - 1; i >= 0; i--)
            {
                try
                {
                    client = this.Clients.ElementAt(i);
                    //todo change score actions
                    client.SendRound(this.Actions, -9999, this.Round);
                }
                catch (Exception)
                {
                    this.Clients.RemoveAt(i);
                    // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.
                }
            }
            this.Actions = new List<Action>();
        }

        private void broadcastEndGame()
        {
            IClient client;
            for (int i = 0; i < this.Clients.Count; i++)
            {
                try
                {
                    client = this.Clients.ElementAt(i);
                    client.End(this.GetWinningPlayer());
                }
                catch (Exception)
                {
                    this.Clients.RemoveAt(i);
                    // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.
                }
            }
        }
    }
}