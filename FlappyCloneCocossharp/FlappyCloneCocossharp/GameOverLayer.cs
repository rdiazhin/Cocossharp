using System;
using System.Collections.Generic;
using CocosSharp;

namespace FlappyCloneCocossharp
{
    public class GameOverLayer : CCLayerColor
    {

        //string scoreMessage = string.Empty;
        CCLabel scoreLabel;
        public GameOverLayer(int score) : base(CCColor4B.Black)
        {
            scoreLabel = new CCLabel(String.Format("Your Score: {0}", score), "Arial", 40, CCLabelFormat.SystemFont);
            scoreLabel.Color = CCColor3B.White;
            AddChild(scoreLabel);

        }

        protected override void AddedToScene()
        {
            base.AddedToScene();

            var background = new CCSprite("textGameOver");
            background.Position = new CCPoint(ContentSize.Width / 2, ContentSize.Height / 2);
            AddChild(background);
            scoreLabel.Position = new CCPoint(ContentSize.Width / 2, ContentSize.Height / 2 + background.ContentSize.Height);

            var startLabel = new CCLabel("Tap to start a new game", "Arial", 40, CCLabelFormat.SystemFont);
            startLabel.Color = CCColor3B.White;
            startLabel.Position = new CCPoint(ContentSize.Width / 2, ContentSize.Height / 2 - background.ContentSize.Height);
            AddChild(startLabel);

            // Register for touch events
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = OnTouchesBegan;
            AddEventListener(touchListener, this);


        }

        private void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count > 0)
            {
                CCScene newGameScene = new CCScene(GameView);
                var transitionToNewGame = new CCTransitionProgressInOut(0.7f, newGameScene);
                newGameScene.AddLayer(new GameLayer());
                Director.ReplaceScene(transitionToNewGame);
            }

        }
    }
}
