using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace _3DSnekUlt
{
    class InputManager
    {
        /// <summary>
        /// Change player direction using WASD / arrow keys
        /// </summary>
        public void handleMotionControl(Player player)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            //Just turning left and right -> Just A and D keys
            Keys [] keys = keyboardState.GetPressedKeys();
            if (keys.Contains<Keys>(Keys.A))
            {
                player.changeDirection(true);//turn left
            }
            else if (keys.Contains<Keys>(Keys.D))
            {
                player.changeDirection(false);//turn right
            }
        }

        /// <summary>
        /// Rotate and angle camera based on keyboard input
        /// </summary>
        public void handleCameraControl(VisualOutputManager vom)
        {
            //MouseState mouseState = Mouse.GetState();//or use arrow keys
            Keys[] keys = Keyboard.GetState().GetPressedKeys();
            float   yawChange = 0,
                    pitchChange = 0,
                    zoomChange = 0;
            if (keys.Contains<Keys>(Keys.Left))
            {
                yawChange = .05f;
            }
            else if (keys.Contains<Keys>(Keys.Right))
            {
                yawChange = -.05f;
            }
            if (keys.Contains<Keys>(Keys.Up))
            {
                pitchChange = .03f;
            }
            else if (keys.Contains<Keys>(Keys.Down))
            {
                pitchChange = -.03f;
            }
            if (keys.Contains<Keys>(Keys.W))
            {
                zoomChange = -10f;
            }
            else if (keys.Contains<Keys>(Keys.S))
            {
                zoomChange = 10f;
            }
            vom.updateCamera(yawChange, pitchChange, zoomChange);
        }

        /// <summary>
        /// Choose to Replay/Restart or Quit the game
        /// </summary>
        public void handleGameOverMenuChoice(Game theGame, out bool needToRestart)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            Keys[] keys = keyboardState.GetPressedKeys();

            needToRestart = false;
            if (keys.Contains<Keys>(Keys.R))
            {
                needToRestart = true;//send signal to restart the game
            }
            else if (keys.Contains<Keys>(Keys.Q))
            {
                theGame.Exit();//quit the game
            }
        }

    }
}
