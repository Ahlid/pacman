using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IGameSession
    {
        int GameId { get; set; }

        /// <summary>
        /// The number of the round
        /// </summary>
        int Round { get; set; }

        /// <summary>
        /// Map between players in the game and theirs current move.
        /// </summary>
        Dictionary<IPlayer, Play> PlayerMoves { get; set; }

        /// <summary>
        /// players of the game session
        /// </summary>
        List<IClient> Clients { get; set; }

        /// <summary>
        /// The actual stage of the game session
        /// </summary>
        IStage Stage { get; set; }

        bool HasGameStarted { get; set; }
        bool HasGameEnded();

        List<Shared.Action> PlayRound();

        void SetPlay(string address, Play play, int round);

        IPlayer GetWinningPlayer();

        void StartGame();

        void EndGame();

    }
}
