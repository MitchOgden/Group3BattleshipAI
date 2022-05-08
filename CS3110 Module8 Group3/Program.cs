// Multiplayer Battleship Game with AI - Partial Solution

using System.Collections.Generic;

namespace Module8
{
    class Program
    {
        static void Main(string[] args)
        {
            List<IPlayer> players = new List<IPlayer>();
            players.Add(new DumbPlayer("Dumb"));
            //players.Add(new RandomPlayer("Random"));
            players.Add(new DumbPlayer("Dumb Dog"));
            players.Add(new RandomPlayer("Random Cat"));
            players.Add(new AshleyAIPlayer("Ashley"));
            players.Add(new Group3Player("Group3Player"));
            

            MultiPlayerBattleShip game = new MultiPlayerBattleShip(players);
            game.Play(PlayMode.NoDelay);  // Play the game with this "play mode"
        }
    }
}
