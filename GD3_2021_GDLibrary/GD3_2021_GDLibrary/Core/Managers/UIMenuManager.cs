using GDLibrary.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary.Managers
{
    /// <summary>
    /// Stores a dictionary of MENU-SPECIFIC ui scenes and updates and draws the currently active scene
    /// </summary>
    public abstract class UIMenuManager : UISceneManager
    {
        public UIMenuManager(Game game, SpriteBatch spriteBatch)
            : base(game, spriteBatch)
        {
        }

        protected override void HandleEvent(EventData eventData)
        {
            if (eventData.EventCategoryType == EventCategoryType.Menu)
            {
                if (eventData.EventActionType == EventActionType.OnPause)
                    statusType = StatusType.Drawn | StatusType.Updated;
                else if (eventData.EventActionType == EventActionType.OnPlay)
                    statusType = StatusType.Off;
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach (UIObject uiObject in activeUIScene.UiObjects)
            {
                var btnObject = uiObject as UIButtonObject;

                if (btnObject != null)
                {
                    if (Input.Mouse.Bounds.Intersects(btnObject.Bounds))
                    {
                        HandleMouseOver(btnObject);
                    }

                    if (Input.Mouse.WasJustClicked(Inputs.MouseButton.Left))
                    {
                        HandleMouseClicked(btnObject);
                    }
                }
            }
        }

        protected abstract void HandleMouseClicked(UIButtonObject btnObject);

        protected abstract void HandleMouseOver(UIButtonObject btnObject);
    }
}