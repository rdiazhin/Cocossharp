using System;
using System.Collections.Generic;
using CocosSharp;
using System.Linq;
using System.Diagnostics;

namespace FlappyCloneCocossharp
{
    public class GameLayer : CCLayerColor
    {

        CCLabel scoreLabel;
        //CCLabel testNodeCountLabel;
        CCSprite player;

        bool isTapping = false;

        int score;

        float pipeGapShrink;

        float previousUpdateTime;
        float delta;
        CCVector2 velocity;
        CCVector2 Gravity;
        CCVector2 graviteStep;
        CCVector2 velocityStep;

        public GameLayer() : base(new CCColor4B(204, 255,255)) //This color is to match the sky background we will use for the parallax scrolling effect
        {
            player = new CCSprite("player-1");

            velocity = new CCVector2(0, 0);
            Gravity = new CCVector2(0, -450);
            graviteStep = new CCVector2(0, 0);
            velocityStep = new CCVector2(0, 0);

            CCAudioEngine.SharedEngine.PlayBackgroundMusic("Sounds/Alpha Dance", true);
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();

            // Use the bounds to layout the positioning of our drawable assets
            var bounds = VisibleBoundsWorldspace;

            scoreLabel = new CCLabel("Score: 0", "Arial", 40, CCLabelFormat.SystemFont);
            scoreLabel.Color = CCColor3B.Black;
            scoreLabel.Position = new CCPoint(VisibleBoundsWorldspace.MaxX - scoreLabel.ContentSize.Width * 1.5f, VisibleBoundsWorldspace.MaxY - scoreLabel.ContentSize.Height);
            scoreLabel.HorizontalAlignment = CCTextAlignment.Right;
            AddChild(scoreLabel);
            ReorderChild(scoreLabel, 20);

            //Show nodes count for debug purpose only. 
            //testNodeCountLabel = new CCLabel("Nodes: 0", "Arial", 24, CCLabelFormat.SystemFont);
            //testNodeCountLabel.Color = CCColor3B.Black;
            //testNodeCountLabel.Position = new CCPoint(VisibleBoundsWorldspace.MaxX - testNodeCountLabel.ContentSize.Width * 1.8f, VisibleBoundsWorldspace.MinY + testNodeCountLabel.ContentSize.Height);
            //testNodeCountLabel.HorizontalAlignment = CCTextAlignment.Right;
            //AddChild(testNodeCountLabel);
            //ReorderChild(testNodeCountLabel, 20);

            CreatePlayer();
            CreateBackground();
            CreateGround();
            StartPipes();

            // Register for touch events
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesEnded = OnTouchesEnded;
            touchListener.OnTouchesBegan = OnTouchesBegan;
            AddEventListener(touchListener, this);

            Schedule(GameLoop);
        }

        private void GameLoop(float currentTime)
        {

            //testNodeCountLabel.Text = string.Format("Nodes: {0}", ChildrenCount);

            delta += currentTime - previousUpdateTime;

            if (delta > 0.02f)
                delta = 0.02f;

            previousUpdateTime = currentTime;

            graviteStep = CCVector2.Multiply(Gravity, delta);

            velocity += graviteStep;

            velocityStep = CCVector2.Multiply(velocity, delta);

            //Jump
            CCVector2 jumpForce = new CCVector2(0, 310);
            if (isTapping)
            {
                velocity = CCVector2.Add(velocity, jumpForce);
            }

            CCVector2 min = new CCVector2(0, -450f);
            CCVector2 max = new CCVector2(0, 250);

            velocity = CCVector2.Clamp(velocity, min, max);

            velocityStep = CCVector2.Multiply(velocity, delta);

            player.PositionY = player.PositionY + velocityStep.Y;

            CheckCollisions();
        }

