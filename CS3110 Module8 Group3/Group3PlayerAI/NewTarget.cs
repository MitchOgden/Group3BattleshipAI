using System.Collections.Generic;

namespace Module8
{
    public class NewTarget
    {

        public Position GridPosition { get; set; }
        public CardinalDirection CurrentDirection = CardinalDirection.North;
        private int _nY,_sY,_eX,_wX = 0;
        private bool _validPosition = false;


        // Constructor that grabs the player index, the position reported as a hit, and the current state of the status grid.
        public NewTarget(Position position)
        {
            GridPosition = position;
        }

        // Will take the current attack direction and see if the next position is valid,
        // if not it will change to the next direction and repeat the process until a valid
        // point is found.
        public Position GetNextPosition(StatusType[,] statGrid)
        {
            _validPosition = false;
            
            Position result = null;

            do
            {
                if (CurrentDirection == CardinalDirection.North)
                {
                    _nY--;
                    result = new Position(GridPosition.X, GridPosition.Y + _nY);
                }

                if (CurrentDirection == CardinalDirection.East)
                {
                    _eX++;
                    result = new Position(GridPosition.X + _eX, GridPosition.Y);
                }

                if (CurrentDirection == CardinalDirection.South)
                {
                    _sY++;
                    result = new Position(GridPosition.X, GridPosition.Y + _sY);
                }

                if (CurrentDirection == CardinalDirection.West)
                {
                    _wX--;
                    result = new Position(GridPosition.X + _wX, GridPosition.Y);
                }

                if (statGrid[result.Y, result.X] != StatusType.Unknown)
                {
                    switch (CurrentDirection)
                    {
                        case CardinalDirection.North:
                            CurrentDirection = CardinalDirection.East;
                            break;
                        case CardinalDirection.East:
                            CurrentDirection = CardinalDirection.South;
                            break;
                        case CardinalDirection.South:
                            CurrentDirection = CardinalDirection.West;
                            break;
                        case CardinalDirection.West:
                            result = null;
                            _validPosition = true;
                            break;
                    }
                }

                if (statGrid[result.Y, result.X] == StatusType.Unknown)
                    _validPosition = true;


            } while (!_validPosition);

            return result;
        }
        
    }
}