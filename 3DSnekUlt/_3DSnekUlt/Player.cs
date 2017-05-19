using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace _3DSnekUlt
{
    public class Player
    {
        public Vector3 coords { get; set; }   //xyz coordinates, maybe just need xz if we keep y uniform, but keeping in 3d vector might make it easier to compute?
        public LinkedList<TailPiece> tail { get; } //contains the player's recent previous locations (which are the current locations of the tail)
        //TailPiece as opposed to just xy coordinates because we might want to make different tail pieces appear differently
        //but maybe even then we could use just xy and implement the different appearance as function of how long the tail is to begin with
        private Vector3 currentDirection; //player's current velocity/motion vector
        private int gridSpaceFactor; //the dist from one grid location to the next (displacement when moving)

        public bool enraged { get; set; }//if the player is under effects of a super/red food!
        //public float enrageTimer { get; set; }//the countdown til player's enrage fades

        public Player(int gridSpaceFactor)
        {
            coords = Vector3.Zero;//player spawns in middle of map
            currentDirection = Vector3.Backward;//moving towards the camera
            tail = new LinkedList<TailPiece>();
            this.gridSpaceFactor = gridSpaceFactor;
            enraged = false;
        }

        /// <summary>
        /// Move the player and its tail through the world space.
        /// </summary>
        public void move()
        {
            //update player and tail coordinates
            LinkedListNode<TailPiece> currentTailPiece = tail.Last;
            while (currentTailPiece != null)
            {
                if(currentTailPiece.Previous != null)
                {
                    currentTailPiece.Value.coords = currentTailPiece.Previous.Value.coords;//each tail piece is moved to the 'old' place of its preceding piece
                    currentTailPiece = currentTailPiece.Previous;
                }
                else//if this is the tail piece right behind the head,
                {
                    currentTailPiece.Value.coords = coords;//then give it the coords of the head
                    break;//and exit the loop immediately
                }
            }
            coords += currentDirection * gridSpaceFactor;//move the player's head
        }

        /// <summary>
        /// Change the player's current direction. Take a boolean (true if turning left, false if right) and update player's direction
        /// based on the previous currentDirection and which way they are turning.
        /// </summary>
        public void changeDirection(bool toLeft)//if not to the left, then it is to the right (can only change direction in 2 ways L/R)
        {
            if (toLeft)//if turning LEFT
            {
                if (currentDirection.Equals(Vector3.Backward))
                {
                    currentDirection = Vector3.Right;
                }else if (currentDirection.Equals(Vector3.Forward))
                {
                    currentDirection = Vector3.Left;
                }else if (currentDirection.Equals(Vector3.Left))
                {
                    currentDirection = Vector3.Backward;
                }else if (currentDirection.Equals(Vector3.Right))
                {
                    currentDirection = Vector3.Forward;
                }
            }
            else//if turning RIGHT
            {
                if (currentDirection.Equals(Vector3.Backward))
                {
                    currentDirection = Vector3.Left;
                }
                else if (currentDirection.Equals(Vector3.Forward))
                {
                    currentDirection = Vector3.Right;
                }
                else if (currentDirection.Equals(Vector3.Left))
                {
                    currentDirection = Vector3.Forward;
                }
                else if (currentDirection.Equals(Vector3.Right))
                {
                    currentDirection = Vector3.Backward;
                }
            }
        }

        /// <summary>
        /// Append a tail piece to the end of the player's tail. Places new segment in same spot as tail's previous end.
        /// </summary>
        public void addTailPiece()
        {
            if (tail.Last == null)
            {
                tail.AddLast(new TailPiece(coords));
            }
            else
            {
                tail.AddLast(new TailPiece(tail.Last.Value.coords));
            }
        }

    }

    public class TailPiece
    {
        public Vector3 coords { set; get; }

        public TailPiece(Vector3 coordinates)
        {
            coords = coordinates;
        }
    }
}