        void CheckCollisions()
        {
            if (player != null)
            {
                CCNode pipe = Children.Where(s => s.Name == "pipe").FirstOrDefault(); 

                bool didPassPipe = false;

                if (pipe != null)
                {
                    var sensor = pipe.Children.Single(s => s.Name == "sensorScore");
                    var topPipe = pipe.Children.Single(s => s.Name == "topPipe");
                    var bottomPipe = pipe.Children.Single(s => s.Name == "bottomPipe");
                    var coin = pipe.Children.Single(s => s.Name == "coin");

                    if (sensor != null)
                        didPassPipe = player.BoundingBoxTransformedToParent.IntersectsRect(sensor.BoundingBoxTransformedToWorld);

                    if (didPassPipe && pipe.PositionX + player.ContentSize.Width < player.PositionX)
                    {
                        score++;

                        if ((score / 2) > 5 && sensor.ContentSize.Height > player.ContentSize.Height * 2)
                        {
                            pipeGapShrink++;
                        }

                    }

                    if (coin != null && coin.Visible && player.BoundingBoxTransformedToWorld.IntersectsRect(coin.BoundingBoxTransformedToWorld))
                    {
                        CCAudioEngine.SharedEngine.PlayEffect("Sounds/coin");
                        coin.Visible = false;
                        if (coin.Tag == 0 || coin.Tag == 2)
                        {
                            score += 4;
                        }
                        else
                        {

                            score += 2;
                        }
                    }

                    scoreLabel.Text = string.Format("Score: {0}", score / 2);

                    if (player.BoundingBoxTransformedToWorld.IntersectsRect(topPipe.BoundingBoxTransformedToWorld) || player.BoundingBoxTransformedToWorld.IntersectsRect(bottomPipe.BoundingBoxTransformedToWorld) ||
                        player.PositionY <= VisibleBoundsWorldspace.MinY + player.ContentSize.Height)
                    {
                        EndGame();
                    }
                }
            }
        }

        void EndGame()
        {
            // Stop scheduled events as we transition to game over scene
            UnscheduleAll();
            CCAudioEngine.SharedEngine.PlayEffect("Sounds/explosion");
            CCAudioEngine.SharedEngine.StopBackgroundMusic();

            CCScene gameOverScene = new CCScene(GameView);
            var transitionToGameOver = new CCTransitionRotoZoom(1.0f, gameOverScene);
            gameOverScene.AddLayer(new GameOverLayer(score / 2));
            Director.ReplaceScene(transitionToGameOver);
        }
        void CreatePlayer()
        {
            player.Name = "player";
            player.Position = new CCPoint(VisibleBoundsWorldspace.MinX + player.ContentSize.Width * 1.5f, ContentSize.Height * 0.75f);
            AddChild(player);
            this.ReorderChild(player, 10);

            CCAnimation playerAnimation = new CCAnimation();

            for (int i = 1; i <= 3; i++)
            {
                playerAnimation.AddSpriteFrame(new CCSprite(string.Format("player-{0}", i)));
            }
            playerAnimation.AddSpriteFrame(playerAnimation.Frames[1].SpriteFrame);
            playerAnimation.DelayPerUnit = 0.01f;

            var animate = new CCAnimate(playerAnimation);
            var animationAction = new CCRepeatForever(animate);

            player.RunAction(animationAction);

        }

        void CreateBackground()
        {
            var backgroundTexture = new CCTexture2D("background");

            for (int i = 0; i <= 1; i++)
            {
                var background = new CCSprite(backgroundTexture);
                background.Name = "background";
                background.AnchorPoint = CCPoint.Zero;
                background.Position = new CCPoint((background.ContentSize.Width * i) - 1 * i, 20);
                AddChild(background);
                this.ReorderChild(background, -30);

                var moveLeft = new CCMoveBy(10, new CCPoint(-background.ContentSize.Width, 0));
                var moveReset = new CCMoveBy(0, new CCPoint(background.ContentSize.Width, 0));
                var moveLoop = new CCSequence(moveLeft, moveReset);
                var moveforever = new CCRepeatForever(moveLoop);

                background.RunAction(moveforever);
            }
        }

        void CreateGround()
        {
            var groundTexture = new CCTexture2D("ground");

            for (int i = 0; i <= 1; i++)
            {
                var ground = new CCSprite(groundTexture);
                ground.Position = new CCPoint(((ground.ContentSize.Width / 2) + (ground.ContentSize.Width * i)), ground.ContentSize.Height / 2);
                AddChild(ground);
                this.ReorderChild(ground, -10);

                var moveLeft = new CCMoveBy(5, new CCPoint(-ground.ContentSize.Width, 0));
                var moveReset = new CCMoveBy(0, new CCPoint(ground.ContentSize.Width, 0));
                var moveLoop = new CCSequence(moveLeft, moveReset);
                var moveforever = new CCRepeatForever(moveLoop);

                ground.RunAction(moveforever);
            }

        }

