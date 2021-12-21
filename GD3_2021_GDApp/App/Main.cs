//#define DEMO

using GDLibrary;
using GDLibrary.Collections;
using GDLibrary.Components;
using GDLibrary.Components.UI;
using GDLibrary.Core;
using GDLibrary.Core.Demo;
using GDLibrary.Graphics;
using GDLibrary.Inputs;
using GDLibrary.Managers;
using GDLibrary.Parameters;
using GDLibrary.Renderers;
using GDLibrary.Utilities;
using JigLibX.Collision;
using JigLibX.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace GDApp
{
    public class Main : Game
    {
        Vector3 colliderScale = new Vector3(.03f, .03f, .03f);
        #region Fields

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        /// <summary>
        /// Stores and updates all scenes (which means all game objects i.e. players, cameras, pickups, behaviours, controllers)
        /// </summary>
        private SceneManager sceneManager;

        /// <summary>
        /// Draws all game objects with an attached and enabled renderer
        /// </summary>
        private RenderManager renderManager;

        /// <summary>
        /// Updates and Draws all ui objects
        /// </summary>
        private UISceneManager uiSceneManager;

        /// <summary>
        /// Updates and Draws all menu objects
        /// </summary>
        private MyMenuManager uiMenuManager;

        /// <summary>
        /// Plays all 2D and 3D sounds
        /// </summary>
        private SoundManager soundManager;

        private MyStateManager stateManager;
        private PickingManager pickingManager;

        /// <summary>
        /// Handles all system wide events between entities
        /// </summary>
        private EventDispatcher eventDispatcher;

        /// <summary>
        /// Applies physics to all game objects with a Collider
        /// </summary>
        private PhysicsManager physicsManager;

        /// <summary>
        /// Quick lookup for all textures used within the game
        /// </summary>
        private Dictionary<string, Texture2D> textureDictionary;

        /// <summary>
        /// Quick lookup for all fonts used within the game
        /// </summary>
        private ContentDictionary<SpriteFont> fontDictionary;

        /// <summary>
        /// Quick lookup for all models used within the game
        /// </summary>
        private ContentDictionary<Model> modelDictionary;

        /// <summary>
        /// Quick lookup for all videos used within the game by texture behaviours
        /// </summary>
        private ContentDictionary<Video> videoDictionary;

        //temps
        private Scene activeScene;

        private UITextObject nameTextObj;
        private Collider collider;

        #endregion Fields

        /// <summary>
        /// Construct the Game object
        /// </summary>
        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Set application data, input, title and scene manager
        /// </summary>
        private void InitializeEngine(string gameTitle, int width, int height)
        {
            //set game title
            Window.Title = gameTitle;

            //the most important element! add event dispatcher for system events
            eventDispatcher = new EventDispatcher(this);

            //add physics manager to enable CD/CR and physics
            physicsManager = new PhysicsManager(this);

            //instanciate scene manager to store all scenes
            sceneManager = new SceneManager(this);

            //create the ui scene manager to update and draw all ui scenes
            uiSceneManager = new UISceneManager(this, _spriteBatch);

            //create the ui menu manager to update and draw all menu scenes
            uiMenuManager = new MyMenuManager(this, _spriteBatch);

            //add support for playing sounds
            soundManager = new SoundManager(this);

            //this will check win/lose logic
            stateManager = new MyStateManager(this);

            //picking support using physics engine
            //this predicate lets us say ignore all the other collidable objects except interactables and consumables
            Predicate<GameObject> collisionPredicate =
                (collidableObject) =>
            {
                if (collidableObject != null)
                    return collidableObject.GameObjectType
                    == GameObjectType.Interactable
                    || collidableObject.GameObjectType == GameObjectType.Consumable;

                return false;
            };
            pickingManager = new PickingManager(this, 2, 100, collisionPredicate);

            //initialize global application data
            Application.Main = this;
            Application.Content = Content;
            Application.GraphicsDevice = _graphics.GraphicsDevice;
            Application.GraphicsDeviceManager = _graphics;
            Application.SceneManager = sceneManager;
            Application.PhysicsManager = physicsManager;
            Application.StateManager = stateManager;

            //instanciate render manager to render all drawn game objects using preferred renderer (e.g. forward, backward)
            renderManager = new RenderManager(this, new ForwardRenderer(), false, true);

            //instanciate screen (singleton) and set resolution etc
            Screen.GetInstance().Set(width, height, true, true);

            //instanciate input components and store reference in Input for global access
            Input.Keys = new KeyboardComponent(this);
            Input.Mouse = new MouseComponent(this);
            Input.Mouse.Position = Screen.Instance.ScreenCentre;
            Input.Gamepad = new GamepadComponent(this);

            //************* add all input components to component list so that they will be updated and/or drawn ***********/

            //add event dispatcher
            Components.Add(eventDispatcher);

            //add time support
            Components.Add(Time.GetInstance(this));

            //add input support
            Components.Add(Input.Keys);
            Components.Add(Input.Mouse);
            Components.Add(Input.Gamepad);

            //add physics manager to enable CD/CR and physics
            Components.Add(physicsManager);

            //add support for picking using physics engine
            Components.Add(pickingManager);

            //add scene manager to update game objects
            Components.Add(sceneManager);

            //add render manager to draw objects
            Components.Add(renderManager);

            //add ui scene manager to update and drawn ui objects
            Components.Add(uiSceneManager);

            //add ui menu manager to update and drawn menu objects
            Components.Add(uiMenuManager);

            //add sound
            Components.Add(soundManager);

            //add state
            Components.Add(stateManager);
        }

        /// <summary>
        /// Not much happens in here as SceneManager, UISceneManager, MenuManager and Inputs are all GameComponents that automatically Update()
        /// Normally we use this to add some temporary demo code in class - Don't forget to remove any temp code inside this method!
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            //if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.P))
            //{
            //    //DEMO - raise event
            //    //EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
            //    //    EventActionType.OnPause));

            //    object[] parameters = { nameTextObj };

            //    EventDispatcher.Raise(new EventData(EventCategoryType.UiObject,
            //        EventActionType.OnRemoveObject, parameters));

            //    ////renderManager.StatusType = StatusType.Off;
            //}
            //else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.U))
            //{
            //    //DEMO - raise event

            //    object[] parameters = { "main game ui", nameTextObj };

            //    EventDispatcher.Raise(new EventData(EventCategoryType.UiObject,
            //        EventActionType.OnAddObject, parameters));

            //    //renderManager.StatusType = StatusType.Drawn;
            //    //EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
            //    //  EventActionType.OnPlay));
            //}

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                object[] parameters = { "health", 1 };
                EventDispatcher.Raise(new EventData(EventCategoryType.UI,
                    EventActionType.OnHealthDelta, parameters));
            }
            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                object[] parameters = { "health", -1 };
                EventDispatcher.Raise(new EventData(EventCategoryType.UI,
                    EventActionType.OnHealthDelta, parameters));
            }


            var mainGameUIScene = new UIScene("main game ui");

            var hudTextureObj = new UITextureObject("HUD",
                 UIObjectType.Texture,
                 new Transform2D(new Vector2(0, 0),
                 new Vector2(1, 1),
                 MathHelper.ToRadians(0)),
                 0, Content.Load<Texture2D>("Assets/Textures/UI/Progress/hud"));
            //add the ui element to the scene
            hudTextureObj.Color = Color.White;
            mainGameUIScene.Add(hudTextureObj);


            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                object[] parameters0 = { "main menu video" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Video,
                     EventActionType.OnPlay, parameters0));

                EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
                          EventActionType.OnPause));

                object[] parameters = { "MineWhispers" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));

                object[] parameters1 = { "Breathing2" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters1));

                object[] parameters2 = { "Heartbeat" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters2));

                object[] parameters3 = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters3));

            }
            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.O))
            {
                EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
                    EventActionType.OnPlay));
            }


            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.F1))
            {
                object[] parameters = { "PlayerForeman line1" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }
            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.F2))
            {
                object[] parameters = { "PlayerForeman line2" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.F3))
            {
                object[] parameters = { "Foreman line3" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.F4))
            {
                object[] parameters = { "Foreman line4" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.F5))
            {
                object[] parameters = { "Foreman line5" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.F6))
            {
                object[] parameters = { "Foreman line6" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.F7))
            {
                object[] parameters = { "Foreman lineplayerdefeat" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.F8))
            {
                object[] parameters = { "Foreman lineplayervictory" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Y))
            {
                object[] parameters = { "Player Dialogue 3 Edited" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.U))
            {
                object[] parameters = { "Player Dialogue 4 Edited" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.I))
            {
                object[] parameters = { "Player Dialogue 5 Edited" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.O))
            {
                object[] parameters = { "Player Dialogue 6 Edited" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.P))
            {
                object[] parameters = { "Player Dialogue 7 Edited" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.K))
            {
                object[] parameters = { "Player Dialogue 8 Edited" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.V))
            {
                object[] parameters = { "main menu video" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Video,
                    EventActionType.OnPlay, parameters));
            }

            if (Input.Keys.WasJustReleased(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                object[] parameters = { "Jump" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustReleased(Microsoft.Xna.Framework.Input.Keys.W))
            {
                object[] parameters = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustReleased(Microsoft.Xna.Framework.Input.Keys.S))
            {
                object[] parameters = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustReleased(Microsoft.Xna.Framework.Input.Keys.D))
            {
                object[] parameters = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustReleased(Microsoft.Xna.Framework.Input.Keys.A))
            {
                object[] parameters = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustReleased(Microsoft.Xna.Framework.Input.Keys.F))
            {
                object[] parameters = { "FlashlightFlickOn" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                object[] parameters = { "Jump" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.W))
            {
                object[] parameters = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.A))
            {
                object[] parameters = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.S))
            {
                object[] parameters = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.D))
            {
                object[] parameters = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));

                if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.C))
                    Application.SceneManager.ActiveScene.CycleCameras();


            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Not much happens in here as RenderManager, UISceneManager and MenuManager are all DrawableGameComponents that automatically Draw()
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }

        /******************************** Student Project-specific ********************************/
        /******************************** Student Project-specific ********************************/
        /******************************** Student Project-specific ********************************/

        #region Student/Group Specific Code

        /// <summary>
        /// Initialize engine, dictionaries, assets, level contents
        /// </summary>
        protected override void Initialize()
        {
            //move here so that UISceneManager can use!
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //data, input, scene manager
            InitializeEngine(AppData.GAME_TITLE_NAME,
                AppData.GAME_RESOLUTION_WIDTH,
                AppData.GAME_RESOLUTION_HEIGHT);

            //load structures that store assets (e.g. textures, sounds) or archetypes (e.g. Quad game object)
            InitializeDictionaries();

            //load assets into the relevant dictionary
            LoadAssets();

            //level with scenes and game objects
            InitializeLevel();

            //add menu and ui
            InitializeUI();

            //TODO - remove hardcoded mouse values - update Screen class to centre the mouse with hardcoded value - remove later
            Input.Mouse.Position = Screen.Instance.ScreenCentre;

            //turn on/off debug info
            //InitializeDebugUI(true, true);

            //to show the menu we must start paused for everything else!
            EventDispatcher.Raise(new EventData(EventCategoryType.Menu, EventActionType.OnPause));

            base.Initialize();
        }

        /******************************* Load/Unload Assets *******************************/

        private void InitializeDictionaries()
        {
            textureDictionary = new Dictionary<string, Texture2D>();

            //why not try the new and improved ContentDictionary instead of a basic Dictionary?
            fontDictionary = new ContentDictionary<SpriteFont>();
            modelDictionary = new ContentDictionary<Model>();

            videoDictionary = new ContentDictionary<Video>();
        }

        private void LoadAssets()
        {
            LoadModels();
            LoadTextures();
            LoadVideos();
            LoadSounds();
            LoadFonts();
        }
        private void LoadVideos()
        {
            videoDictionary.Add("Assets/Video/main_menu_video");
        }

        /// <summary>
        /// Load models to dictionary
        /// </summary>
        private void LoadModels()
        {
            //notice with the ContentDictionary we dont have to worry about Load() or a name (its assigned from pathname)
            modelDictionary.Add("Assets/Models/sphere");
            modelDictionary.Add("Assets/Models/cube");
            modelDictionary.Add("Assets/Models/teapot");
            modelDictionary.Add("Assets/Models/monkey1");
            modelDictionary.Add("Assets/Models/tunnel");
            modelDictionary.Add("Assets/Models/tunnel_curve");
            modelDictionary.Add("Assets/Models/hub_improved");
            modelDictionary.Add("Assets/Models/gear");
            //modelDictionary.Add("Assets/Models/barrel");
        }

        /// <summary>
        /// Load fonts to dictionary
        /// </summary>
        private void LoadFonts()
        {
            fontDictionary.Add("Assets/Fonts/ui");
            fontDictionary.Add("Assets/Fonts/menu");
            fontDictionary.Add("Assets/Fonts/debug");
        }

        /// <summary>
        /// Load sound data used by sound manager
        /// </summary>
        private void LoadSounds()
        {
            var soundEffect0 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Jump");

            var soundEffect =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/MineWhispers");

            var soundEffect1 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Steps");

            var soundEffect2 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Breathing2");

            var soundEffect3 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Droplet");

            var soundEffect4 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Droplet2");

            var soundEffect5 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/ElevatorOpening");

            var soundEffect7 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/FlashlightFlickOn");

            var soundEffect8 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/GearPickup");

            var soundEffect9 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Heartbeat");

            var soundEffect10 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Heartbeat2");

            var soundEffect11 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/InventoryMap-Menu");

            var soundEffect12 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/MenuClick");

            var soundEffect13 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Motors");

            var soundEffect14 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Rock1");

            var soundEffect15 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/RockCrashingV3");

            var soundEffect16 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Swing");

            var dialoguePlayer =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Player Dialogue 1 Edited");

            var dialoguePlayer1 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Player Dialogue 2 Edited");

            var dialoguePlayer2 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Player Dialogue 3 Edited");

            var dialoguePlayer3 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Player Dialogue 4 Edited");

            var dialoguePlayer4 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Player Dialogue 5 Edited");

            var dialoguePlayer5 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Player Dialogue 6 Edited");

            var dialoguePlayer6 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Player Dialogue 7 Edited");

            var dialoguePlayer7 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Player Dialogue 8 Edited");

            var dialogueForeman =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Foreman line1");

            var dialogueForeman1 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Foreman line2");

            var dialogueForeman2 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Foreman line3");

            var dialogueForeman3 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Foreman line4");

            var dialogueForeman4 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Foreman line5");

            var dialogueForeman5 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Foreman line6");

            var dialogueForeman6 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Foreman lineplayerdefeat");

            var dialogueForeman7 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Foreman lineplayervictory");

            var dialoguePlayerForeman =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/PlayerForeman line1");

            var dialoguePlayerForeman1 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/PlayerForeman line2");


            //add the new sound effect
            soundManager.Add(new GDLibrary.Managers.Cue(
                "Jump",
                soundEffect0,
                SoundCategoryType.BackgroundMusic,
                new Vector3(1, 0, 0),
                true));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "MineWhispers",
                soundEffect,
                SoundCategoryType.BackgroundMusic,
                new Vector3(1, 0, 0),
                true));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Steps",
                soundEffect1,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                true));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Breathing2",
                soundEffect2,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                true));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Droplet",
                soundEffect3,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Droplet2",
                soundEffect4,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "ELevatorOpening",
                soundEffect5,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "FlashlightFlickOn",
                soundEffect7,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "GearPickup",
                soundEffect8,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Heartbeat",
                soundEffect9,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                true));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Heartbeat2",
                soundEffect10,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "InventoryMap-Menu",
                soundEffect11,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "MenuClick",
                soundEffect12,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Motors",
                soundEffect13,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Rock1",
                soundEffect14,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "RockCrashingV3",
                soundEffect15,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Swing",
                soundEffect16,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Foreman line1",
                dialogueForeman,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Foreman line2",
                dialogueForeman1,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Foreman line3",
                dialogueForeman2,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Foreman line4",
                dialogueForeman3,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Foreman line5",
                dialogueForeman4,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Foreman line6",
                dialogueForeman5,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Foreman lineplayerdefeat",
                dialogueForeman6,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Foreman lineplayervictory",
                dialogueForeman7,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Player Dialogue 1 Edited",
                dialoguePlayer,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Player Dialogue 2 Edited",
                dialoguePlayer1,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Player Dialogue 3 Edited",
                dialoguePlayer2,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Player Dialogue 4 Edited",
                dialoguePlayer3,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Player Dialogue 5 Edited",
                dialoguePlayer4,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Player Dialogue 6 Edited",
                dialoguePlayer5,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Player Dialogue 7 Edited",
                dialoguePlayer6,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Player Dialogue 8 Edited",
                dialoguePlayer7,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "PlayerForeman line1",
                dialoguePlayerForeman,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "PlayerForeman line2",
                dialoguePlayerForeman1,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));
        }

        /// <summary>
        /// Load texture data from file and add to the dictionary
        /// </summary>
        private void LoadTextures()
        {
            //debug
            textureDictionary.Add("checkerboard", Content.Load<Texture2D>("Assets/Demo/Textures/checkerboard"));
            textureDictionary.Add("mona lisa", Content.Load<Texture2D>("Assets/Demo/Textures/mona lisa"));

            //skybox
            textureDictionary.Add("skybox_front", Content.Load<Texture2D>("Assets/Textures/Skybox/front"));
            textureDictionary.Add("skybox_left", Content.Load<Texture2D>("Assets/Textures/Skybox/left"));
            textureDictionary.Add("skybox_right", Content.Load<Texture2D>("Assets/Textures/Skybox/right"));
            textureDictionary.Add("skybox_back", Content.Load<Texture2D>("Assets/Textures/Skybox/back"));
            textureDictionary.Add("skybox_sky", Content.Load<Texture2D>("Assets/Textures/Skybox/sky"));

            //environment
            textureDictionary.Add("grass", Content.Load<Texture2D>("Assets/Textures/Foliage/Ground/grass1"));
            textureDictionary.Add("crate1", Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate1"));

            textureDictionary.Add("rock", Content.Load<Texture2D>("Assets/Textures/Cave/Rock"));
            textureDictionary.Add("wood", Content.Load<Texture2D>("Assets/Textures/wood/Wood"));
            textureDictionary.Add("oldmetal", Content.Load<Texture2D>("Assets/Textures/paint_and_metal"));



            //ui
            textureDictionary.Add("ui_progress_32_8", Content.Load<Texture2D>("Assets/Textures/UI/Controls/ui_progress_32_8"));
            textureDictionary.Add("progress_white", Content.Load<Texture2D>("Assets/Textures/UI/Controls/progress_white"));
            textureDictionary.Add("HP_Bar_V2", Content.Load<Texture2D>("Assets/Textures/UI/Progress/HP_Bar_V2"));

            //menu
            textureDictionary.Add("mainmenu", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/mainmenu"));
            textureDictionary.Add("audiomenu", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/audiomenu"));
            textureDictionary.Add("controlsmenu", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/controlsmenu"));
            textureDictionary.Add("exitmenuwithtrans", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/exitmenuwithtrans"));
            textureDictionary.Add("genericbtn", Content.Load<Texture2D>("Assets/Textures/UI/Controls/genericbtn"));

            //reticule
            textureDictionary.Add("reticuleOpen",
      Content.Load<Texture2D>("Assets/Textures/UI/Controls/reticuleOpen"));
            textureDictionary.Add("reticuleDefault",
          Content.Load<Texture2D>("Assets/Textures/UI/Controls/reticuleDefault"));
        }

        /// <summary>
        /// Free all asset resources, dictionaries, network connections etc
        /// </summary>
        protected override void UnloadContent()
        {
            //TODO - add graceful dispose for content

            //remove all models used for the game and free RAM
            modelDictionary?.Dispose();
            fontDictionary?.Dispose();
            videoDictionary?.Dispose();

            base.UnloadContent();
        }

        /******************************* UI & Menu *******************************/

        /// <summary>
        /// Create a scene, add content, add to the scene manager, and load default scene
        /// </summary>
        private void InitializeLevel()
        {
            float worldScale = 1000;
            activeScene = new Scene("level 1");

            InitializeCameras(activeScene);

            InitializeSkybox(activeScene, worldScale);

            //remove because now we are interested only in collidable things!
            //InitializeCubes(activeScene);
            //InitializeModels(activeScene);

            //InitializeSkybox(activeScene, 1000);
            //InitializeCubes(activeScene);
            //InitializeModels(activeScene);

            #region Props

            InitializeElevator(activeScene);
            
            InitializeSpeaker(activeScene);
            InitializeLight(activeScene);


            #endregion
            #region Level
            ////Tunnel 1 - 10
            InitializeTunnel(activeScene, new Vector3(10f, 0.28f, 54.7f), new Vector3(-90f, 90f, 0f), new Vector3(1f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(26f, 0.28f, 54.7f), new Vector3(-90f, 90f, 0f), new Vector3(1f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(42f, 0.28f, 54.7f), new Vector3(-90f, 90f, 0f), new Vector3(1f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(58f, 0.28f, 54.7f), new Vector3(-90f, 90f, 0f), new Vector3(1f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(74f, 0.28f, 54.7f), new Vector3(-90f, 90f, 0f), new Vector3(1f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(72f, 0.28f, -2f), new Vector3(-90f, 90f, 0f), new Vector3(1.2f, 1.2f, 1.4f));
            InitializeTunnel(activeScene, new Vector3(106.5f, 0.28f, 34f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(-13f, 0.28f, -77f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(-13f, 0.28f, -93f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(5.4f, 0.28f, -125f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));

            //Tunnel 11 - 20    16 dodge
            InitializeTunnel(activeScene, new Vector3(-13f, 0.28f, -125f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(-13f, 0.28f, -141f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(5.4f, 0.28f, -77f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(5.4f, 0.28f, -93f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(5.4f, 0.28f, -109f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(-13f, 0.28f, -109f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(5.4f, 0.28f, -141f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(5.4f, 0.28f, -157f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(-82f, 0.28f, -240.5f), new Vector3(-90f, 90f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(5.4f, 0.28f, -157f), new Vector3(-90f, 180f, 0f), new Vector3(1.2f, 1f, 1.3f));

            //Tunnel 21 - 24
            InitializeTunnel(activeScene, new Vector3(-44f, 0.28f, -5.6f), new Vector3(-90f, 90f, 0f), new Vector3(1.2f, 1f, 1.3f));
            //^^ Bit dodgy
            InitializeTunnel(activeScene, new Vector3(-104f, 0.28f, 46f), new Vector3(-90f, 90f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(-159f, 0.28f, 97.7f), new Vector3(-90f, 90f, 0f), new Vector3(1.2f, 1f, 1.3f));
            InitializeTunnel(activeScene, new Vector3(-175f, 0.28f, 97.7f), new Vector3(-90f, 90f, 0f), new Vector3(1.2f, 1f, 1.3f));

            InitializeHub(activeScene);


            //Corner tunnels
            InitializeTurn(activeScene, new Vector3(-90f, 180f, 0f), new Vector3(-21.5f, 0.25f, 50f), new Vector3(1.3f, 1f, 1.30f), new Vector3(15f, -0.29f, -45f));
            InitializeTurn(activeScene, new Vector3(-90f, 270f, 0f), new Vector3(102f, 0.25f, 61.5f), new Vector3(1.3f, 1f, 1.30f), new Vector3(-45f, -0.27f, -13f));
            InitializeTurn(activeScene, new Vector3(-90f, 0f, 0f), new Vector3(114f, 0.25f, 10f), new Vector3(1.3f, 1f, 1.30f), new Vector3(-15f, -0.27f, 47f));
            InitializeTurn(activeScene, new Vector3(-90f, 0f, 0f), new Vector3(13f, 0.25f, -185f), new Vector3(1.3f, 1f, 1.30f), new Vector3(-15f, -0.27f, 47f));
            InitializeTurn(activeScene, new Vector3(-90f, 180f, 0f), new Vector3(-46f, 0.25f, -201f), new Vector3(1.3f, 1f, 1.30f), new Vector3(15f, -0.27f, -47f));
            InitializeTurn(activeScene, new Vector3(-90f, 0f, 0f), new Vector3(-38.3f, 0.25f, -229f), new Vector3(1.3f, 1f, 1.30f), new Vector3(-15f, -0.27f, 47f));
            InitializeTurn(activeScene, new Vector3(-90f, 0f, 0f), new Vector3(-5.2f, 0.25f, -168f), new Vector3(1.3f, 1f, 1.30f), new Vector3(-15f, -0.27f, 47f));
            InitializeTurn(activeScene, new Vector3(-90f, 0f, 0f), new Vector3(-48.5f, 0.25f, -180f), new Vector3(1.3f, 1f, 1.30f), new Vector3(-15f, -0.27f, 47f));
            InitializeTurn(activeScene, new Vector3(-90f, 0f, 0f), new Vector3(-53f, 0.25f, -138f), new Vector3(1.3f, 1f, 1.30f), new Vector3(-15f, -0.27f, 47f));
            InitializeTurn(activeScene, new Vector3(-90f, 0f, 0f), new Vector3(-102f, 0.25f, -198f), new Vector3(1.3f, 1f, 1.30f), new Vector3(-15f, -0.27f, 47f));
            InitializeTurn(activeScene, new Vector3(-90f, 90f, 0f), new Vector3(-98f, 0.25f, -240.5f), new Vector3(1.3f, 1f, 1.30f), new Vector3(47f, -0.27f, 15f));
            InitializeTurn(activeScene, new Vector3(-90f, 90f, 0f), new Vector3(-60f, 0.25f, -5.7f), new Vector3(1.3f, 1f, 1.30f), new Vector3(47f, -0.27f, 15f));
            InitializeTurn(activeScene, new Vector3(-90f, 270f, 0f), new Vector3(-76f, 0.25f, 53.5f), new Vector3(1.3f, 1f, 1.30f), new Vector3(-47f, -0.27f, -15f));
            InitializeTurn(activeScene, new Vector3(-90f, 90f, 0f), new Vector3(-115f, 0.25f, 46f), new Vector3(1.3f, 1f, 1.30f), new Vector3(47f, -0.27f, 15f));
            InitializeTurn(activeScene, new Vector3(-90f, 270f, 0f), new Vector3(-131f, 0.25f, 105f), new Vector3(1.3f, 1f, 1.30f), new Vector3(-47f, -0.27f, -15f));
            InitializeTurn(activeScene, new Vector3(-90f, 90f, 0f), new Vector3(-175f, 0.25f, 97.7f), new Vector3(1.3f, 1f, 1.30f), new Vector3(45f, -0.27f, 15f));
            InitializeTurn(activeScene, new Vector3(-90f, 90f, 0f), new Vector3(-181f, 0.25f, 127.7f), new Vector3(1.3f, 1f, 1.30f), new Vector3(45f, -0.27f, 15f));



            InitializeRail(activeScene);
            InitializeRail1(activeScene);
            InitializeRail2(activeScene);
            InitializeRail3(activeScene);
            InitializeRail4(activeScene);
            InitializeRail5(activeScene);
            InitializeRail6(activeScene);
            InitializeRail7(activeScene);
            InitializeRail8(activeScene);
            InitializeRail9(activeScene);
            InitializeRail10(activeScene);
            InitializeCart(activeScene);

            InitializeHelmet(activeScene);

            InitializeStructs(activeScene);
            InitializeStructs1(activeScene);
            InitializeStructs2(activeScene);
            InitializeStructs3(activeScene);
            InitializeStructs4(activeScene);
            InitializeStructs5(activeScene);
            InitializeStructs6(activeScene);
            InitializeStructs7(activeScene);
            InitializeStructs8(activeScene);
            InitializeStructs9(activeScene);
            InitializeStructs10(activeScene);
            InitializeStructs11(activeScene);
            InitializeStructs12(activeScene);
            InitializeStructs13(activeScene);
            InitializeStructs14(activeScene);
            InitializeStructs15(activeScene);
            InitializeStructs16(activeScene);
            InitializeStructs17(activeScene);
            InitializeStructs18(activeScene);
            InitializeStructs19(activeScene);
            InitializeStructs20(activeScene);
            InitializeStructs21(activeScene);
            InitializeStructs22(activeScene);
            InitializeStructs23(activeScene);
            InitializeStructs24(activeScene);
            InitializeStructs25(activeScene);
            InitializeStructs26(activeScene);
            InitializeStructs27(activeScene);
            InitializeStructs28(activeScene);
            InitializeStructs29(activeScene);
            InitializeStructs30(activeScene);
            InitializeStructs31(activeScene);
            InitializeStructs32(activeScene);
            InitializeStructs33(activeScene);
            InitializeStructs34(activeScene);
            InitializeStructs35(activeScene);
            InitializeStructs36(activeScene);
            InitializeStructs37(activeScene);
            InitializeStructs38(activeScene);
            InitializeStructs39(activeScene);
            InitializeStructs40(activeScene);
            InitializeStructs41(activeScene);
            InitializeStructs42(activeScene);
            InitializeStructs43(activeScene);
            InitializeStructs44(activeScene);
            InitializeStructs45(activeScene);
            InitializeStructs46(activeScene);
            InitializeStructs47(activeScene);
            InitializeStructs48(activeScene);
            InitializeStructs49(activeScene);
            InitializeStructs50(activeScene);
            InitializeStructs51(activeScene);
            InitializeStructs52(activeScene);
            InitializeStructs53(activeScene);
            InitializeStructs54(activeScene);
            InitializeStructs55(activeScene);
            InitializeStructs56(activeScene);
            InitializeStructs57(activeScene);
            InitializeStructs58(activeScene);
            InitializeStructs59(activeScene);
            #endregion
            InitializeCollidables(activeScene, worldScale);
            sceneManager.Add(activeScene);
            sceneManager.LoadScene("level 1");
        }

        private void InitializeTunnel(Scene level, Vector3 tunnelTransform, Vector3 tunnelRotate, Vector3 tunnelScale)
        {
            //tunnel
            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);
            //re-use the vertices and indices of the model
            var mesh = new QuadMesh();

            var archetypalTunnel = new GameObject("tunnel ground", GameObjectType.Architecture, true);

            archetypalTunnel.Transform.Rotate(tunnelRotate);
            archetypalTunnel.Transform.Translate(tunnelTransform);
            archetypalTunnel.Transform.Scale(tunnelScale);

            archetypalTunnel.AddComponent(new ModelRenderer(modelDictionary["tunnel"], new BasicMaterial
                ("rock", shader, Color.White, 1, textureDictionary["rock"])));

            Vector3 collisionRotate = new Vector3();
            if (tunnelRotate == new Vector3(-90f, 90f, 0f))
            {
                collisionRotate = new Vector3(0f, 90f, 0f);
            }
            else if (tunnelRotate == new Vector3(-90f, 180f, 0f))
            {
                collisionRotate = new Vector3(0f, 180f, 0f);
            }


            //add Collision Surface(s)
            collider = new Collider();
            archetypalTunnel.AddComponent(collider);
            collider.AddPrimitive(CollisionUtility.GetTriangleMesh(modelDictionary["tunnel"],
                new Vector3(0, -0.3f, 0f), collisionRotate, colliderScale),
                new MaterialProperties(.8f, .8f, .7f));
            collider.Enable(true, 1);


            level.Add(archetypalTunnel);
        }

        private void InitializeTurn(Scene level, Vector3 tunnelTurnRotate, Vector3 tunnelTurnTransform, Vector3 tunnelTurnScale, Vector3 collisionTransform)
        {
            //tunnel
            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);
            //re-use the vertices and indices of the model
            var mesh = new QuadMesh();

            var archetypalTunnelTurn = new GameObject("tunnel ground", GameObjectType.Architecture, true);

            archetypalTunnelTurn.Transform.Rotate(tunnelTurnRotate);
            archetypalTunnelTurn.Transform.Translate(tunnelTurnTransform);
            archetypalTunnelTurn.Transform.Scale(tunnelTurnScale);

            archetypalTunnelTurn.AddComponent(new ModelRenderer(modelDictionary["tunnel_curve"], new BasicMaterial
                ("rock", shader, Color.White, 1, textureDictionary["rock"])));

            Vector3 collisionRotate = new Vector3(0f, 0f, 0f);
            if (tunnelTurnRotate == new Vector3(-90f, 90f, 0f))
            {
                collisionRotate = new Vector3(0f, 0f, 0f);
            }
            else if (tunnelTurnRotate == new Vector3(-90f, 180f, 0f))
            {
                //collisionRotate = new Vector3(0f, 180f, 0f);
                collisionRotate = new Vector3(0, 90f, 0f);
            }
            else if (tunnelTurnRotate == new Vector3(-90f, 270f, 0f))
            {
                collisionRotate = new Vector3(0f, 180f, 0f);
            }
            else if (tunnelTurnRotate == new Vector3(-90f, 0f, 0f))
            {
                collisionRotate = new Vector3(0f, -90f, 0f);
            }

            //add Collision Surface(s)
            collider = new Collider();
            archetypalTunnelTurn.AddComponent(collider);
            collider.AddPrimitive(CollisionUtility.GetTriangleMesh(modelDictionary["tunnel_curve"],
                collisionTransform, collisionRotate, new Vector3(.0275f, .037f, .035f)),
                new MaterialProperties(.8f, .8f, .7f));
            collider.Enable(true, 1);


            level.Add(archetypalTunnelTurn);
        }


        private void InitializeHub(Scene level)
        {
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/Cave/Rock"));

            var archetypalCave = new GameObject("cave", GameObjectType.Architecture);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/hub_improved"), material);
            renderer.Material = material;
            archetypalCave.AddComponent(renderer);

            archetypalCave.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/hub_improved");

            archetypalCave.Transform.Rotate(-90f, 90f, 0f);
            archetypalCave.Transform.Translate(1.2f, 0.1f, 1.3f);


            //add Collision Surface(s)
            //collider = new Collider();
            //archetypalCave.AddComponent(collider);
            //collider.AddPrimitive(CollisionUtility.GetTriangleMesh(modelDictionary["hub_improved"],
            //    new Vector3(0f, -1f, 45f), new Vector3(0f, 90f, 0f), new Vector3(0.0115f, 0.02f, 0.013f)),
            //    new MaterialProperties(.8f, .8f, .7f));
            //collider.Enable(true, 1);


            level.Add(archetypalCave);
        }


        #region Props
        private void InitializeSpeaker(Scene level)
        {
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/RustedSteel"));

            //tunnel_turn
            var archetypalSpeaker = new GameObject("speaker", GameObjectType.Architecture);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/speaker"), material);
            renderer.Material = material;
            archetypalSpeaker.AddComponent(renderer);

            archetypalSpeaker.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/speaker");

            archetypalSpeaker.Transform.Rotate(-90f, 0f, 0f);
            archetypalSpeaker.Transform.Translate(2f, 0.5f, -10.8f);
            archetypalSpeaker.Transform.SetScale(10f, 10f, 10f);

            level.Add(archetypalSpeaker);
        }

        private void InitializeLight(Scene level)
        {
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/yellow"));

            //tunnel_turn
            var archetypalLight = new GameObject("light", GameObjectType.Architecture);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/light"), material);
            renderer.Material = material;
            archetypalLight.AddComponent(renderer);

            archetypalLight.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/light");

            archetypalLight.Transform.Rotate(180f, 10f, 0f);
            archetypalLight.Transform.Translate(10f, 0.2f, -25f);
            archetypalLight.Transform.SetScale(0.8f, 0.8f, 0.8f);

            level.Add(archetypalLight);
        }

        #endregion

        

        #region Elements
       

        
private void InitializeElevator(Scene level)
        {
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/Metal"));

            var archetypalElevator = new GameObject("elevator", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/elevator"), material);
            renderer.Material = material;


            archetypalElevator.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/elevator");

            archetypalElevator.Transform.Rotate(270f, 270, 0f);
            archetypalElevator.Transform.Translate(51f, 11f, -16f);
            archetypalElevator.Transform.SetScale(1.3f, 1.5f, 1.3f);
            level.Add(archetypalElevator);

            InitializeElevatorG(activeScene);
            InitializeElevatorW(activeScene);
        }

        private void InitializeElevatorW(Scene level)
        {
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/Metal"));

            var archetypalElevatorW = new GameObject("elevator", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/elevatorW"), material);
            renderer.Material = material;


            archetypalElevatorW.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/elevatorW");

            archetypalElevatorW.Transform.Rotate(270f, 270, 0f);
            archetypalElevatorW.Transform.Translate(51f, 11f, -16f);
            archetypalElevatorW.Transform.SetScale(1.3f, 1.5f, 1.3f);
            level.Add(archetypalElevatorW);
        }

        private void InitializeElevatorG(Scene level)
        {
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/Red"));

            var archetypalElevatorG = new GameObject("elevator", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/elevatorG"), material);
            renderer.Material = material;


            archetypalElevatorG.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/elevatorG");

            archetypalElevatorG.Transform.Rotate(270f, 90, 0f);
            archetypalElevatorG.Transform.Translate(44f, 7.5f, -16f);
            archetypalElevatorG.Transform.SetScale(6f, 0.5f, 5f);
            level.Add(archetypalElevatorG);
        }

        #region Rails and Minecart
        private void InitializeRail(Scene level)
        {
            //tunnel
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/OldMetal"));

            var archetypalRail = new GameObject("rail", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/rail"), material);
            renderer.Material = material;
            archetypalRail.AddComponent(renderer);

            archetypalRail.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/rail");

            archetypalRail.Transform.Rotate(-90f, 90f, 0f);
            archetypalRail.Transform.Translate(2f, 0.5f, -10.8f);
            archetypalRail.Transform.SetScale(0.5f, 0.8f, 0.3f);

            renderer.Material = material;

            level.Add(archetypalRail);
        }

        private void InitializeRail1(Scene level)
        {
            //tunnel
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/OldMetal"));

            var archetypalRail1 = new GameObject("rail", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/rail"), material);
            renderer.Material = material;
            archetypalRail1.AddComponent(renderer);

            archetypalRail1.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/rail");

            archetypalRail1.Transform.Rotate(-90f, 90f, 0f);
            archetypalRail1.Transform.Translate(2f, 0.5f, -26f);
            archetypalRail1.Transform.SetScale(0.5f, 0.8f, 0.3f);

            renderer.Material = material;

            level.Add(archetypalRail1);
        }

        private void InitializeRail2(Scene level)
        {
            //tunnel
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/OldMetal"));

            var archetypalRail2 = new GameObject("rail", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/rail"), material);
            renderer.Material = material;
            archetypalRail2.AddComponent(renderer);

            archetypalRail2.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/rail");

            archetypalRail2.Transform.Rotate(-90f, 115f, 0f);
            archetypalRail2.Transform.Translate(1.3f, 0.5f, -42.3f);
            archetypalRail2.Transform.SetScale(0.5f, 0.8f, 0.3f);

            renderer.Material = material;

            level.Add(archetypalRail2);
        }

        private void InitializeRail3(Scene level)
        {
            //tunnel
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/OldMetal"));

            var archetypalRail3 = new GameObject("rail", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/rail"), material);
            renderer.Material = material;
            archetypalRail3.AddComponent(renderer);

            archetypalRail3.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/rail");

            archetypalRail3.Transform.Rotate(-90f, 95f, 0f);
            archetypalRail3.Transform.Translate(-7.7f, 0.5f, -72.2f);
            archetypalRail3.Transform.SetScale(0.5f, 0.8f, 0.3f);

            renderer.Material = material;

            level.Add(archetypalRail3);
        }

        private void InitializeRail4(Scene level)
        {
            //tunnel
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/OldMetal"));

            var archetypalRail4 = new GameObject("rail", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/rail"), material);
            renderer.Material = material;
            archetypalRail4.AddComponent(renderer);

            archetypalRail4.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/rail");

            archetypalRail4.Transform.Rotate(-90f, 100f, 0f);
            archetypalRail4.Transform.Translate(-5.1f, 0.5f, -56.8f);
            archetypalRail4.Transform.SetScale(0.5f, 0.8f, 0.3f);

            renderer.Material = material;

            level.Add(archetypalRail4);
        }

        private void InitializeRail5(Scene level)
        {
            //tunnel
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/OldMetal"));

            var archetypalRail5 = new GameObject("rail", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/rail"), material);
            renderer.Material = material;
            archetypalRail5.AddComponent(renderer);

            archetypalRail5.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/rail");

            archetypalRail5.Transform.Rotate(-90f, 90f, 0f);
            archetypalRail5.Transform.Translate(-8.9f, 0.5f, -87.8f);
            archetypalRail5.Transform.SetScale(0.5f, 0.8f, 0.3f);

            renderer.Material = material;

            level.Add(archetypalRail5);
        }

        private void InitializeRail6(Scene level)
        {
            //tunnel
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/OldMetal"));

            var archetypalRail6 = new GameObject("rail", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/rail"), material);
            renderer.Material = material;
            archetypalRail6.AddComponent(renderer);

            archetypalRail6.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/rail");

            archetypalRail6.Transform.Rotate(-90f, 90f, 0f);
            archetypalRail6.Transform.Translate(-8.9f, 0.5f, -103f);
            archetypalRail6.Transform.SetScale(0.5f, 0.8f, 0.3f);

            renderer.Material = material;

            level.Add(archetypalRail6);
        }

        private void InitializeRail7(Scene level)
        {
            //tunnel
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/OldMetal"));

            var archetypalRail7 = new GameObject("rail", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/rail"), material);
            renderer.Material = material;
            archetypalRail7.AddComponent(renderer);

            archetypalRail7.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/rail");

            archetypalRail7.Transform.Rotate(-90f, 90f, 0f);
            archetypalRail7.Transform.Translate(-8.9f, 0.5f, -140);
            archetypalRail7.Transform.SetScale(0.5f, 0.8f, 0.3f);

            renderer.Material = material;

            level.Add(archetypalRail7);
        }

        private void InitializeRail8(Scene level)
        {
            //tunnel
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/OldMetal"));

            var archetypalRail8 = new GameObject("rail", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/rail"), material);
            renderer.Material = material;
            archetypalRail8.AddComponent(renderer);

            archetypalRail8.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/rail");

            archetypalRail8.Transform.Rotate(-90f, 90f, 0f);
            archetypalRail8.Transform.Translate(-8.9f, 0.5f, -116f);
            archetypalRail8.Transform.SetScale(0.5f, 0.8f, 0.3f);

            renderer.Material = material;

            level.Add(archetypalRail8);
        }

        private void InitializeRail9(Scene level)
        {
            //tunnel
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/OldMetal"));

            var archetypalRail9 = new GameObject("rail", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/rail"), material);
            renderer.Material = material;
            archetypalRail9.AddComponent(renderer);

            archetypalRail9.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/rail");

            archetypalRail9.Transform.Rotate(-90f, 90f, 0f);
            archetypalRail9.Transform.Translate(-8.9f, 0.5f, -130f);
            archetypalRail9.Transform.SetScale(0.5f, 0.8f, 0.3f);

            renderer.Material = material;

            level.Add(archetypalRail9);
        }

        private void InitializeRail10(Scene level)
        {
            //tunnel
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/OldMetal"));

            var archetypalRail10 = new GameObject("rail", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/rail"), material);
            renderer.Material = material;
            archetypalRail10.AddComponent(renderer);

            archetypalRail10.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/rail");

            archetypalRail10.Transform.Rotate(-90f, 90f, 0f);
            archetypalRail10.Transform.Translate(-8.9f, 0.5f, -155f);
            archetypalRail10.Transform.SetScale(0.5f, 0.8f, 0.3f);

            renderer.Material = material;

            level.Add(archetypalRail10);
        }

        private void InitializeCart(Scene level)
        {
            //tunnel
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/BlackMetal"));

            var archetypalCart = new GameObject("rail", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/cart"), material);
            renderer.Material = material;
            archetypalCart.AddComponent(renderer);

            archetypalCart.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/cart");

            archetypalCart.Transform.Rotate(0f, 90f, 0f);
            archetypalCart.Transform.Translate(-9f, 1.3f, -155f);
            archetypalCart.Transform.SetScale(1.5f, 1f, 1.1f);

            renderer.Material = material;

            level.Add(archetypalCart);
        }
        #endregion



        private void InitializeHelmet(Scene level)
        {
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/paint_and_metal/yellow"));

            var archetypalHelmet = new GameObject("helmet", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/helmet"), material);
            renderer.Material = material;


            archetypalHelmet.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/helmet");

            archetypalHelmet.Transform.Rotate(270f, 270, 0f);
            archetypalHelmet.Transform.Translate(30f, 1.3f, -20f);
            level.Add(archetypalHelmet);
        }
        #endregion

        #region Structs

        #region YSplit
        private void InitializeStructs(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(7.7f, 8f, -29f);
            archetypalStructs.Transform.SetScale(1.2f, 0.8f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs1(Scene level)
        {
            //Y Split Left
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-2f, 6.3f, -75f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs3(Scene level)
        {
            //Y Split Left
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-4f, 6.3f, -95f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs4(Scene level)
        {
            //Y Split Left
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-4f, 6.3f, -115f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs5(Scene level)
        {
            //Y Split Left
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-4, 6.3f, -135f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs10(Scene level)
        {
            //Y Split Left
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-4, 6.3f, -155f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs11(Scene level)
        {
            //Y Split Left
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-4, 6.3f, -170f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs2(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(15f, 6.3f, -75f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs6(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(15f, 6.3f, -95f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs7(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(15f, 6.3f, -115f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs8(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(15f, 6.3f, -135f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs12(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(15f, 6.3f, -155f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs13(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(15f, 6.3f, -175f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }
        #endregion

        #region BendShaft
        private void InitializeStructs9(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-1.7f, 6.3f, -85f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs14(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(15f, 6.3f, -187f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs15(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-29f, 6.3f, -6.7f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs16(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-49f, 6.3f, -7);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs17(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-62f, 6.3f, -7);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs18(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-62f, 6.3f, 5);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs19(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-62f, 6.3f, 25);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs20(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-62f, 6.3f, 44);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs21(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-76f, 6.3f, 44.5f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs22(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-90f, 6.3f, 44f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs23(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-116f, 6.3f, 44f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs24(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-117f, 6.3f, 59f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs25(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-117.3f, 6.3f, 80f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs26(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-117.3f, 6.3f, 94f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs27(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-132f, 6.3f, 95.5f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs28(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-144f, 6.3f, 95.5f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs29(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-164f, 6.3f, 95.5f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs30(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-176f, 6.3f, 95.5f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs31(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-177.4f, 6.3f, 110f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs32(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-178f, 5.5f, 125f);
            archetypalStructs.Transform.SetScale(1.5f, 0.6f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        #endregion

        #region LoopShaft
        private void InitializeStructs33(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-11f, 6.3f, 34f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs34(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-11.5f, 6.3f, 50f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }


        private void InitializeStructs35(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-8f, 6f, 52.4f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs36(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(28f, 6f, 52.4f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs37(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(48f, 6f, 52.4f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs38(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(68f, 6f, 52.4f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs39(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(88f, 6f, 52.4f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs40(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(104f, 6f, 52.4f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs41(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(115.5f, 6f, 50f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs42(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(116f, 6f, 30f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs43(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(116f, 6f, 8f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs44(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(104f, 6f, -4f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs45(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(84f, 6f, -4f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs46(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(64f, 6f, -3f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs47(Scene level)
        {
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(70f, 6f, -4f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }


        #endregion

        #region Other Structs

        private void InitializeStructs48(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(3f, 6.3f, -199f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs49(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-13f, 6.3f, -198.8f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs50(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-34.2f, 6.3f, -198.8f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs51(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-36.3f, 6.3f, -198.8f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs52(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-36.3f, 6.3f, -218.8f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs53(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-36.5f, 6.3f, -230f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs54(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-51f, 6.3f, -242.7f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs55(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-68f, 6.3f, -242.7f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs56(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-85f, 6.3f, -242.7f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs57(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 180f, 0f);
            archetypalStructs.Transform.Translate(-99f, 6.3f, -242.7f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs58(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-100.2f, 6.3f, -228f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }

        private void InitializeStructs59(Scene level)
        {
            //Y Split Right
            //Support Structures
            var material = new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            var archetypalStructs = new GameObject("Support", GameObjectType.Interactable);
            var renderer = new ModelRenderer(Content.Load<Model>("Assets/Models/structs"), material);
            renderer.Material = material;
            archetypalStructs.AddComponent(renderer);

            archetypalStructs.AddComponent(renderer);
            renderer.Model = Content.Load<Model>("Assets/Models/structs");

            archetypalStructs.Transform.Rotate(-90f, 90f, 0f);
            archetypalStructs.Transform.Translate(-100.2f, 6.3f, -214f);
            archetypalStructs.Transform.SetScale(2f, 0.7f, 8f);

            renderer.Material = material;

            level.Add(archetypalStructs);
        }




        #endregion

        #endregion
        /// <summary>
        /// Adds menu and UI elements
        /// </summary>
        private void InitializeUI()
        {
            InitializeGameMenu();
            InitializeGameUI();
        }

        /// <summary>
        /// Adds main menu elements
        /// </summary>
        private void InitializeGameMenu()
        {
            //a re-usable variable for each ui object
            UIObject menuObject = null;

            #region Main Menu

            /************************** Main Menu Scene **************************/
            //make the main menu scene
            var mainMenuUIScene = new UIScene(AppData.MENU_MAIN_NAME);

            /**************************** Background Image ****************************/
            /*
            //main background
            var texture = textureDictionary["mainmenu"];
            //get how much we need to scale background to fit screen, then downsizes a little so we can see game behind background
            var scale = _graphics.GetScaleForTexture(texture,
                new Vector2(0.8f, 0.8f));

            menuObject = new UITextureObject("main background",
                UIObjectType.Texture,
                new Transform2D(Screen.Instance.ScreenCentre, scale, 0), //sets position as center of screen
                0,
                new Color(255, 255, 255, 200),
                texture.GetOriginAtCenter(), //if we want to position image on screen center then we need to set origin as texture center
                texture);

            */
            var texture = textureDictionary["progress_white"];
            var position = new Vector2(_graphics.PreferredBackBufferWidth / 2, 50);
            var origin = new Vector2(texture.Width / 2, texture.Height / 2);

            //var mainGameUIScene = new UIScene(AppData.UI_SCENE_MAIN_NAME);

            
            //add a health bar in the centre of the game window

            texture = textureDictionary["checkerboard"]; //any texture given we will replace it
            position = new Vector2(200, 200);

            var video = videoDictionary["main_menu_video"];
            origin = new Vector2(video.Width / 2, video.Height / 2);

            //create the UI element
            var videoTextureObj = new UITextureObject("main_menu_video",
                UIObjectType.Texture,

                new Transform2D(origin, new Vector2(1f, 1f), 0),
                0,
                Color.White,
                origin,
                texture);

            //add a video behaviour
            videoTextureObj.AddComponent(new UIVideoTextureBehaviour(
                new VideoCue(video, 0, false, true)));
            

            menuObject = videoTextureObj;
            
            //add the ui element to the scene
            mainMenuUIScene.Add(videoTextureObj);

            //add ui object to scene
            mainMenuUIScene.Add(menuObject);

            object[] parameters = { "main menu video" };
            EventDispatcher.Raise(new EventData(EventCategoryType.Video,
                    EventActionType.OnPlay, parameters));

            //add ui object to scene
            //mainMenuUIScene.Add(menuObject);

            /**************************** Play Button ****************************/

            var btnTexture = textureDictionary["genericbtn"];
            var sourceRectangle
                = new Microsoft.Xna.Framework.Rectangle(2, 2,
                btnTexture.Width, btnTexture.Height);
            var btnOrigin = new Vector2(btnTexture.Width / 2.0f, btnTexture.Height / 2.0f);

            var playBtn = new UIButtonObject(AppData.MENU_PLAY_BTN_NAME, UIObjectType.Button,
                new Transform2D(AppData.MENU_PLAY_BTN_POSITION,
                1f * Vector2.One, 0.58f),
                0.1f,
                Color.White,
                SpriteEffects.None,
                btnOrigin,
                btnTexture,
                null,
                sourceRectangle,
                "Play",
                fontDictionary["menu"],
                Color.Black,
                Vector2.Zero);

            //demo button color change
            playBtn.AddComponent(new UIColorMouseOverBehaviour(Color.Green, Color.White));

            mainMenuUIScene.Add(playBtn);

            /**************************** Controls Button ****************************/

            //same button texture so we can re-use texture, sourceRectangle and origin

            var controlsBtn = new UIButtonObject(AppData.MENU_CREDITS_BTN_NAME, UIObjectType.Button,
                 new Transform2D(AppData.MENU_CREDITS_BTN_POSITION, 1f * Vector2.One, -0.1f),
                 0.1f,
                 Color.White,
                 btnOrigin,
                 btnTexture,
                 "Credits",
                 fontDictionary["menu"],
                 Color.Black);

            //demo button color change
            controlsBtn.AddComponent(new UIColorMouseOverBehaviour(Color.Orange, Color.White));

            mainMenuUIScene.Add(controlsBtn);

            /**************************** Exit Button ****************************/

            //same button texture so we can re-use texture, sourceRectangle and origin

            //use a simple/smaller version of the UIButtonObject constructor
            var exitBtn = new UIButtonObject(AppData.MENU_EXIT_BTN_NAME, UIObjectType.Button,
                new Transform2D(AppData.MENU_EXIT_BTN_POSITION, 1f * Vector2.One, -0.5f),
                0.1f,
                Color.Orange,
                btnOrigin,
                btnTexture,
                "Exit",
                fontDictionary["menu"],
                Color.Black);

            //demo button color change
            exitBtn.AddComponent(new UIColorMouseOverBehaviour(Color.Red, Color.White));

            mainMenuUIScene.Add(exitBtn);

            #endregion Main Menu

            //add scene to the menu manager
            uiMenuManager.Add(mainMenuUIScene);

            /************************** Credits Menu Scene **************************/
            var cresitsMenuUIScene = new UIScene(AppData.MENU_CREDITS_NAME);
            texture = textureDictionary["progress_white"];
            position = new Vector2(_graphics.PreferredBackBufferWidth / 2, 50);
            origin = new Vector2(texture.Width / 2, texture.Height / 2);


            /************************** Options Menu Scene **************************/

            /************************** Exit Menu Scene **************************/

            //finally we say...where do we start
            uiMenuManager.SetActiveScene(AppData.MENU_MAIN_NAME);
        }

        /// <summary>
        /// Adds ui elements seen in-game (e.g. health, timer)
        /// </summary>
        private void InitializeGameUI()
        {
            
            //create the scene
            var mainGameUIScene = new UIScene(AppData.UI_SCENE_MAIN_NAME);
            
            #region Add Health Bar
            /*
            //add a health bar in the centre of the game window
            var texture = textureDictionary["progress_white"];
            var position = new Vector2(_graphics.PreferredBackBufferWidth / 2, 50);
            var origin = new Vector2(texture.Width / 2, texture.Height / 2);

            //create the UI element
            var healthTextureObj = new UITextureObject("health",
                UIObjectType.Texture,
                new Transform2D(position, new Vector2(2, 0.5f), 0),
                0,
                Color.White,
                origin,
                texture);

            //add a demo time based behaviour - because we can!
            healthTextureObj.AddComponent(new UITimeColorFlipBehaviour(Color.White, Color.Red, 1000));

            //add a progress controller
            healthTextureObj.AddComponent(new UIProgressBarController(5, 10));

            //add the ui element to the scene
            mainGameUIScene.Add(healthTextureObj);
            */
            #endregion Add Health Bar
            
            //create the scene
            //var mainGameUIScene = new UIScene(AppData.UI_SCENE_MAIN_NAME);

            #region Add Health Bar
            //add a health bar in the centre of the game window
            /*
            var texture = textureDictionary["HP_Bar_V2"];
            var position = new Vector2(_graphics.PreferredBackBufferWidth / 1.005f, 1020);
            var origin = new Vector2(texture.Width / 2, texture.Height / 2);

            //create the UI element
            var healthTextureObj = new UITextureObject("health",
                UIObjectType.Texture,
                new Transform2D(position, new Vector2(1.40f, 0.50f), 0),
                0,
                Color.White,
                origin,
                texture);

            //add a progress controller
            healthTextureObj.AddComponent(new UIProgressBarController(5, 10));
            */
            
            var texture = textureDictionary["HP_Bar_V2"];
            var position = new Vector2(_graphics.PreferredBackBufferWidth / 1.08f, 1020);
            var origin = new Vector2(texture.Width / 2, texture.Height / 2);

            var healthTextureObj = new UITextureObject("health",
                UIObjectType.Texture,
                new Transform2D(position, new Vector2(1.06f, 0.5f), 0),
                0,
                Color.Green,
                origin,
                texture);
            healthTextureObj.AddComponent(new UITimeColorFlipBehaviour(Color.Green, Color.Pink, 2000));
            healthTextureObj.AddComponent(new UiHealthController(100, 100));

            
            mainGameUIScene.Add(healthTextureObj);
            #endregion

            //Creating the HUD
            var hudTextureObj = new UITextureObject("HUD",
                 UIObjectType.Texture,
                 new Transform2D(new Vector2(0, 0),
                 new Vector2(1, 1),
                 MathHelper.ToRadians(0)),
                 0, Content.Load<Texture2D>("Assets/Textures/UI/Progress/hud"));
            //add the ui element to the scene
            hudTextureObj.Color = Color.White;
            mainGameUIScene.Add(healthTextureObj);
            mainGameUIScene.Add(hudTextureObj);


            #region Add Text

            var font = fontDictionary["ui"];
            var str = "Steve";

            //create the UI element
            nameTextObj = new UITextObject(str, UIObjectType.Text,
                new Transform2D(new Vector2(50, 50),
                Vector2.One, 0),
                0, font, "beta release");

            //  nameTextObj.Origin = font.MeasureString(str) / 2;
            //  nameTextObj.AddComponent(new UIExpandFadeBehaviour());

            //add the ui element to the scene
            mainGameUIScene.Add(nameTextObj);

            #endregion Add Text
            /*
            var defaultTexture = textureDictionary["reticuleDefault"];
            var alternateTexture = textureDictionary["reticuleOpen"];
            origin = defaultTexture.GetOriginAtCenter();

            var reticule = new UITextureObject("reticule",
                     UIObjectType.Texture,
                new Transform2D(Vector2.Zero, Vector2.One, 0),
                0,
                Color.White,
                SpriteEffects.None,
                origin,
                defaultTexture,
                alternateTexture,
                new Microsoft.Xna.Framework.Rectangle(0, 0,
                defaultTexture.Width, defaultTexture.Height));

            reticule.AddComponent(new UIReticuleBehaviour());

            mainGameUIScene.Add(reticule);
            */

            #region Add Scene To Manager & Set Active Scene

            //add the ui scene to the manager
            uiSceneManager.Add(mainGameUIScene);

            //set the active scene
            uiSceneManager.SetActiveScene(AppData.UI_SCENE_MAIN_NAME);

            #endregion Add Scene To Manager & Set Active Scene
        }

        /// <summary>
        /// Adds component to draw debug info to the screen
        /// </summary>
         
        /*
        private void InitializeDebugUI(bool showDebugInfo, bool showCollisionSkins = true)
        {
            if (showDebugInfo)
            {
                Components.Add(new GDLibrary.Utilities.GDDebug.PerfUtility(this,
                    _spriteBatch, fontDictionary["debug"],
                    new Vector2(40, _graphics.PreferredBackBufferHeight - 80),
                    Color.White));
            }

            if (showCollisionSkins)
                Components.Add(new GDLibrary.Utilities.GDDebug.PhysicsDebugDrawer(this, Color.Red));
        }
        */
        /******************************* Non-Collidables *******************************/

        /// <summary>
        /// Set up the skybox using a QuadMesh
        /// </summary>
        /// <param name="level">Scene Stores all game objects for current...</param>
        /// <param name="worldScale">float Value used to scale skybox normally 250 - 1000</param>
        private void InitializeSkybox(Scene level, float worldScale = 500)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card
            var shader = new BasicShader(Application.Content, true, true);
            //re-use the vertices and indices of the primitive
            var mesh = new QuadMesh();
            //create an archetype that we can clone from
            var archetypalQuad = new GameObject("quad", GameObjectType.Skybox, true);

            #endregion Reusable - You can copy and re-use this code elsewhere, if required
            /*
            GameObject clone = null;
            //back
            clone = archetypalQuad.Clone() as GameObject;
            clone.Name = "skybox_back";
            clone.Transform.Translate(0, 0, -worldScale / 2.0f);
            clone.Transform.Scale(worldScale, worldScale, 1);
            clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("skybox_back_material", shader, Color.White, 1, textureDictionary["skybox_back"])));
            level.Add(clone);

            //left
            clone = archetypalQuad.Clone() as GameObject;
            clone.Name = "skybox_left";
            clone.Transform.Translate(-worldScale / 2.0f, 0, 0);
            clone.Transform.Scale(worldScale, worldScale, null);
            clone.Transform.Rotate(0, 90, 0);
            clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("skybox_left_material", shader, Color.White, 1, textureDictionary["skybox_left"])));
            level.Add(clone);

            //right
            clone = archetypalQuad.Clone() as GameObject;
            clone.Name = "skybox_right";
            clone.Transform.Translate(worldScale / 2.0f, 0, 0);
            clone.Transform.Scale(worldScale, worldScale, null);
            clone.Transform.Rotate(0, -90, 0);
            clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("skybox_right_material", shader, Color.White, 1, textureDictionary["skybox_right"])));
            level.Add(clone);

            //front
            clone = archetypalQuad.Clone() as GameObject;
            clone.Name = "skybox_front";
            clone.Transform.Translate(0, 0, worldScale / 2.0f);
            clone.Transform.Scale(worldScale, worldScale, null);
            clone.Transform.Rotate(0, -180, 0);
            clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("skybox_front_material", shader, Color.White, 1, textureDictionary["skybox_front"])));
            level.Add(clone);

            //top
            clone = archetypalQuad.Clone() as GameObject;
            clone.Name = "skybox_sky";
            clone.Transform.Translate(0, worldScale / 2.0f, 0);
            clone.Transform.Scale(worldScale, worldScale, null);
            clone.Transform.Rotate(90, -90, 0);
            clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("skybox_sky_material", shader, Color.White, 1, textureDictionary["skybox_sky"])));
            level.Add(clone);
        */
        }

        /// <summary>
        /// Initialize the camera(s) in our scene
        /// </summary>
        /// <param name="level"></param>
        private void InitializeCameras(Scene level)
        {
            #region First Person Camera - Non Collidable

            //add camera game object
            var camera = new GameObject(AppData.CAMERA_FIRSTPERSON_NONCOLLIDABLE_NAME, GameObjectType.Camera);

            //add components
            //here is where we can set a smaller viewport e.g. for split screen
            //e.g. new Viewport(0, 0, _graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight)
            camera.AddComponent(new Camera(_graphics.GraphicsDevice.Viewport));

            //add controller to actually move the noncollidable camera
            camera.AddComponent(new FirstPersonController(0.8f, 0.5f, new Vector2(0.5f, 0.4f))); //LEE CHANGED VALUES FROM 0.05f, 0.035f////////0.0012F AND 0.04        //set initial position
            camera.Transform.SetTranslation(100, 100, 100);//10,12,10

            //add to level
            level.Add(camera);

            #endregion First Person Camera - Non Collidable

            #region Curve Camera - Non Collidable

            //add curve for camera translation
            var translationCurve = new Curve3D(CurveLoopType.Cycle);
            translationCurve.Add(new Vector3(0, 2, 10), 0);
            translationCurve.Add(new Vector3(0, 8, 15), 1000);
            translationCurve.Add(new Vector3(0, 8, 20), 2000);
            translationCurve.Add(new Vector3(0, 6, 25), 3000);
            translationCurve.Add(new Vector3(0, 4, 25), 4000);
            translationCurve.Add(new Vector3(0, 2, 10), 6000);

            //add camera game object
            var curveCamera = new GameObject(AppData.CAMERA_CURVE_NONCOLLIDABLE_NAME, GameObjectType.Camera);

            //add components
            curveCamera.AddComponent(new Camera(_graphics.GraphicsDevice.Viewport));
            curveCamera.AddComponent(new CurveBehaviour(translationCurve));
            curveCamera.AddComponent(new FOVOnScrollController(MathHelper.ToRadians(2)));

            //add to level
            level.Add(curveCamera);

            #endregion Curve Camera - Non Collidable

            #region First Person Camera - Collidable

            //add camera game object
            camera = new GameObject(AppData.CAMERA_FIRSTPERSON_COLLIDABLE_NAME, GameObjectType.Camera);

            //set initial position - important to set before the collider as collider capsule feeds off this position
            camera.Transform.SetTranslation(30, 10, 30);     //  , y, 

            //add components
            camera.AddComponent(new Camera(_graphics.GraphicsDevice.Viewport));

            //adding a collidable surface that enables acceleration, jumping
            //var collider = new CharacterCollider(2, 2, true, false);

            var collider = new MyHeroCollider(2, 2, true, false);
            camera.AddComponent(collider);
            collider.AddPrimitive(new Capsule(camera.Transform.LocalTranslation,
                Matrix.CreateRotationX(MathHelper.PiOver2), 6, 3.6f),//1,3.6f    6 is the height of the capsule
                new MaterialProperties(0.2f, 10f, 0.7f));//0.2f, 0.8f, 0.7f
            collider.Enable(false, 2);

            //add controller to actually move the collidable camera
            camera.AddComponent(new MyCollidableFirstPersonController(12,
                        0.5f, 0.3f, new Vector2(0.03f, 0.02f)));//12,0.5f,0.3f//0.006f, 0.004f

            //add to level
            level.Add(camera);

            #endregion First Person Camera - Collidable

            //set the main camera, if we dont call this then the first camera added will be the Main
            level.SetMainCamera(AppData.CAMERA_FIRSTPERSON_COLLIDABLE_NAME);

            //allows us to scale time on all game objects that based movement on Time
            // Time.Instance.TimeScale = 0.1f;
        }

        /******************************* Collidables *******************************/

        /// <summary>
        /// Demo of the new physics manager and collidable objects
        /// </summary>
        private void InitializeCollidables(Scene level, float worldScale = 500)
        {
            InitializeCollidableGround(level, worldScale);
            InitializeCollidableForeman1Cubes(level);

           //InitializeCollidableModels(level);
            //InitializeCollidableTriangleMeshes(level);
        }

        private void InitializeCollidableTriangleMeshes(Scene level)
        {
            ////re-use the code on the gfx card, if we want to draw multiple objects using Clone
            //var shader = new BasicShader(Application.Content, false, true);

            ////create the teapot
            //var complexModel = new GameObject("teapot", GameObjectType.Environment, true);
            //complexModel.Transform.SetTranslation(0, 5, 0);
            ////        complexModel.Transform.SetScale(0.4f, 0.4f, 0.4f);
            //complexModel.AddComponent(new ModelRenderer(
            //    modelDictionary["monkey1"],
            //    new BasicMaterial("teapot_material", shader,
            //    Color.White, 1, textureDictionary["mona lisa"])));

            ////add Collision Surface(s)
            //collider = new Collider();
            //complexModel.AddComponent(collider);
            //collider.AddPrimitive(
            //    CollisionUtility.GetTriangleMesh(modelDictionary["monkey1"],
            //    new Vector3(0, 5, 0), new Vector3(90, 0, 0), new Vector3(0.5f, 0.5f, 0.5f)),
            //    new MaterialProperties(0.8f, 0.8f, 0.7f));
            //collider.Enable(true, 1);

            ////add To Scene Manager
            //level.Add(complexModel);
        }

        /*
        private void InitializeCollidableModels(Scene level)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);

            //create the sphere
            var sphereArchetype = new GameObject("sphere", GameObjectType.Interactable, true);

            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            GameObject clone = null;

            for (int i = 0; i < 5; i++)
            {
                clone = sphereArchetype.Clone() as GameObject;
                clone.Name = $"sphere - {i}";

                clone.Transform.SetTranslation(5 + i / 10f, 5 + 4 * i, 0);
                clone.AddComponent(new ModelRenderer(
                    modelDictionary["sphere"],
                    new BasicMaterial("sphere_material",
                    shader, Color.White, 1, textureDictionary["checkerboard"])));

                //add Collision Surface(s)
                collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new JigLibX.Geometry.Sphere(
                   sphereArchetype.Transform.LocalTranslation, 1),
                    new MaterialProperties(0.8f, 0.8f, 0.7f));
                collider.Enable(false, 1);

                //add To Scene Manager
                level.Add(clone);
            }
        }
        */

        private void InitializeCollidableGround(Scene level, float worldScale)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);
            //re-use the vertices and indices of the model
            var mesh = new QuadMesh();

            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            //create the ground
            var ground = new GameObject("ground", GameObjectType.Ground, true);
            ground.Transform.SetRotation(-90, 0, 0);
            ground.Transform.SetScale(worldScale, worldScale, 1);
            ground.AddComponent(new MeshRenderer(mesh, new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/Cave/Rock")))); //new BasicMaterial("grass_material", shader, Color.White, 1, textureDictionary["grass"])));
            //new BasicMaterial("model material", new BasicShader(Application.Content), Content.Load<Texture2D>("Assets/Textures/wood/Wood"));

            //add Collision Surface(s)
            collider = new Collider();
            ground.AddComponent(collider);
            collider.AddPrimitive(new JigLibX.Geometry.Plane(
                ground.Transform.Up, ground.Transform.LocalTranslation),
                new MaterialProperties(0.8f, -0.4f, 0.7f));
            collider.Enable(true, 1);

            //add To Scene Manager
            level.Add(ground);
        }

        private void InitializeCollidableForeman1Cubes(Scene level)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);
            //re-use the mesh
            var mesh = new CubeMesh();
            //clone the cube
            var gearProp = new GameObject("gear", GameObjectType.Consumable, false);

            
            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            GameObject clone = null;

            for (int i = 1; i < 2; i += 1)
            {
                //clone the archetypal cube
                clone = gearProp.Clone() as GameObject;
                clone.Name = $"gear - {i}";
                clone.Transform.Translate(0, 1 + i, 0);
                clone.AddComponent(new MeshRenderer(mesh,
                    new BasicMaterial("gear", shader,
                    Color.White, 1, textureDictionary["oldmetal"])));

                //add desc and value to a pickup used when we collect/remove/collide with it
                clone.AddComponent(new PickupBehaviour("ammo pack", 15, "PlayerForeman line1"));

                //add Collision Surface(s)
                collider = new MyPlayerCollider();
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    gearProp.Transform.LocalTranslation,
                    gearProp.Transform.LocalRotation,
                    gearProp.Transform.LocalScale),
                    new MaterialProperties(0.8f, 0.8f, 0.7f));
                collider.Enable(false, 10);

                //add the playerforeman line to play when the gearProp is spawned

                object[] parameters3 = { "PlayerForeman line1" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters3));
                

                //add To Scene Manager
                level.Add(clone);
            }
        }

        #endregion Student/Group Specific Code

        /******************************* Demo (Remove For Release) *******************************/

        #region Demo Code

#if DEMO

        public delegate void MyDelegate(string s, bool b);

        public List<MyDelegate> delList = new List<MyDelegate>();

        public void DoSomething(string msg, bool enableIt)
        {
        }

        private void InitializeEditorHelpers()
        {
            //a game object to record camera positions to an XML file for use in a curve later
            var curveRecorder = new GameObject("curve recorder", GameObjectType.Editor);
            curveRecorder.AddComponent(new GDLibrary.Editor.CurveRecorderController());
            activeScene.Add(curveRecorder);
        }

        private void RunDemos()
        {
            // CurveDemo();
            // SaveLoadDemo();

            EventSenderDemo();
        }

        private void EventSenderDemo()
        {
            var myDel = new MyDelegate(DoSomething);
            myDel("sdfsdfdf", true);
            delList.Add(DoSomething);
        }

        private void CurveDemo()
        {
            //var curve1D = new GDLibrary.Parameters.Curve1D(CurveLoopType.Cycle);
            //curve1D.Add(0, 0);
            //curve1D.Add(10, 1000);
            //curve1D.Add(20, 2000);
            //curve1D.Add(40, 4000);
            //curve1D.Add(60, 6000);
            //var value = curve1D.Evaluate(500, 2);
        }

        private void SaveLoadDemo()
        {
        #region Serialization Single Object Demo

            var demoSaveLoad = new DemoSaveLoad(new Vector3(1, 2, 3), new Vector3(45, 90, -180), new Vector3(1.5f, 0.1f, 20.25f));
            GDLibrary.Utilities.SerializationUtility.Save("DemoSingle.xml", demoSaveLoad);
            var readSingle = GDLibrary.Utilities.SerializationUtility.Load("DemoSingle.xml",
                typeof(DemoSaveLoad)) as DemoSaveLoad;

        #endregion Serialization Single Object Demo

        #region Serialization List Objects Demo

            List<DemoSaveLoad> listDemos = new List<DemoSaveLoad>();
            listDemos.Add(new DemoSaveLoad(new Vector3(1, 2, 3), new Vector3(45, 90, -180), new Vector3(1.5f, 0.1f, 20.25f)));
            listDemos.Add(new DemoSaveLoad(new Vector3(10, 20, 30), new Vector3(4, 9, -18), new Vector3(15f, 1f, 202.5f)));
            listDemos.Add(new DemoSaveLoad(new Vector3(100, 200, 300), new Vector3(145, 290, -80), new Vector3(6.5f, 1.1f, 8.05f)));

            GDLibrary.Utilities.SerializationUtility.Save("ListDemo.xml", listDemos);
            var readList = GDLibrary.Utilities.SerializationUtility.Load("ListDemo.xml",
                typeof(List<DemoSaveLoad>)) as List<DemoSaveLoad>;

        #endregion Serialization List Objects Demo
        }

#endif

        #endregion Demo Code
    }
}