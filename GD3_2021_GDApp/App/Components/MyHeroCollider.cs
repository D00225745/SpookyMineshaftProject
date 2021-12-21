using GDLibrary;
using GDLibrary.Components;
using GDLibrary.Core;
using JigLibX.Collision;

namespace GDApp
{
    public class MyHeroCollider : CharacterCollider
    {
        public MyHeroCollider(float accelerationRate, float decelerationRate,
       bool isHandlingCollision = true, bool isTrigger = false)
            : base(accelerationRate, decelerationRate, isHandlingCollision, isTrigger)
        {
        }

        protected override void HandleResponse(GameObject parentGameObject)
        {
            if (parentGameObject.GameObjectType == GameObjectType.Consumable)
            {
                System.Diagnostics.Debug.WriteLine(parentGameObject?.Name);

                object[] parameters = { parentGameObject };
                EventDispatcher.Raise(new EventData(EventCategoryType.GameObject,
                    EventActionType.OnRemoveObject, parameters));

                //this accesses sound, after accessing it raise the event by EventDispatcher.raise
                var pickupBehaviour = parentGameObject.GetComponent<PickupBehaviour>();

                //below is original work for raising var pickupBehaviour that doesn't work
                object[] parameters1 = { pickupBehaviour };
                //raises the event as eventDispatcher
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                     EventActionType.OnPlay2D, parameters1));
                //pickupBehaviour.Sound



                
                object[] parameters2 = { "PlayerForeman line2" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters2));
                

                /*
                object[] parameters2 = { "Foreman lineplayerdefeat" };
                //raises the event as eventDispatcher
                EventDispatcher.Raise(new EventData(EventCategoryType.UI,
                     EventActionType.OnLose, parameters2));

                object[] parameters3 = { "Foreman lineplayervictory" };
                //raises the event as eventDispatcher
                EventDispatcher.Raise(new EventData(EventCategoryType.UI,
                     EventActionType.OnWin, parameters3));
                */

                /* object[] parameters1 = { "health", 1 };
                 EventDispatcher.Raise(new EventData(EventCategoryType.UI,
                     EventActionType.OnHealthDelta, parameters1));
                */
                // EventDispatcher.Raise(new EventData(EventCategoryType.Inventory,
                //  EventActionType.OnAddInventory, parameters1));
            }

            base.HandleResponse(parentGameObject);
        }


    }
}