        void CreatePipesAndCoins()
        {

            var pipeNode = new CCNode();
            pipeNode.AnchorPoint = CCPoint.Zero;
            pipeNode.Name = "pipe";

            var bottomPipe = new CCSprite("pipe");
            bottomPipe.AnchorPoint = CCPoint.Zero;
            bottomPipe.ScaleX = 0.5f;
            bottomPipe.FlipY = true;
            bottomPipe.Name = "bottomPipe";
            bottomPipe.Position = new CCPoint(0, 0);

            var pipeGap = new CCDrawNode();
            pipeGap.AnchorPoint = CCPoint.Zero;
            pipeGap.DrawRect(new CCRect(0, 0, 1, (player.ContentSize.Height * 2.8f) - pipeGapShrink), CCColor4B.Red);
            pipeGap.Name = "sensorScore";
            pipeGap.Position = new CCPoint(bottomPipe.ScaledContentSize.Width, bottomPipe.ContentSize.Height);
            pipeGap.Visible = false;

            var topPipe = new CCSprite("pipe");
            topPipe.AnchorPoint = CCPoint.Zero;
            topPipe.ScaleX = 0.5f;
            topPipe.Name = "topPipe";
            topPipe.Position = new CCPoint(0, bottomPipe.ContentSize.Height + pipeGap.ContentSize.Height);

            pipeNode.AddChild(bottomPipe);
            pipeNode.AddChild(pipeGap);
            pipeNode.AddChild(topPipe);


            AddChild(pipeNode);
            this.ReorderChild(pipeNode, -20);

            float xPosition = ContentSize.Width;
            float yPosition = CCRandom.Next(-530, 5);

            pipeNode.Position = new CCPoint(xPosition, yPosition);

            //Create coin
            var coin = new CCSprite("gold_1");
            coin.Name = "coin";
            coin.Scale = 0.75f;

            int coinPos = CCRandom.Next(0, 3);

            if (coinPos == 0)
            {
                coin.Tag = 0;
                coin.PositionX = topPipe.PositionX + 190;
                coin.PositionY = topPipe.PositionY + 120;
            } else if (coinPos == 1)
            {
                coin.Tag = 1;
                coin.PositionX = topPipe.PositionX + coin.ContentSize.Width / 3;
                coin.PositionY = topPipe.PositionY - pipeGap.ContentSize.Height / 2;
            }
            else
            {
                coin.Tag = 2;
                coin.PositionX = topPipe.PositionX + 190;
                coin.PositionY = topPipe.PositionY - pipeGap.ContentSize.Height - 120;
            }


            pipeNode.AddChild(coin);
            this.ReorderChild(coin, 10);

            CCAnimation coinAnimation = new CCAnimation();

            for (int i = 1; i <= 4; i++)
            {
                coinAnimation.AddSpriteFrame(new CCSprite(string.Format("gold_{0}", i)));
            }
            coinAnimation.DelayPerUnit = 0.1f;

            var animate = new CCAnimate(coinAnimation);
            var animationAction = new CCRepeatForever(animate);

            coin.RunAction(animationAction);



            float endPosition = ContentSize.Width + (pipeNode.ContentSize.Width * 2) + 190;
            var moveAction = new CCMoveBy(6.2f, new CCPoint(-endPosition, 0));
            var remove = new CCRemoveSelf();
            var moveSequence = new CCSequence(moveAction, remove);
            pipeNode.RunAction(moveSequence);



        }

        void StartPipes()
        {
            var create = new CCCallFunc(CreatePipesAndCoins);

            var wait = new CCDelayTime(3);
            var sequence = new CCSequence(create, wait);
            var repeartForever = new CCRepeatForever(sequence);
            this.RunAction(repeartForever);
        }

        private void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count > 0)
            {
                isTapping = true;
                CCAudioEngine.SharedEngine.PlayEffect("Sounds/jump");
            }
        }
        void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count > 0)
            {
                isTapping = false;
            }
        }

      

    }
}

