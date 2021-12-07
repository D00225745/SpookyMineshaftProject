﻿//#define DEMO

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

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.P))
            {
                EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
                          EventActionType.OnPause));
            }
            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.O))
            {
                EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
                    EventActionType.OnPlay));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                object[] parameters = { "MineWhispers" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.W))
            {
                object[] parameters = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.S))
            {
                object[] parameters = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.D))
            {
                object[] parameters = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.A))
            {
                object[] parameters = { "Steps" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.F))
            {
                object[] parameters = { "FlashlightFlickOn" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                object[] parameters = { "Breathing2" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                object[] parameters = { "Heartbeat" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Not much happens in here as RenderManager, UISceneManager and MenuManager are all DrawableGameComponents that automatically Draw()
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
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
            InitializeDebugUI(true, true);

            //to show the menu we must start paused for everything else!
            EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
                        EventActionType.OnPause));

            base.Initialize();
        }

        /******************************* Load/Unload Assets *******************************/

        private void InitializeDictionaries()
        {
            textureDictionary = new Dictionary<string, Texture2D>();

            //why not try the new and improved ContentDictionary instead of a basic Dictionary?
            fontDictionary = new ContentDictionary<SpriteFont>();
            modelDictionary = new ContentDictionary<Model>();
        }

        private void LoadAssets()
        {
            LoadModels();
            LoadTextures();
            LoadSounds();
            LoadFonts();
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

            //add the new sound effect

            soundManager.Add(new GDLibrary.Managers.Cue(
                "MineWhispers",
                soundEffect,
                SoundCategoryType.BackgroundMusic,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Steps",
                soundEffect1,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            soundManager.Add(new GDLibrary.Managers.Cue(
                "Breathing2",
                soundEffect2,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

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
                false));

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

            //ui
            textureDictionary.Add("ui_progress_32_8", Content.Load<Texture2D>("Assets/Textures/UI/Controls/ui_progress_32_8"));
            textureDictionary.Add("progress_white", Content.Load<Texture2D>("Assets/Textures/UI/Controls/progress_white"));

            //menu
            textureDictionary.Add("mainmenu", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/mainmenu"));
            textureDictionary.Add("audiomenu", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/audiomenu"));
            textureDictionary.Add("controlsmenu", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/controlsmenu"));
            textureDictionary.Add("exitmenuwithtrans", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/exitmenuwithtrans"));
            textureDictionary.Add("genericbtn", Content.Load<Texture2D>("Assets/Textures/UI/Controls/genericbtn"));
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
            //InitializeCubes(activeScene);
            //InitializeModels(activeScene);

            InitializeCollidables(activeScene, worldScale);

            sceneManager.Add(activeScene);
            sceneManager.LoadScene("level 1");
        }

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

            //add ui object to scene
            mainMenuUIScene.Add(menuObject);

            /**************************** Play Button ****************************/

            var btnTexture = textureDictionary["genericbtn"];
            var sourceRectangle
                = new Microsoft.Xna.Framework.Rectangle(0, 0,
                btnTexture.Width, btnTexture.Height);
            var origin = new Vector2(btnTexture.Width / 2.0f, btnTexture.Height / 2.0f);

            var playBtn = new UIButtonObject(AppData.MENU_PLAY_BTN_NAME, UIObjectType.Button,
                new Transform2D(AppData.MENU_PLAY_BTN_POSITION,
                0.5f * Vector2.One, 0),
                0.1f,
                Color.White,
                SpriteEffects.None,
                origin,
                btnTexture,
                null,
                sourceRectangle,
                "Play",
                fontDictionary["menu"],
                Color.Black,
                Vector2.Zero);

            //demo button color change
            playBtn.AddComponent(new UIColorMouseOverBehaviour(Color.Orange, Color.White));

            mainMenuUIScene.Add(playBtn);

            /**************************** Controls Button ****************************/

            //same button texture so we can re-use texture, sourceRectangle and origin

            var controlsBtn = new UIButtonObject(AppData.MENU_CONTROLS_BTN_NAME, UIObjectType.Button,
                new Transform2D(AppData.MENU_CONTROLS_BTN_POSITION, 0.5f * Vector2.One, 0),
                0.1f,
                Color.White,
                origin,
                btnTexture,
                "Controls",
                fontDictionary["menu"],
                Color.Black);

            //demo button color change
            controlsBtn.AddComponent(new UIColorMouseOverBehaviour(Color.Orange, Color.White));

            mainMenuUIScene.Add(controlsBtn);

            /**************************** Exit Button ****************************/

            //same button texture so we can re-use texture, sourceRectangle and origin

            //use a simple/smaller version of the UIButtonObject constructor
            var exitBtn = new UIButtonObject(AppData.MENU_EXIT_BTN_NAME, UIObjectType.Button,
                new Transform2D(AppData.MENU_EXIT_BTN_POSITION, 0.5f * Vector2.One, 0),
                0.1f,
                Color.Orange,
                origin,
                btnTexture,
                "Exit",
                fontDictionary["menu"],
                Color.Black);

            //demo button color change
            exitBtn.AddComponent(new UIColorMouseOverBehaviour(Color.Orange, Color.White));

            mainMenuUIScene.Add(exitBtn);

            #endregion Main Menu

            //add scene to the menu manager
            uiMenuManager.Add(mainMenuUIScene);

            /************************** Controls Menu Scene **************************/

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

            #endregion Add Health Bar

            #region Add Text

            var font = fontDictionary["ui"];
            var str = "player name";

            //create the UI element
            nameTextObj = new UITextObject(str, UIObjectType.Text,
                new Transform2D(new Vector2(50, 50),
                Vector2.One, 0),
                0, font, "Brutus Maximus");

            //  nameTextObj.Origin = font.MeasureString(str) / 2;
            //  nameTextObj.AddComponent(new UIExpandFadeBehaviour());

            //add the ui element to the scene
            mainGameUIScene.Add(nameTextObj);

            #endregion Add Text

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
        private void InitializeDebugUI(bool showDebugInfo, bool showCollisionSkins = true)
        {
            if (showDebugInfo)
            {
                Components.Add(new GDLibrary.Utilities.GDDebug.PerfUtility(this,
                    _spriteBatch, fontDictionary["debug"],
                    new Vector2(40, _graphics.PreferredBackBufferHeight - 40),
                    Color.White));
            }

            if (showCollisionSkins)
                Components.Add(new GDLibrary.Utilities.GDDebug.PhysicsDebugDrawer(this, Color.Red));
        }

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
        }

        /// <summary>
        /// Initialize the camera(s) in our scene
        /// </summary>
        /// <param name="level"></param>
        private void InitializeCameras(Scene level)
        {
            #region First Person Camera

            //add camera game object
            var camera = new GameObject("main camera", GameObjectType.Camera);

            //add components
            //here is where we can set a smaller viewport e.g. for split screen
            //e.g. new Viewport(0, 0, _graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight)
            camera.AddComponent(new Camera(_graphics.GraphicsDevice.Viewport));
            camera.AddComponent(new FirstPersonController(0.05f, 0.025f, 0.00009f));

            //set initial position
            camera.Transform.SetTranslation(0, 2, 10);

            //add to level
            level.Add(camera);

            #endregion First Person Camera

            #region Curve Camera

            //add curve for camera translation
            var translationCurve = new Curve3D(CurveLoopType.Cycle);
            translationCurve.Add(new Vector3(0, 1, 10), 0);
            translationCurve.Add(new Vector3(0, 6, 15), 1000);
            translationCurve.Add(new Vector3(0, 1, 20), 2000);
            translationCurve.Add(new Vector3(0, -6, 25), 3000);
            translationCurve.Add(new Vector3(0, 1, 30), 4000);
            translationCurve.Add(new Vector3(0, 1, 10), 6000);

            //add camera game object
            var curveCamera = new GameObject("curve camera", GameObjectType.Camera);

            //add components
            curveCamera.AddComponent(new Camera(_graphics.GraphicsDevice.Viewport));
            curveCamera.AddComponent(new CurveBehaviour(translationCurve));
            curveCamera.AddComponent(new FOVOnScrollController(MathHelper.ToRadians(2)));

            //add to level
            level.Add(curveCamera);

            #endregion Curve Camera

            //set theMain camera, if we dont call this then the first camera added will be the Main
            level.SetMainCamera("main camera");

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
            InitializeCollidableCubes(level);

            InitializeCollidableModels(level);
            InitializeCollidableTriangleMeshes(level);
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

        private void InitializeCollidableModels(Scene level)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);

            //create the sphere
            var sphereArchetype = new GameObject("sphere",
                GameObjectType.Interactable, true);

            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            GameObject clone = null;

            for (int i = 0; i < 5; i++)
            {
                clone = sphereArchetype.Clone() as GameObject;
                clone.Name = $"sphere - {i}";
                clone.Transform.SetTranslation(5 + i / 10f, 5 + 4 * i, 0);
                clone.AddComponent(new ModelRenderer(modelDictionary["sphere"],
                    new BasicMaterial("sphere_material",
                    shader, Color.White, 1, textureDictionary["checkerboard"])));

                //add Collision Surface(s)
                collider = new Collider();
                clone.AddComponent(collider);
                collider.AddPrimitive(new JigLibX.Geometry.Sphere(
                   sphereArchetype.Transform.LocalTranslation, 1),
                    new MaterialProperties(0.8f, 0.8f, 0.7f));
                collider.Enable(false, 1);

                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void InitializeCollidableGround(Scene level, float worldScale)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);
            //re-use the vertices and indices of the model
            var mesh = new QuadMesh();

            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            //create the ground
            var ground = new GameObject("ground", GameObjectType.Environment, true);
            ground.Transform.SetRotation(-90, 0, 0);
            ground.Transform.SetScale(worldScale, worldScale, 1);
            ground.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1, textureDictionary["grass"])));
            //level.Add(ground);

            //add Collision Surface(s)
            collider = new Collider();
            ground.AddComponent(collider);
            collider.AddPrimitive(new JigLibX.Geometry.Plane(
                ground.Transform.Up, ground.Transform.LocalTranslation),
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            collider.Enable(true, 1);

            //add To Scene Manager
            level.Add(ground);
        }

        private void InitializeCollidableCubes(Scene level)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);
            //re-use the mesh
            var mesh = new CubeMesh();
            //clone the cube
            var cube = new GameObject("cube", GameObjectType.Consumable, false);

            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            GameObject clone = null;

            for (int i = 5; i < 40; i += 5)
            {
                //clone the archetypal cube
                clone = cube.Clone() as GameObject;
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(0, 5 + i, 0);
                clone.AddComponent(new MeshRenderer(mesh,
                    new BasicMaterial("cube_material", shader,
                    Color.White, 1, textureDictionary["crate1"])));

                //add desc and value to a pickup
                clone.AddComponent(new PickupBehaviour("ammo pack", 15));

                //add Collision Surface(s)
                collider = new Collider();
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    cube.Transform.LocalTranslation,
                    cube.Transform.LocalRotation,
                    cube.Transform.LocalScale),
                    new MaterialProperties(0.8f, 0.8f, 0.7f));
                collider.Enable(false, 10);

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