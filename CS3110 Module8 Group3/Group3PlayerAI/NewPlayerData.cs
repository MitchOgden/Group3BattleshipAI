﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Module8
{
    public class NewPlayerData
    {
        public StatusType[,] StatusGrid { get; set; } // Used to store status values per grid point
        public int ShipsLeft { get; set; } // Counter from the value of ships left.
        public int Index { get; set; } // Store player index
        private int _gridSize;
        public Stack<NewTarget> TargetStack { get; set; } // Store identified targets for this player
        public NewTarget CurrentTarget;
        private List<Ship> _ships;
        private int[,] _probabilityGrid;

        public NewPlayerData(int gridSize, Ships ships, AttackResult result)
        {
            // Initialize variables with parameters 
            ShipsLeft = ships._ships.Count;
            Index = result.PlayerIndex;
            _ships = ships._ships;
            _gridSize = gridSize;
            
            // Create status grid
            StatusGrid = new StatusType[gridSize, gridSize];

            // Initialize values in status grid to 0
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                    StatusGrid[i, j] = StatusType.Unknown;
            }

            // Log initial result
            if (result.ResultType == AttackResultType.Hit)
            {
                StatusGrid[result.Position.Y,result.Position.X] = StatusType.Hit;
                TargetStack.Push(new NewTarget(result.Position));
            }
            
            if (result.ResultType == AttackResultType.Miss)
            {
                StatusGrid[result.Position.Y,result.Position.X] = StatusType.Miss;
            }

        }

        //    MostProbablePosition(): 
        //    Start at (0,0) and check if the largest ship can fit vertically against
        //    current list of misses/sunk ships. If it fits, then add a point to all
        //    positions the ship fits in in ProbabilityGrid. Iterate through each position Then start over
        //    at (0,0) and check horizontally if the ship fits and if it does add a point
        //    to all positions the ship fits in in ProbabilityGrid.
        //
        //    DO not apply points to positions that hold our ship until
        //
        //    Do this process again for the next size ship that the player should have left.
        //
        //    Return position from the list of positions with the highest score on the ProbabilityGrid.

        
        private Position MostProbablePosition()
        {
            Debug.WriteLine("Called MostProbablePosition");
            
            _probabilityGrid = new int[_gridSize,_gridSize];
            
            Position mostProbable = new Position(0, 0); // Initialize to origin at the beginning of each execute
            int largestScore = 0;
            
            
            #if DEBUG

            DebugStatusGrid();

            #endif
            
                // Assign horizontal probability points
                for (int i = 0; i < StatusGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < StatusGrid.GetLength(1); j++)
                    {
                        for (int s = 5; s > 1; s--)
                        {
                            bool fits = true;
                            for (int p = 0; p < s; p++)
                            {
                                if (i + p >= _gridSize)
                                {
                                    fits = false;
                                    break;
                                }

                                if (StatusGrid[i + p, j] != StatusType.Unknown && StatusGrid[i + p, j] != StatusType.Hit)
                                    fits = false;
                            }

                            if (fits)
                            {
                                for (int p = 0; p < s; p++)
                                {
                                    _probabilityGrid[i + p, j] += 1;
                                }

                            }
                        }
                    }
                } // End of horizontal probability point assignment

                // Assign vertical probability points
                for (int i = 0; i < StatusGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < StatusGrid.GetLength(1); j++)
                    {
                        for (int s = 5; s > 1; s--)
                        {
                            bool fits = true;
                            for (int p = 0; p < s; p++)
                            {
                                if (j + p >= _gridSize)
                                {
                                    fits = false;
                                    break;
                                }
                                if (StatusGrid[i, j + p] != StatusType.Unknown && StatusGrid[i, j + p] != StatusType.Hit)
                                    fits = false;
                            }

                            if (fits)
                            {
                                for (int p = 0; p < s; p++)
                                {
                                    _probabilityGrid[i, j + p] += 1;
                                }
                            }

                        }
                    }
                } // End of vertical probability point assignment

                // Find position with highest score 
                for (int i = 0; i < _probabilityGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < _probabilityGrid.GetLength(1); j++)
                    {
                        
                        if (_probabilityGrid[i, j] > largestScore)
                        {
                            largestScore = _probabilityGrid[i, j];
                            mostProbable = new Position(j, i);
                        }

                    }
                }
                
                Debug.WriteLine($"Most probable position for player {Index} was ({mostProbable.X},{mostProbable.Y})");
                return mostProbable;
        }

        
        
        // SunkShip()
        //
        // In the event that a ship is sunk, mark related hit positions as sunk.
        
        public void SunkShip(ShipTypes shipType, Position position)
        {
            Debug.WriteLine("Called SunkShip()");
            
            int shipLength = 0; // Store lenght of sunk ship
            List<Position> possiblePositions = new List<Position>(); // List of the positions to be marked sunk
            bool valid = true; // Bool used as exit point for while loops
            
            // Switch case for figuring out length based on reported ship type

            switch (shipType)
            {
                case ShipTypes.AircraftCarrier:
                    shipLength = 5;
                    break;
                case ShipTypes.Battleship:
                    shipLength = 4;
                    break;
                case ShipTypes.Destroyer:
                case ShipTypes.Submarine:
                    shipLength = 3;
                    break;
                case ShipTypes.PatrolBoat:
                    shipLength = 2;
                    break;
            }

#if DEBUG

            DebugStatusGrid();

#endif
            
            Debug.WriteLine($"Length of ship sunk is {shipLength}");
            
            // Check for hit positions north of the sunk point and add them to possiblePositions
            // until a position isn't a hit or we have all the hit positions we need for the shipLength
            while (valid)
            {
                Debug.WriteLine($"PossiblePosition count before(north-while): {possiblePositions.Count}");
                
                for (int i = position.Y - 1; i >= 0; i--)
                {
                    Debug.WriteLine($"Checking position north of origin @ ({position.X},{i})");

                    if (StatusGrid[i, position.X] == StatusType.Hit)
                    {
                        Debug.WriteLine($"Adding hit position north of origin @ ({position.X},{i})");
                        possiblePositions.Add(new Position(position.X, i));
                    }

                    if (StatusGrid[i, position.X] != StatusType.Hit)
                    {
                        valid = false;
                        break;
                    }
                }
                
                if (possiblePositions.Count == shipLength - 1 || possiblePositions.Count == 0)
                {
                    valid = false;
                }
                
                Debug.WriteLine($"PossiblePosition count after(north-while): {possiblePositions.Count}");

            }

            valid = true; // Set back to true for next while

            // Check for hit positions south of the sunk point and add them to possiblePositions
            // until a position isn't a hit or we have all the hit positions we need for the shipLength
            while (valid && possiblePositions.Count < shipLength-1)
            {
                Debug.WriteLine($"PossiblePosition count before(south-while): {possiblePositions.Count}");
                
                for (int i = position.Y + 1; i < StatusGrid.GetLength(1); i++)
                {

                    Debug.WriteLine($"Checking position south of origin @ ({position.X},{i})");

                    if (StatusGrid[i, position.X] == StatusType.Hit)
                    {
                        Debug.WriteLine($"Adding hit position south of origin @ ({position.X},{i})");
                        possiblePositions.Add(new Position(position.X, i));
                    }
                    
                    if (StatusGrid[i, position.X] != StatusType.Hit)
                    {
                        valid = false;
                        break;
                    }
                    
                }
                
                if (possiblePositions.Count == shipLength - 1 || possiblePositions.Count == 0)
                {
                    valid = false;
                }
                Debug.WriteLine($"PossiblePosition count after(south-while): {possiblePositions.Count}");

            }

            valid = true; // Set back to true for next while

            
            if (possiblePositions.Count < shipLength-1)
            {
                possiblePositions.Clear();
                
                // Check for hit positions west of the sunk point and add them to possiblePositions
                // until a position isn't a hit or we have all the hit positions we need for the shipLength
                while (valid)
                {
                    
                    Debug.WriteLine($"PossiblePosition count before(left-while): {possiblePositions.Count}");
                    
                    for (int i = position.X - 1; i >= 0; i--)
                    {
                        Debug.WriteLine($"Checking position left of origin @ ({i},{position.Y})");

                        if (StatusGrid[position.Y, i] == StatusType.Hit)
                        {
                            Debug.WriteLine($"Adding hit position left of origin @ ({i},{position.Y})");
                            possiblePositions.Add(new Position(i, position.Y));
                        }
                        
                        if (StatusGrid[position.Y, i] != StatusType.Hit)
                        {
                            valid = false;
                            break;
                        }
                        
                    }
                    if (possiblePositions.Count == shipLength - 1 || possiblePositions.Count == 0)
                    {
                        valid = false;
                    }
                    Debug.WriteLine($"PossiblePosition count before(left-while): {possiblePositions.Count}");
                    
                }

                valid = true; // Set back to true for next while

                // Check for hit positions east of the sunk point and add them to possiblePositions
                // until a position isn't a hit or we have all the hit positions we need for the shipLength
                while (valid & possiblePositions.Count < shipLength-1)
                {
                    
                    Debug.WriteLine($"PossiblePosition count before(right-while): {possiblePositions.Count}");

                    for (int i = position.X + 1; i < StatusGrid.GetLength(0); i++)
                    {

                        Debug.WriteLine($"Checking position right of origin @ ({i},{position.Y})");

                        if (StatusGrid[position.Y, i] == StatusType.Hit)
                        {
                            Debug.WriteLine($"Adding hit position right of origin @ ({i},{position.Y})");
                            possiblePositions.Add(new Position(i, position.Y));
                        }
                        
                        if (StatusGrid[position.Y, i] != StatusType.Hit)
                        {
                            valid = false;
                            break;
                        }
                        
                    }
                    Debug.WriteLine($"PossiblePosition count after(right-while): {possiblePositions.Count}");
                }
            }
            
            // Change each position to Sunk
            foreach (var t in possiblePositions)
                StatusGrid[t.Y, t.X] = StatusType.Sank;
            
            // Change origin position to sunk
            StatusGrid[position.Y, position.X] = StatusType.Sank;

            ShipsLeft--;
        }

        public void DebugStatusGrid()
        {
            Debug.WriteLine($"Player {Index} Status Grid");
            Debug.WriteLine("________");
            
            for (int i = 0; i < StatusGrid.GetLength(0); i++)
            {
                for (int j = 0; j < StatusGrid.GetLength(1); j++)
                {
                    
                    if(StatusGrid[i,j] == StatusType.Hit)
                        Debug.Write("| H|");
                    if(StatusGrid[i,j] == StatusType.Miss)
                        Debug.Write("| M|");
                    if(StatusGrid[i,j] == StatusType.Sank)
                        Debug.Write("| S|");
                    if(StatusGrid[i,j] == StatusType.Unknown)
                        Debug.Write("| U|");
                    
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("________");
        }

    }
}