using System;
using System.Collections;
using UnityEngine;

namespace Sandwicher
{
    public enum MoveDir { Down, Up, Right, Left, Stop }
    public enum CharAction { Idle, Walk }
    public static class MoveDirUtility
    {
        public static Vector2 ToVector(this MoveDir dir)
        {
            return dir switch
            {
                MoveDir.Down => Vector2.down,
                MoveDir.Up => Vector2.up,
                MoveDir.Right => Vector2.right,
                MoveDir.Left => Vector2.left,
                _ => Vector2.zero
            };
        }

        public static MoveDir ToMoveDir(this Vector2 dir)
        {
            if (dir == Vector2.down) return MoveDir.Down;
            else if (dir == Vector2.up) return MoveDir.Up;
            else if (dir == Vector2.right) return MoveDir.Right;
            else if (dir == Vector2.left) return MoveDir.Left;
            else return MoveDir.Stop;

        }
    }

    public enum OrderIngIndex { Any, Bottom, Top, ListIndex }
    public enum OrderPhrase { Text, Ingredients, GeneralName, RecipeName }
    public enum GameState { Sandwich, Cashier, Serve, ThrowAway }
    public enum IngredientShape { Flat, Rounded }

    public enum SoundVolume { Full, Medium, Quiet, Mute }
    
    [Flags]
    public enum RatingFeedback
    {
        None = 0,
        WrongRecipe = 1,
        Late = 2,
        Pricey = 4
    }
}