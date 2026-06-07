using UnityEngine;
using SoftKitty.InventoryEngine;

namespace SoftKitty
{

#if MASTER_COMBAT_CORE
    public class ItemAction:Action{
        public Entity entity(GraphRuntimeState _runtime) { //The target entity for item manipulation.
            return GetEntity(inputPort[1].connectedNodes[0],_runtime);
        }
        public enum inventoryTypes{
          Any_Available,
          Inventory,
          Equipment
        }
        public inventoryTypes inventoryType; //
        public bool hidden; //Wehether this item goes to hidden stacks.
        public float itemId(GraphRuntimeState _runtime) { //The unique integer id of the item
            return GetVariableValue(inputPort[2].connectedNodes[0],_runtime,-1);
        }
        public enum methods{
          Add,
          Remove_All
        }
        public methods method; //The method of the action.
        public float value(GraphRuntimeState _runtime) { //The value of the action.
            return GetVariableValue(inputPort[3].connectedNodes[0],_runtime);
        }
 
        public ItemAction(){}
 
        public ItemAction(Action _baseClass) : base(_baseClass){}
 
        public ItemAction InitAction(Action _baseClass){
            ItemAction  _newObj =new ItemAction(_baseClass);
            _newObj.inventoryType = (inventoryTypes)_baseClass.IntValues[0];
            _newObj.hidden = _baseClass.BoolValues[0];
            _newObj.method = (methods)_baseClass.IntValues[1];
            return _newObj;
        }

        int _id;
        Item _item;
        int _num;
        public override void Execute(GraphRuntimeState _runtime){ //Execute this action once
            if (entity(_runtime) != null)
            {
                _id = -1;
                _item = null;
                _id = Mathf.FloorToInt(itemId(_runtime));
                _item = ItemObject.instance.GetItem(_id);
                if (_item != null)
                {
                    InventoryData _inventory = null;
                    if (inventoryType == inventoryTypes.Equipment)
                    {
                        _inventory = entity(_runtime).GetModule<InventoryModule>().GetEquipment();
                    }
                    else if (inventoryType == inventoryTypes.Inventory)
                    {
                        _inventory = entity(_runtime).GetModule<InventoryModule>().GetInventory();
                    }
                    else
                    {
                        _inventory = entity(_runtime).GetModule<InventoryModule>().GetAnyInventoryData();
                    }

                    if (_inventory != null)
                    {
                         _num = Mathf.FloorToInt(value(_runtime));
                        if (method == methods.Add)
                        {
                            if (_num >= 0)
                            {
                                if (hidden)
                                {

                                    _inventory.AddHiddenItem(new Item(_id), _num);

                                }
                                else
                                {
                                    _inventory.AddItem(new Item(_id), _num);
                                }
                            }
                            else
                                _inventory.RemoveItem(_id, _num);

                            if (SGD_Settings.Instance.DebugLevel >= 3)
                                Debug.Log("<ItemAction> [Node:" + Uid + "] Executed() > (Entity)" + entity(_runtime).uid + " InventoryData("
                                    + _inventory.Name + " added item ( " + _item.name + " x " + _num.ToString() + " )"
                                    + (hidden ? " to Hidden Stacks." : "."));
                        }
                        else if (method == methods.Remove_All)
                        {
                            _inventory.RemoveItem(_id, _inventory.GetItemNumber(_id));
                            if (SGD_Settings.Instance.DebugLevel >= 3)
                                Debug.Log("<ItemAction> [Node:" + Uid + "] Executed() > (Entity)" + entity(_runtime).uid + " InventoryData("
                                    + _inventory.Name + " removed all item ( " + _item.name  + " )"
                                    + (hidden ? " from Hidden Stacks." : "."));
                        }


                       
                    }
                    else if(SGD_Settings.Instance.DebugLevel >= 1)
                    {
                        Debug.LogError("<ItemAction> [Node:" + Uid + "] failed to Execute(), because could not found desired type of inventory on Entity(UID: " + entity(_runtime).uid + " ).");
                    }
                   
                }
                else if (SGD_Settings.Instance.DebugLevel >= 1)
                {
                    Debug.LogError("<ItemAction> [Node:" + Uid + "] failed to Execute(), because the item does not exist in the database (ID: "+ itemId(_runtime)+" )");
                }
            }
            else if (SGD_Settings.Instance.DebugLevel >= 1)
            {
                Debug.LogError("<ItemAction> [Node:" + Uid + "] failed to Execute(), because the entity is null");
            }
        }
 
    }
#endif
}
