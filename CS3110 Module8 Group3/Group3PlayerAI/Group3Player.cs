using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Module8
{
    internal class Group3Player : IPlayer
    {
        private int _index; // player's index in turn order
        private int _gridSize; // size of grid
        private Ships _ships; // size of grid
        private static readonly Random Random = new Random(); // used to randomize choices
        private bool _selfDestruct = false;
        private int _zeroZeroCounter = 0;
        private int _turnCounter = 1;
        private List<NewPlayerData> _playersData;
        private int _lowestShipCount = 0;
        private int _playerCount = 0;
        private List<int> _eliminatedPlayers = new List<int>();

        // Constructor:
        public Group3Player(string name)
        {
            Name = name;
        }

        // Property that returns player's name:
        public string Name { get; }

        // Property that returns player's index in turn order.
        public int Index => _index;


        // Logic to start new game
        // **** TBD ****
        // TBD: Does this properly reset if more than 1 game is played during runtime?
        // (arrays need to be reset, etc.)
        // **** TBD ****
        public void StartNewGame(int playerIndex, int gridSize, Ships ships)
        {
            _gridSize = gridSize;
            _index = playerIndex;
            _lowestShipCount = ships._ships.Count;
            // **** TBD ****
            // TBD: Find a 'smarter' way to place ships.
            // Currently: this borrows from RandomPlayer, which just puts the ships in the grid in Random columns
            //Note it cannot deal with the case where there's not enough columns
            //for 1 per ship
            // **** TBD ****

            var availableColumns = new List<int>();
            for (int i = 0; i < gridSize; i++)
            {
                availableColumns.Add(i);
            }

            _ships = ships;
            foreach (var ship in ships._ships)
            {
                // Pick an open X from the remaining columns
                var x = availableColumns[Random.Next(availableColumns.Count)];
                availableColumns.Remove(x); //Make sure we can't pick it again

                // Pick a Y that fits
                var y = Random.Next(gridSize - ship.Length);
                ship.Place(new Position(x, y), Direction.Vertical);
            }
        }


        // Method to intelligently find best spot to attack.
        public Position GetAttackPosition()
        {
            Position proposedPosition = new Position(0, 0);
            int weakPlayer = -1; // Stores must vulnerable player index
            
            // If our player is called to take the first shot before any playerData objects are created
            // just return (0,0)
            if (_turnCounter == 1)
                return proposedPosition;

            // Figure out most vulnerable player based on number of ships left
            for(int i = 0; i < _playersData.Count; i++)
            {
                if (_playersData[i].ShipsLeft <= _lowestShipCount && _playersData[i].Index != Index && !IsEliminated(_playersData[i].Index))
                {
                    _lowestShipCount = _playersData[i].ShipsLeft;
                    weakPlayer = _playersData[i].Index;
                }
            }
            
            proposedPosition = _playersData[weakPlayer].NextShot();

            if (proposedPosition != null && IsValid(proposedPosition))
            {
                return proposedPosition;
            }

            return GetAttackPosition();

        }

        // IsEliminiated()
        //
        // Checks if a player has been eliminated
        private bool IsEliminated(int playerIndex)
        {
            foreach (var player in _eliminatedPlayers)
            {
                if (playerIndex == player)
                    return true;
            }

            return false;
        }
        
        // This method, given a position, checks if it is a valid spot at which to fire.
        // Valid spots do not contain the player's own ships, have not already been shot at, and
        // are on the grid.
        internal bool IsValid(Position p)
        {
            if (p.X == 0 && p.Y == 0)
                _zeroZeroCounter++;
            
            if (_zeroZeroCounter > 2)
                if (p.X == 0 && p.Y == 0)
                    _selfDestruct = true;

            // Self Destruct Protocol
            if (!_selfDestruct)
            {
                IEnumerable<ShipTypes> result = new List<ShipTypes>();

                foreach (Ship s in _ships._ships)
                {
                    result = from position in s.Positions
                        where position.X == p.X && position.Y == p.Y
                        select s.ShipType;
                }

                foreach (var r in result)
                    if (r == ShipTypes.Battleship)
                        return false;
            }
            

            // Check to see if spot is on the grid
            if (p.X < 0 || p.X >= _gridSize || p.Y < 0 || p.Y >= _gridSize)
            {
                return false;
            }

            // If all the checks have passed, this spot is valid.
            return true;

        }

        // Method to log results throughout the game.
        // 
        // 
        public void SetAttackResults(List<AttackResult> results)
        {
            
            // On turn one when the attack results are received, create a list of PlayerData objects 
            // to store a status and probability grid
            if (_turnCounter == 1)
            {
                // Initialize _playersData to player count
                _playersData = new List<NewPlayerData>(results.Count+1);
                _playerCount = results.Count;
                Debug.WriteLine($"{results.Count} Players are being added to _playersData");
                foreach (var r in results)
                {
                    Debug.WriteLine($"Player {r.PlayerIndex} has been added with an initial value of {r.ResultType} in it's status grid at ({r.Position.X},{r.Position.Y})");
                    _playersData.Insert(r.PlayerIndex,new NewPlayerData(_gridSize,_ships, r));
                }
            }

            // Check for removed players and add to eliminated players list
            foreach (var r in results)
            {
                if(r.ResultType == AttackResultType.Sank && r.SunkShip == ShipTypes.Battleship)
                    _eliminatedPlayers.Add(r.PlayerIndex);
            }

            foreach (var r in results)
            {
                _playersData[r.PlayerIndex].ProcessResult(r);
            }
            
            
            _turnCounter++;

        }
        
        
        
    }
    
}


