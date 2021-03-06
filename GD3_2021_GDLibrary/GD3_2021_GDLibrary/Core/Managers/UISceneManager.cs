using GDLibrary.Components.UI;
using GDLibrary.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GDLibrary.Managers
{
    /// <summary>
    /// Stores a dictionary of ui scenes and updates and draws the currently active scene
    /// </summary>
    public class UISceneManager : PausableDrawableGameComponent
    {
        #region Fields

        protected SpriteBatch spriteBatch;
        protected Dictionary<string, UIScene> uiScenes;
        protected UIScene activeUIScene;
        protected string activeUISceneName;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Instanciate a ui scene manager to store all ui scenes (e.g. in-game ui)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="spriteBatch"></param>
        public UISceneManager(Game game, SpriteBatch spriteBatch) : base(game)
        {
            this.spriteBatch = spriteBatch;
            uiScenes = new Dictionary<string, UIScene>();
            activeUIScene = null;
            activeUISceneName = "";
        }

        #endregion Constructors

        #region Event Handling

        protected override void SubscribeToEvents()
        {
            EventDispatcher.Subscribe(EventCategoryType.UiObject, HandleEvent);
            base.SubscribeToEvents();
        }

        protected override void HandleEvent(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnAddObject:
                    Add(eventData.Parameters[0] as string,
                        eventData.Parameters[1] as UIObject);
                    break;

                case EventActionType.OnRemoveObject:
                    Remove(eventData.Parameters[0] as UIObject);
                    break;

                default:
                    break;
            }

            base.HandleEvent(eventData);
        }

        #endregion Event Handling

        #region Actions - Add, Remove, Find, Clear

        public bool SetActiveScene(string uiSceneName)
        {
            UIScene scene = Find(uiSceneName);
            if (scene != null)
            {
                activeUIScene = scene;
                activeUISceneName = uiSceneName;
                return true;
            }
            return false;
        }

        public void Add(UIScene uiScene)
        {
            if (!uiScenes.ContainsKey(uiScene.Name))
                uiScenes.Add(uiScene.Name, uiScene);
        }

        public void Add(string uiSceneName, UIObject uiObject)
        {
            var scene = Find(uiSceneName);
            scene?.Add(uiObject);
        }

        public bool Remove(string key)
        {
            return uiScenes.Remove(key);
        }

        public bool Remove(UIObject uiObject)
        {
            var scenesList = uiScenes.Keys;
            foreach (string uiSceneName in uiScenes.Keys)
            {
                var uiScene = uiScenes[uiSceneName];
                return uiScene.Remove(uiObject);
            }

            return false;
        }

        public UIScene Find(string key)
        {
            UIScene uiScene;
            uiScenes.TryGetValue(key, out uiScene);
            return uiScene;
        }

        public void Clear()
        {
            if (uiScenes.Count != 0)
                uiScenes.Clear();
        }

        #endregion Actions - Add, Remove, Find, Clear

        #region Update & Draw

        public override void Update(GameTime gameTime)
        {
            //is this component paused because of the menu?
            if (IsUpdated)
                activeUIScene?.Update();
        }

        public override void Draw(GameTime gameTime)
        {
            //is this component paused because of the menu?
            if (IsDrawn)
            {
                spriteBatch.Begin(SpriteSortMode.BackToFront,
                    BlendState.AlphaBlend, null, null);
                activeUIScene?.Draw(spriteBatch);
                spriteBatch.End();
            }
        }

        #endregion Update & Draw
    }
}