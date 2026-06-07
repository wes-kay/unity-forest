using UnityEngine;
using SoftKitty.InventoryEngine;

namespace SoftKitty
{
#if MASTER_COMBAT_CORE
    public class ItemHandler:Handler{
        public Entity entity(GraphRuntimeState _runtime) { //The entity to query.
            return GetEntity(inputPort[0].connectedNodes[0],_runtime);
        }
        public enum inventoryTypes{
          All,
          Inventory,
          Equipment
        }
        public inventoryTypes inventoryType; //The type of inventory to query.
        public bool hidden; //Whether including the hidden stacks.
        public float itemId(GraphRuntimeState _runtime) { //The unique integer id of the item.
            return GetVariableValue(inputPort[1].connectedNodes[0],_runtime);
        }
        public enum logics{
          Greater,
          Greater_or_Equal,
          Equal,
          Less_or_Equal,
          Less,
          Unequal,
          Nonexist
        }
        public logics logic; //The method of comparison
        public float value(GraphRuntimeState _runtime) { //The number to compare the count of the item with.
            return GetVariableValue(inputPort[2].connectedNodes[0],_runtime);
        }
 
        public ItemHandler(){}
 
        public ItemHandler(Handler _baseClass) : base(_baseClass){}
 
        public ItemHandler InitHandler(Handler _baseClass){
            ItemHandler  _newObj =new ItemHandler(_baseClass);
            _newObj.inventoryType = (inventoryTypes)_baseClass.IntValues[0];
            _newObj.hidden = _baseClass.BoolValues[0];
            _newObj.logic = (logics)_baseClass.IntValues[1];
            return _newObj;
        }

        int _id;
        Item _item;
        InventoryData _inventory;
        int _number;
        bool _result;
        public override void Update(GraphRuntimeState _runtime){ //Update state of the handler
            if (entity(_runtime) == null)
            {
                if (SGD_Settings.Instance.DebugLevel >= 1)
                {
                    Debug.LogError("<ItemHandler> [Node:" + Uid + "] failed to Update(), because the entity is null");
                }
                return;
            }
            _id = -1;
            _item = null;
            _inventory = null;
            _number = 0;
            _result = false;
            _id = Mathf.FloorToInt(itemId(_runtime));
            _item = ItemObject.instance.GetItem(_id);
            if (_item == null)
            {
                if (SGD_Settings.Instance.DebugLevel >= 1)
                {
                    Debug.LogError("<ItemHandler> [Node:" + Uid + "] failed to Update(), because the item( ID:" + _id + ") is null");
                }
                return;
            }
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
            if (_inventory == null)
            {
                if(SGD_Settings.Instance.DebugLevel >= 1)
                {
                    Debug.LogError("<ItemHandler> [Node:" + Uid + "] failed to Update(), because could not found desired type of inventory on Entity(UID: " + entity(_runtime).uid + " ).");
                }
                return;
            }
            _number = _inventory.GetItemNumber(_id, hidden);
            switch (logic)
            {
                case logics.Greater:
                    _result=(_number > Mathf.FloorToInt(value(_runtime)));
                    break;
                case logics.Greater_or_Equal:
                    _result = (_number >= Mathf.FloorToInt(value(_runtime)));
                    break;
                case logics.Equal:
                    _result = (_number == Mathf.FloorToInt(value(_runtime)));
                    break;
                case logics.Less_or_Equal:
                    _result = (_number <= Mathf.FloorToInt(value(_runtime)));
                    break;
                case logics.Less:
                    _result = (_number < Mathf.FloorToInt(value(_runtime)));
                    break;
                case logics.Unequal:
                    _result = (_number != Mathf.FloorToInt(value(_runtime)));
                    break;
                default:
                    _result = (_number <=0);
                    break;
            }
            SetActive(_runtime, _result);
            if (SGD_Settings.Instance.DebugLevel >= 3) Debug.Log("<ItemHandler> [Node:" + Uid + "] -> Condition Update( Entity[ UID: " + entity(_runtime).uid+" ] has "+ _number+" item( "+ _item.name+" ), which is " + logic + " than " + Mathf.FloorToInt(value(_runtime)) + ") > " + _result);
        }
 
    }
#endif
}
