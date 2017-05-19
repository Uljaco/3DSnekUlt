using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace _3DSnekUlt
{
    public class VisualOutputManager
    {
        private ContentManager Content;
        private GraphicsDevice graphicsDevice;
        private GraphicsDeviceManager graphics;
        private float aspectRatio;
        private float farPlaneDistance = 25000f;
        private SpriteBatch spriteBatch;//will be used for playing video

        private Model snekTextModel, snekTextSquareModel, snakeHeadModel, arenaModel, gameOverTextModel,
                        gameOverOptionsTextModel, ultimateTextModel, skyboxModel;
        private VideoPlayer videoPlayer;
        private Video crazyDogManVideo;
        private Texture2D videoTexture;

        private Vector3 cameraPosition, cameraLookAt;
        private float rotation = 0f;//of model(s)
        private float skyboxRotation = 0f;

        private float zoomFactor = 6000f; 
        private float yaw = 180f;
        private float pitch = 180f;

        public VisualOutputManager(GraphicsDeviceManager gdm, GraphicsDevice gd, ContentManager content)
        {
            Content = content;
            graphics = gdm;
            graphicsDevice = gd;
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;//required data for 3D rendering
            spriteBatch = new SpriteBatch(gd);
            graphics.PreferredBackBufferHeight = 850;//set viewing window dimensions
            graphics.PreferredBackBufferWidth = 1000;
            graphics.ApplyChanges();//need to explicitly apply the changes since we are outside the Game constructor

            cameraLookAt = Vector3.Zero;//origin
            cameraPosition = new Vector3(0, 800, 4200);//new Vector3(700, 500, -400);

            videoPlayer = new VideoPlayer();

            loadModels();
            loadVideos();
        }

        private void loadModels()
        {
            //Load models in for future rendering.
            snekTextModel = Content.Load<Model>("Models/3DSnekText");
            snekTextSquareModel = Content.Load<Model>("Models/3DSnekSquareText");
            snakeHeadModel = Content.Load<Model>("Models/snakeHead");
            arenaModel = Content.Load<Model>("Models/arena");
            gameOverTextModel = Content.Load<Model>("Models/GameOverText");
            gameOverOptionsTextModel = Content.Load<Model>("Models/GameOverMenuText");
            ultimateTextModel = Content.Load<Model>("Models/ultimateText");
            skyboxModel = Content.Load<Model>("Models/skybox");
        }
        private void loadVideos()
        {
            crazyDogManVideo = Content.Load<Video>("Videos/crazyDogMan");
        }

        public void draw(Player player, Vector3 foodLocation, bool foodIsSuperFood)
        {
            graphics.GraphicsDevice.Clear(Color.Aquamarine);//Set background color
            setCamera(player);
            drawPlayer(player);
            if (foodIsSuperFood)
            {
                drawModel(ultimateTextModel, foodLocation, rotation, Color.Red.ToVector3());
            }
            else
            {
                drawModel(snekTextModel, foodLocation, rotation, Color.White.ToVector3());
            }
            if (player.enraged)
            {
                drawModel(snekTextSquareModel, Vector3.Up * 700, -rotation, (float)Math.Sin(System.Environment.TickCount) + 3.5f, Color.MediumVioletRed.ToVector3());//super uigi mode
                drawUltimatesAtCorners();
            }
            else
            {
                drawModel(snekTextSquareModel, Vector3.Up*700, -rotation, 2.5f, Color.BlanchedAlmond.ToVector3());//regular, easy to read
                //drawModel(snekTextSquareModel, Vector3.Up * 700, -rotation, (float)Math.Sin(System.Environment.TickCount / 100) + 3.5f, Color.BlanchedAlmond.ToVector3());
            }
            drawModel(arenaModel, Vector3.Zero, Color.White.ToVector3());
            drawModel(skyboxModel, Vector3.Down * 3400, skyboxRotation += .003f, 12f, Color.White.ToVector3());

            if (videoPlayer.State == MediaState.Playing)
            {
                continueVideo();
            }
        }

        private void drawPlayer(Player player)
        {
            if (player.enraged)
            {
                drawModel(snakeHeadModel, player.coords, rotation += .05f, .5f*(float)Math.Sin(System.Environment.TickCount/250) + 2f, Color.Red.ToVector3());
            }
            else
            {
                drawModel(snakeHeadModel, player.coords, rotation += .025f, Color.Yellow.ToVector3());//Draw the head
            }
         
            if (player.tail.Count != 0)//if there is a tail, then draw it
            {
                float scale = 1f;
                float scaleInterval = 1f / (player.tail.Count + 1f);//choose a rate for the tail piece sizes to decrease the closer they are to the end
                LinkedListNode<TailPiece> currentTailPiece = player.tail.First;
                while (currentTailPiece != null)
                {
                    drawModel(snakeHeadModel, currentTailPiece.Value.coords, 0f, Math.Max(scale, .5f), Color.White.ToVector3());//scale down, but not too small
                    scale -= scaleInterval;
                    currentTailPiece = currentTailPiece.Next;
                }
            }
        }

        private void drawUltimatesAtCorners()
        {
            drawSinusoidalModel(ultimateTextModel, (Vector3.Left + Vector3.Forward) * 2000 + Vector3.Up*300);
            drawSinusoidalModel(ultimateTextModel, (Vector3.Left + Vector3.Backward) * 2000 + Vector3.Up * 300);
            drawSinusoidalModel(ultimateTextModel, (Vector3.Right + Vector3.Forward) * 2000 + Vector3.Up * 300);
            drawSinusoidalModel(ultimateTextModel, (Vector3.Right + Vector3.Backward) * 2000 + Vector3.Up * 300);
        }

        public void drawGameOverMenu()
        {
            drawModel(gameOverTextModel, Vector3.Up * 500, 0f, 3.5f, Color.Red.ToVector3());
            drawModel(gameOverOptionsTextModel, Vector3.Zero, 0f, 3.5f, Color.AliceBlue.ToVector3());
        }

        private void setCamera(Player player)//maybe just for testing, until we add player camera control, this camera will just follow the player
        {
            
        }

        public void updateCamera(float yawChange, float pitchChange, float zoomChange)
        {
            yaw += yawChange;
            pitch += pitchChange;
            zoomFactor += zoomChange;

            cameraPosition = Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(yaw, pitch, 0f));
            cameraPosition *= zoomFactor;
            cameraPosition += cameraLookAt;
        }

        /// <summary>
        /// Allow external command to begin playing the video. 
        /// </summary>
        public void startVideo()
        {
            videoPlayer.Play(crazyDogManVideo);
        }
        /// <summary>
        /// Continue the video by drawing the next frame (texture). 
        /// Video's sound will continue to play even if this method is not called.
        /// </summary>
        private void continueVideo()
        {
            videoTexture = videoPlayer.GetTexture();
            //2D Spaces upon which the video will be placed
            Rectangle screen1 = new Rectangle(0, graphics.GraphicsDevice.Viewport.Height - 250, 250, 250),//bottom left
                        screen2 = new Rectangle(graphics.GraphicsDevice.Viewport.Width - 250, graphics.GraphicsDevice.Viewport.Height - 250, 250, 250),//bottom right
                        screen3 = new Rectangle(graphics.GraphicsDevice.Viewport.Width - 250, 0, 250, 250),//top right
                        screen4 = new Rectangle(0, 0, 250, 250);//top left
            if (videoTexture != null)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(videoTexture, screen1, Color.White);
                spriteBatch.Draw(videoTexture, screen2, Color.White);
                spriteBatch.Draw(videoTexture, screen3, Color.White);
                spriteBatch.Draw(videoTexture, screen4, Color.White);
                spriteBatch.End();
            }

            graphicsDevice.BlendState = BlendState.Opaque;//These 3 calls are necessary to retain proper Model drawing since we also use a SpriteBatch 
            graphicsDevice.DepthStencilState = DepthStencilState.Default;//SpriteBatches set the States to different values when used
            //graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
        }

        private void drawModel(Model model, Vector3 modelPosition, Vector3 color)
        {
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {   //TO-DO: precalculate anything that does not change
                    effect.EnableDefaultLighting();
                    effect.DiffuseColor = color;
                    effect.DirectionalLight0.Direction = new Vector3(0f, 0.0f, 0.0f);
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(modelPosition);//change the position of the model in the world
                    effect.View = Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up); //change the position and direction of the camera
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(45.0f), aspectRatio,
                        1.0f, farPlaneDistance);//control how the view of the 3D world is turned into a 2D image
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }
        private void drawModel(Model model, Vector3 modelPosition, float rotation, Vector3 color)
        {
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {   //TO-DO: precalculate anything that does not change
                    effect.EnableDefaultLighting();
                    effect.DiffuseColor = color;
                    effect.DirectionalLight0.Direction = new Vector3(0f, 0.0f, 0.0f);
                    effect.World = transforms[mesh.ParentBone.Index] 
                        * Matrix.CreateRotationY(rotation)
                        * Matrix.CreateTranslation(modelPosition);//change the position of the model in the world
                    effect.View = Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up); //change the position and direction of the camera
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(45.0f), aspectRatio,
                        1.0f, farPlaneDistance);//control how the view of the 3D world is turned into a 2D image
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }
        private void drawModel(Model model, Vector3 modelPosition, float rotation, float scale, Vector3 color)
        {
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {   //TO-DO: precalculate anything that does not change
                    effect.EnableDefaultLighting();
                    effect.DiffuseColor = color;
                    effect.DirectionalLight0.Direction = new Vector3(0f, 0.0f, 0.0f);
                    effect.World = transforms[mesh.ParentBone.Index]
                        * Matrix.CreateScale(scale)
                        * Matrix.CreateRotationY(rotation)
                        * Matrix.CreateTranslation(modelPosition);//change the position of the model in the world
                    effect.View = Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up); //change the position and direction of the camera
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(45.0f), aspectRatio,
                        1.0f, farPlaneDistance);//control how the view of the 3D world is turned into a 2D image
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }
        private void drawSinusoidalModel(Model model, Vector3 modelPosition)
        {
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {   //TO-DO: precalculate anything that does not change
                    effect.EnableDefaultLighting();
                    effect.DiffuseColor = Color.Red.ToVector3();
                    effect.DirectionalLight0.Direction = new Vector3(0f, 0.0f, 0.0f);
                    effect.World = transforms[mesh.ParentBone.Index]
                        * Matrix.CreateScale(2.0f)
                        * Matrix.CreateRotationY(rotation)
                        * Matrix.CreateRotationX((float)Math.Sin(System.Environment.TickCount/4000) + 3.5f)
                        * Matrix.CreateRotationZ((float)Math.Sin(System.Environment.TickCount / 2500) + 3.5f)
                        * Matrix.CreateTranslation(modelPosition);//change the position of the model in the world
                    effect.View = Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up); //change the position and direction of the camera
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(45.0f), aspectRatio,
                        1.0f, farPlaneDistance);//control how the view of the 3D world is turned into a 2D image
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }
    }
}
