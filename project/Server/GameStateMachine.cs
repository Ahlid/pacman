using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// Represents a state machine for the game
    /// </summary>
    public class GameStateMachine
    {
        /// <summary>
        /// The game's stage
        /// </summary>
        public IStage Stage { get; set; }

        /// <summary>
        /// The player's move for current round
        /// </summary>
        public Dictionary<IPlayer, Play> PlayerMoves { get; set; }

        /// <summary>
        /// Current round
        /// </summary>
        public int Round { get; set; }

        /// <summary>
        /// Round's result Actions
        /// </summary>
        public List<Shared.Action> Actions { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="NumberOfPlayers">Number of players for current game </param>
        /// <param name="players">The list with the players</param>
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

        /// <summary>
        /// Computes the next round
        /// </summary>
        /// <returns>The actions resulted from this round</returns>
        public List<Shared.Action> NextRound()
        {
            this.Round++;
            this.Actions.Clear();
            ComputeMovement();
            DetectCollisions();
            return new List<Shared.Action>(this.Actions);
        }

        /// <summary>
        /// Set's a player's play for current round
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="play">Player's play</param>
        public void SetPlay(IPlayer player, Play play)
        {
            PlayerMoves[player] = play;
        }

        /// <summary>
        /// Gets the current winning player
        /// </summary>
        /// <returns>The winning player</returns>
        public IPlayer GetTopPlayer()
        {
            //if there is only one player alive
            var alivePlayers = this.Stage.GetPlayers().Where(p => p.Alive);
            if (alivePlayers.Count() == 1)
            {
                return alivePlayers.First();
            }
            //retornar o que tem mais score
            if (alivePlayers.Count() > 1)
                return alivePlayers.OrderBy(p => p.Score).Last();

            return this.Stage.GetPlayers().OrderBy(p => p.Score).Last();
        }

        /// <summary>
        /// Checks if the game has ended
        /// </summary>
        /// <returns>true if yes of false if not</returns>
        public bool HasGameEnded()
        {
            if (this.Stage.GetPlayers().Count > 0) // if players exist on the game 
            {
                // check if there are any player alive or if the all the coins have been captured.
                return (this.Stage.GetPlayers().Count(p => p.Alive) == 0) || (this.Stage.GetCoins().Count == 0);
            }
            else
            {
                return true; // there are not players on the game, they all quit or crashed. 
            }
        }

        /// <summary>
        /// Computes movement
        /// </summary>
        private void ComputeMovement()
        {
            //Assume that the playerMoves has a play for every play, with default Play.NONE
            foreach (Player player in this.Stage.GetPlayers())
            {
                if (!player.Alive)
                {
                    continue;
                }
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

        /// <summary>
        /// Detecs colisions
        /// </summary>
        private void DetectCollisions()
        {
            ComputeCollisionsPlayerCoin();
            ComputeCollisionsPlayerWall();
            ComputeCollisionsPlayerMonster();
        }

        /// <summary>
        /// Detects coin colision
        /// </summary>
        private void ComputeCollisionsPlayerCoin()
        {
            foreach (Player player in this.Stage.GetPlayers())
            {
                if (!player.Alive)
                {
                    continue;
                }
                for (int i = 0; i < this.Stage.GetCoins().Count; i++)
                {
                    ICoin coin = this.Stage.GetCoins()[i];

                    bool colliding = player.IsColliding(Player.WIDTH, Player.HEIGHT,
                        coin, Coin.WIDTH, Coin.HEIGHT);

                    if (colliding)
                    {
                        player.Score++;
                        this.Stage.RemoveCoin(coin);
                        i--;
                        this.Actions.Add(new global::Shared.Action()
                        {
                            action = global::Shared.Action.ActionTaken.REMOVE,
                            ID = coin.ID
                        });
                    }
                }
            }
        }

        /// <summary>
        /// detecs wall colision
        /// </summary>
        private void ComputeCollisionsPlayerWall()
        {
            foreach (Player player in this.Stage.GetPlayers())
            {
                if (!player.Alive)
                {
                    continue;

                }
                foreach (Wall wall in this.Stage.GetWalls())
                {
                    bool colliding = player.IsColliding(Player.WIDTH, Player.HEIGHT,
                        wall, Wall.WIDTH, Wall.HEIGHT);

                    if (colliding)
                    {

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

        /// <summary>
        /// Detects monster colision
        /// </summary>
        private void ComputeCollisionsPlayerMonster()
        {
            foreach (Player player in this.Stage.GetPlayers())
            {

                if (!player.Alive)
                {
                    continue;
                }
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
