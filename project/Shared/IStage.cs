using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IStage
    {
        List<IMonster> getMonsters();
        void addMonster(IMonster monster);

        List<ICoin> getCoins();
        void addCoin(ICoin coin);
        void removeCoin(ICoin coin);

        List<IPlayer> getPlayers();
        void addPlayer(IPlayer player);
        void removePlayer(IPlayer player);
    }
}
