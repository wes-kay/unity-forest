using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace SoftKitty.InventoryEngine
{
    public class ActionBarUi : MonoBehaviour
    {
        [Header("[After click a slot, will this slot be selected?]")]
        public bool SelectableSlot = true;
        [Header("[Whether display the XP progress bar.]")]
        public bool EnableXP = true;
        [Header("[Whether display the player's level on the XP progress bar.]")]
        public bool DisplayLevel = true;
        [Header("[Setup the ActionBar Data.]")]
        public List<ActionSlotSet> SlotData=new List<ActionSlotSet>();
        [Header("[Save path of the ActionBar data.]")]
        public string SavePath = "";
        [Header("[Current Action Bar index.]")]
        public int ActionSet = 0;
        [Header("When checked, the action bar will auto initialize for the player at Start().")]
        public bool AutoInitializeForPlayer = true;


        [Header("[References]")]
        public ActionSlot[] Slots;
        public Image LockIcon;
        public Text SetNumberText;
        public HintText ProgressHint;
        public Image ProgressBar;
        public Text ProgressText;

        #region Variables
        private Entity entity
        {
            get {
                return GameManager.GetEntity(entityUid);
            }
        }
        public static ActionBarUi instance;
        private bool isLock = false;
        private string ProgressBarHintString = "";
        private int SelectedSlot = 0;
        private int LastSelectedSlot = -1;
        private float xp=-1;
        private float mxp;
        private string entityUid;
        private bool inited = false;
        #endregion

        #region Internal Methods
        void Awake()
        {
            instance = this;
            SetProgressHint(ProgressBarHintString);
            ProgressBar.gameObject.SetActive(EnableXP);
            ProgressText.gameObject.SetActive(EnableXP);
        }

        IEnumerator Start()
        {
            yield return 2;
            Initialize("player",SavePath);
        }

        public void OnSlotClick(int _index, int _button)
        {
            Slots[_index].Use();
            if (_button == 0) SelectedSlot = _index;
        }

        /// <summary>
        /// Initialize the action bar with given InventoryData,EquipmentHolder and the save path.
        /// </summary>
        /// <param name="_inventoryHolder"></param>
        /// <param name="_saveFileName"></param>
        /// <param name="_equipmentHolder"></param>
        public void Initialize(string _entityUID, string _saveFileName= "ActionBar.sav")
        {
            entityUid = _entityUID;
            if (_saveFileName.Length > 0)//Check if the save path is null;
            {
                if (File.Exists(GameManager.GetFullSavePath(_saveFileName)))//Check if the save file exist
                {
                    SavePath = _saveFileName;
                    string _data = File.ReadAllText(GameManager.GetFullSavePath(_saveFileName), System.Text.Encoding.UTF8);
                    Load(_data);
                }
            }
            SwitchSet(ActionSet);
            inited = true;
        }

        public void SetChange(int _add)
        {
            if (_add > 0)
            {
                if (ActionSet + _add < SlotData.Count)
                    ActionSet += _add;
                else
                    ActionSet = 0;
                SwitchSet(ActionSet);
            }
            else if (_add < 0)
            {
                if (ActionSet + _add >= 0)
                    ActionSet += _add;
                else
                    ActionSet = SlotData.Count - 1;
                SwitchSet(ActionSet);
            }
        }

        public void ToggleLock()
        {
            isLock = !isLock;
            LockIcon.color = isLock ? new Color(1F, 0.68F, 0.09F, 1F) : new Color(0.65F, 0.61F, 0.58F, 1F);
            for (int i = 0; i < Slots.Length; i++)
            {
                Slots[i].SetLock(isLock);
            }
            SoundManager.Play2D("EquipOff");
        }


        void Update()
        {
            if (entity==null) {
                return;
            }
            if (!ItemDragManager.isVisible())
            {
                if (SelectableSlot && LastSelectedSlot != SelectedSlot)
                {
                    LastSelectedSlot = SelectedSlot;
                    for (int i = 0; i < Slots.Length; i++)
                    {
                        Slots[i].Item.ToggleOutline(i == SelectedSlot);
                    }
                }
            }
            else
            {
                LastSelectedSlot = -1;
            }
            if (EnableXP && entity != null && Time.frameCount%10==0)
            {
                if (xp != entity.GetAttributeFloat(ItemObject.instance.XpAttributeKey)
                    || mxp != entity.GetAttributeFloat(ItemObject.instance.MaxXpAttributeKey))
                {
                    xp = entity.GetAttributeFloat(ItemObject.instance.XpAttributeKey);
                    mxp = entity.GetAttributeFloat(ItemObject.instance.MaxXpAttributeKey);
                    SetProgress(xp, mxp);
                    SetProgressHint("Level. "+ entity.GetAttributeInt(ItemObject.instance.LevelAttributeKey).ToString());
                    if (DisplayLevel) ProgressText.text += "  ( Level." + entity.GetAttributeInt(ItemObject.instance.LevelAttributeKey).ToString() + " )";
                }
            }
        }
        #endregion

        public void Load(string _json) // Load data with json string
        {
            ActionBarSaveRoot _saveRoot = JsonUtility.FromJson<ActionBarSaveRoot>(_json);
            SlotData.Clear();
            for (int i = 0; i < _saveRoot.sets.Length; i++)
            {
                ActionSlotSet _newSet = new ActionSlotSet();
                _newSet.slots = new List<ActionSlotData>();
                for (int u = 0; u < _saveRoot.sets[i].slots.Length; u++)
                {
                    ActionSlotData _newSlot = new ActionSlotData();
                    _newSlot.key = (KeyCode)_saveRoot.sets[i].slots[u].key;
                    _newSlot.itemId = _saveRoot.sets[i].slots[u].itemId;
                    _newSlot.upgradeLevel = _saveRoot.sets[i].slots[u].upgradeLevel;
                    _newSlot.enchantments = new List<int>();
                    _newSlot.enchantments.AddRange(_saveRoot.sets[i].slots[u].enchantments);
                    _newSlot.sockets = new List<int>();
                    _newSlot.sockets.AddRange(_saveRoot.sets[i].slots[u].sockets);
                    _newSlot.holderType = _saveRoot.sets[i].slots[u].holderType;
                    _newSet.slots.Add(_newSlot);
                }
                SlotData.Add(_newSet);
            }
        }
        public void Save()//Save data to the SavePath.
        {
            if (!inited) return;
            if (SavePath.Length > 0)
            {
                ActionBarSaveRoot _saveRoot = new ActionBarSaveRoot();
                _saveRoot.sets = new ActionBarSave[SlotData.Count];
                for (int i = 0; i < SlotData.Count; i++)
                {
                    _saveRoot.sets[i] = new ActionBarSave();
                    _saveRoot.sets[i].slots = new ActionBarSlotSave[SlotData[i].slots.Count];
                    for (int u = 0; u < SlotData[i].slots.Count; u++)
                    {
                        _saveRoot.sets[i].slots[u] = new ActionBarSlotSave();
                        _saveRoot.sets[i].slots[u].key = (int)SlotData[i].slots[u].key;
                        _saveRoot.sets[i].slots[u].itemId = SlotData[i].slots[u].itemId;
                        _saveRoot.sets[i].slots[u].upgradeLevel = SlotData[i].slots[u].upgradeLevel;
                        _saveRoot.sets[i].slots[u].enchantments.Clear();
                        _saveRoot.sets[i].slots[u].enchantments.AddRange(SlotData[i].slots[u].enchantments);
                        _saveRoot.sets[i].slots[u].sockets.Clear();
                        _saveRoot.sets[i].slots[u].sockets.AddRange(SlotData[i].slots[u].sockets);
                        _saveRoot.sets[i].slots[u].holderType = SlotData[i].slots[u].holderType;
                    }
                }
                string _json = JsonUtility.ToJson(_saveRoot);
                File.WriteAllText(GameManager.GetFullSavePath(SavePath), _json,System.Text.Encoding.UTF8);
            }
        }
        public void SetSelectedSlot(int _index) //Set a slot to be selected
        {
            SelectedSlot= _index;
        }
        public int GetSelectedSlot()//Get the slot which set to be selected.
        {
            return SelectedSlot;
        }
        public void UseSelectedSlot()//Use the item in the slot which set to be selected.
        {
            OnSlotClick(SelectedSlot, 0);
        }
        public void SetProgressHint(string _text)//Set the hint text of the progress bar.
        {
            ProgressBarHintString = _text;
            ProgressHint.HintString = ProgressBarHintString;
        }
        public void SetProgress(float _value, float _maxium)//Set the progress bar progress value and maxmium value
        {
            ProgressBar.transform.localScale = new Vector3(Mathf.Clamp01(_value /Mathf.Max(0.0001F,_maxium)),1F,1F);
            ProgressText.text = Mathf.FloorToInt(_value).ToString() + " / " + Mathf.FloorToInt(_maxium).ToString();
        }
        public KeyCode GetAssignedKey(int _index, int _actionBarIndex=-1) //Get the assigned KeyCode by slot index and action bar index.
        {
            return SlotData[_actionBarIndex == -1 ? ActionSet : _actionBarIndex].slots[_index].key;
        }
        public void AssignKey(KeyCode _key, int _index, int _actionBarIndex = -1)//Assign key by slot index and action bar index
        {
            SlotData[_actionBarIndex == -1 ? ActionSet : _actionBarIndex].slots[_index].key= _key;
            if(_actionBarIndex == ActionSet || _actionBarIndex == -1) Slots[_index].UpdateKey();
            Save();
        }
        public bool isKeyAssigned(KeyCode _key, int _actionBarIndex = -1)//Check if a key is already assigned within an action bar.
        {
            for (int i=0;i< SlotData[_actionBarIndex == -1 ? ActionSet : _actionBarIndex].slots.Count;i++) {
                if (SlotData[_actionBarIndex == -1 ? ActionSet : _actionBarIndex].slots[i].key == _key) return true;
            }
            return false;
        }
        public void SwapKey(int _indexA,int _indexB, int _actionBarIndex = -1)//Swap keys of two slots within an action bar.
        {
            KeyCode _keyA = GetAssignedKey(_indexA, _actionBarIndex);
            KeyCode _keyB = GetAssignedKey(_indexB, _actionBarIndex);
            AssignKey(_keyB, _indexA, _actionBarIndex);
            AssignKey(_keyA, _indexB, _actionBarIndex);
        }
        public void SwitchSet(int _actionBarIndex)//Switch the action bar.
        {
            SoundManager.Play2D("paper");
            ActionSet = _actionBarIndex;
            SetNumberText.text = (ActionSet+1).ToString();
            for (int i = 0; i < Slots.Length; i++)
            {
                Slots[i].Item.RegisterClickCallback(i, OnSlotClick);
                Slots[i].Item.Outline.color = ItemObject.instance.ItemSelectedColor;
                Slots[i].Item.Fav.GetComponent<Image>().color = ItemObject.instance.FavoriteColor;
                if (i < SlotData[ActionSet].slots.Count)
                {
                    Slots[i].gameObject.SetActive(true);
                    var _holder = entity.GetModule<InventoryModule>().GetInventoryDataByType(SlotData[ActionSet].slots[i].holderType);
                    if (_holder == null) _holder = entity.GetModule<InventoryModule>().GetAnyInventoryData();
                     Slots[i].Initialize(i,SlotData[ActionSet].slots[i], _holder);
                }
                else
                {
                    Slots[i].gameObject.SetActive(false);
                }
            }
        }

       

    }
}
