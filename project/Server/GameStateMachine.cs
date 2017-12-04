using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class GameStateMachine
    {
        public IStage Stage { get; set; }
        public Dictionary<IPlayer, Play> PlayerMoves { get; set; }
        public int Round { get; set; }
        public List<Shared.Action> Actions { get; set; }

        public GameStateMachine(int NumberOfPlayers, List<IPlayer> players)
        {
            this.Stage = new Stage();
            this.Stage.BuildInitStage(NumberOfPlayers);
            this.PlayerMoves = new Dictionary<IPlayer, Play>();
            this.Actions = new List<Shared.Action>();

            foreach (IPlayer player in players)
            {
                this.Stage.AddPlayer(player);
                this.PlayerMoves[player] = Play.NONE;
            }
        }

        public List<Shared.Action> NextRound()
        {
            this.Round++;
            this.Actions.Clear();
            ComputeMovement();
            DetectCollisions();
            return new List<Shared.Action>(this.Actions);
        }

        public void SetPlay(IPlayer player, Play play)
        {
            PlayerMoves[player] = play;
        }

        public IPlayer GetTopPlayer()
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
                Shared.Action action = monster.Step(this.Stage);
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
    }
}
