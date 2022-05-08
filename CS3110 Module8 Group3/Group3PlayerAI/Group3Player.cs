// Group3Player AI Class
// @Authors: Mitchell Ogden & Ashley Varran
// Our implementation of a battleship AI per the requirements

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
        private int _turnCounter = 1; // keep track of the current turn per the received attack results
        private List<PlayerData> _playersData;
        private int _lowestShipCount = 0; // Stores ship count related to most vulnerable player
        private int _playerCount = 0;
        private List<int> _eliminatedPlayers = new List<int>(); // As players are eliminated store them here
        private bool _dumbPlaying = true; // Flag to keep track of whether a DumbAI is currently playing
        int totalLength = 0; // Number of spaces the passed in ship list will occupy
        private List<Position> battleshipPositions = new List<Position>(5); // Stores posotions of our placed battleship for quick access
        
        // List of positions to take out dumb AIs first if they are in play
        private Stack<Position> _dumbBattleship = new Stack<Position>();


        // Constructor:
        public Group3Player(string name)
        {
            Name = name;
        }

        // Property that returns player's name:
        public string Name { get; }

        // Property that returns player's index in turn order.
        public int Index => _index;


        // StartNewGame is called when a game starts and the player's index,
        // the grid size, and the ships that will be placed have been set for the game.
        
        public void StartNewGame(int playerIndex, int gridSize, Ships ships)
        {
            _gridSize = gridSize;
            _index = playerIndex;
            _lowestShipCount = ships._ships.Count;
            totalLength = ships._ships.Sum(ship => ship.Length);

            // Add values to _dumbBattleship
            _dumbBattleship.Push(new Position(0,7));
            _dumbBattleship.Push(new Position(1,7));
            _dumbBattleship.Push(new Position(2,7));
            _dumbBattleship.Push(new Position(3,7));
            
            // Place ships vertically in a randomly selected column
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
                
                // Store BattleShip Positions
                if (ship.IsBattleShip)
                {
                    Position start = new Position(x, y);
                    Direction direction = Direction.Vertical;
                    for (int i = 0; i < 4; i++)
                    {
                        battleshipPositions.Add(new Position(start.Y, start.Y));
                        if (direction == Direction.Horizontal) start.X++;
                        if (direction == Direction.Vertical) start.Y++;
                    }
                }
                    
            }
        }


        // GetAttackPosition will look to see if there is a current target
        // If so, it will continue attacking the unknown spaces around the origina target point.
        // If not, it will look to the the _targetStack to grab the next target in the list.
        // If no target exists it will use MostProbablePosition to make a guess based on which spot has
        // the best probability of having a ship placed in it.
        public Position GetAttackPosition()
        {
            // Eliminate any dumbAIs first
            while (_dumbBattleship.Count > 0 && _dumbPlaying)
                return _dumbBattleship.Pop();
            
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

            // In the event there is not a weak player(game is processing end game conditions), return (0,0)
            // otherwise return next shot on the weakest position
            if (weakPlayer != -1)
            {
                proposedPosition = _playersData[weakPlayer].NextShot();
            }
            else
            {
                return new Position(0, 0);
            }
            
            // Verify if the proposed position is not null and is valid per logic in IsValid()
            if (proposedPosition != null && IsValid(proposedPosition))
            {
                return proposedPosition;
            }
            
            // In the event that proposedPosition is null call itself again to get a valid shot
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

            // While the current turn count would allow for shots to be taken on spots that aren't the AIs ships
            // 
            if (_turnCounter < _gridSize * _gridSize - totalLength)
            {
                foreach (var position in battleshipPositions)
                {
                    if (p.X == position.X && p.Y == position.Y)
                        return false;
                }
            }
            // Once the number of spaces left to fire on would only allow for shots on our ships,
            // allow any shots that don't sink the battleship
            else 
            {
                int hitsOnBattleship = 0;
                bool shotOnBattleship = false;
                foreach (var position in battleshipPositions)
                {
                    if (_playersData[_index].StatusGrid[position.Y, position.X] == StatusType.Hit)
                        hitsOnBattleship++;
                    if (p.X == position.X && p.Y == position.Y)
                        shotOnBattleship = true;
                }

                if (hitsOnBattleship == 3 && shotOnBattleship)
                    return false;

            }

            // If all the checks have passed, this spot is valid.
            return true;

        }

        // Method to log results throughout the game.
        //
        // Checks to see if DumbPlayer AI is playing by tracking the status of shots on the usual battleship placement.
        // 
        // Passes results to corresponding _playerData to be processed.
        public void SetAttackResults(List<AttackResult> results)
        {
            // Check if DumbAI is playing, if not disable attacks on the standard battleship positions
            if (_dumbPlaying)
            {
                int hitCount = 0;
                bool shotOnDumb = false;
                foreach (var result in results)
                {
                    foreach (var position in _dumbBattleship)
                    {
                        if (result.Position.X == position.X && result.Position.Y == position.Y)
                        {
                            shotOnDumb = true;
                            if (result.ResultType == AttackResultType.Hit)
                                hitCount++;
                        }
                    }
                }

                if (shotOnDumb && hitCount == 0)
                    _dumbPlaying = false;
            }


            // On turn one when the attack results are received, create a list of PlayerData objects 
            // to store a status and probability grid
            if (_turnCounter == 1)
            {
                // Initialize _playersData to player count
                _playersData = new List<PlayerData>(results.Count+1);
                _playerCount = results.Count;
                Debug.WriteLine($"{results.Count} Players are being added to _playersData");
                foreach (var r in results)
                {
                    Debug.WriteLine($"Player {r.PlayerIndex} has been added with an initial value of {r.ResultType} in it's status grid at ({r.Position.X},{r.Position.Y})");
                    _playersData.Insert(r.PlayerIndex,new PlayerData(_gridSize,_ships, r));
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


