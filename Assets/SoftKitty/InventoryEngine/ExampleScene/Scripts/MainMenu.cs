using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace SoftKitty.InventoryEngine
{
    /// <summary>
    /// =======================VERY IMPORTANT!!!!!!!!=========================
    /// This script is an example to show how to use InventoryEngine. 
    /// Please do not directly use this script in your project.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        public bool AutoSave = true;

        #region References
        public EntityComponent Monster;
        public EntityComponent MerchantNpc;
        public EntityComponent StorageContainer;
        public EntityComponent NpcEquipment;
        public EntityComponent NpcWorker;
        public ListItem DebugItem;
        public AdvAnimation EnemyAnimation;
        public Image EnemyHpBar;
        public Text EnemyHpBarText;
        public Text EnemyNameText;
        public GameObject EnemyDynamicText;
        public AdvAnimation EnemyAttackAnimation;
        public AdvAnimation EnemyStunAnimation;
        public AdvAnimation PlayerAnimation;
        public Image PlayerHpBar;
        public Image PlayerSpBar;
        public Text PlayerHpText;
        public Text PlayerSpText;
        public GameObject PlayerDynamicText;
        private float SkyBoxRot = 21F;
        private bool EnemyAttacking = false;
        #endregion

#if MASTER_INVENTORY_ENGINE
        #region Visual Effects


        private void Update()
        {
            //Make the background looks more alive.
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetFloat("_Exposure", 1.15F + Mathf.Sin(Time.unscaledTime * 0.7F) * 0.15F);
                RenderSettings.skybox.SetFloat("_Rotation", SkyBoxRot);
                SkyBoxRot = Mathf.Lerp(SkyBoxRot, 21F + (Screen.width * 0.5F - InputProxy.mousePosition.x) * 0.003F, Time.unscaledDeltaTime);
            }
            PlayerStateUpdate();
            EnemyStateUpdate();
            Time.timeScale = 0F;
            
        }
        #endregion

        
        IEnumerator Start()
        {
            if (AutoSave)
            {
                if (File.Exists(GameManager.GetFullSavePath("DemoSave.sav")))
                {
                    GameManager.EntityManagerData.Load(GameManager.GetFullSavePath("DemoSave.sav"));///Load Saved Data
                }
            }
            yield return 1;
            /// Callback examples ======================================================================
            ItemObject.PlayerInventoryData.RegisterItemUseCallback(OnItemUse);//Register callback to trigger when player's item in inventory being used.
            ItemObject.PlayerEquipmentData.RegisterItemUseCallback(OnEquippedItemUse);//Register callback to trigger when player's item in equipment being used.
            ItemObject.PlayerEquipmentData.RegisterItemChangeCallback(OnEuqipmentItemChange);//Register callback for player's equipment
            ItemObject.PlayerInventoryData.RegisterItemDropCallback(OnItemDrop);//Register callback for player drop item from inventory
            ItemObject.PlayerEquipmentData.RegisterItemDropCallback(OnItemDrop);//Register callback for player drop item from equipment
            ////////////////////////////////////////////////////
            float _chp = GameManager.GetPlayer().GetAttributeFloat("chp"); //Get the current health value of player.
            float _mhp = GameManager.GetPlayer().GetAttributeFloat("hp");//Get the maximum health value of player.
            PlayerHpBar.rectTransform.sizeDelta = new Vector2(390F * _chp / _mhp, 40F);
            PlayerHpText.text = Mathf.RoundToInt(_chp).ToString() + "/" + Mathf.RoundToInt(_mhp).ToString();
            //SetMonsterLevel(1);

            NpcWorker.GetModule<InventoryModule>().GetInventory().RegisterCraftingStateCallback(OnCraftingStateChange);
        }

        public void OnCraftingStateChange(CraftingState _state, float _remainTime)
        {
            Debug.Log(_state+" > "+ _remainTime);
        }

        private void OnDestroy() // If you ever called RegisterXXXXCallback(), don't forget to call UnRegisterXXXXCallback() when the gameobject going to be destroyed.
        {
            NpcWorker.GetModule<InventoryModule>().GetInventory().UnRegisterCraftingStateCallback(OnCraftingStateChange);
            ItemObject.PlayerInventoryData.UnRegisterItemUseCallback(OnItemUse);
            ItemObject.PlayerEquipmentData.UnRegisterItemUseCallback(OnEquippedItemUse);
            ItemObject.PlayerEquipmentData.UnRegisterItemChangeCallback(OnEuqipmentItemChange);
            ItemObject.PlayerInventoryData.UnRegisterItemDropCallback(OnItemDrop);
            ItemObject.PlayerEquipmentData.UnRegisterItemDropCallback(OnItemDrop);
            if (AutoSave)
            {
                GameManager.EntityManagerData.Save(GameManager.GetFullSavePath("DemoSave.sav")); //Auto Save
            }
        }

        public void OnItemDrop(Item _droppedItem, int _number)
        {
            //Add your code here to Instantiate the item model on the ground.
            DynamicMsg.PopMsg("Player dropped" + " [" + _droppedItem.nameWithAffixing + "] x "+_number.ToString());
        }
       

#if MASTER_CHARACTER_CREATOR
        public MasterCharacterCreator.CharacterEntity Player; //If you have Master Character Creator installed, drag the player entity to this slot.
#endif
        public void OnEuqipmentItemChange(Dictionary<Item, int> _changedItems)
        {
            foreach (var _item in _changedItems.Keys) {
#if MASTER_CHARACTER_CREATOR
                if (Player != null)
                {
                    if (_changedItems[_item] > 0)
                    {
                        Player.Equip(_item.equipAppearance); // Master Character Creator Equip
                    }
                    else
                    {
                        Player.Unequip(_item.equipAppearance.Type); // Master Character Creator Unequip
                    }
                }
#endif

                if (!_item.consumable) DynamicMsg.PopMsg("Player "+ (_changedItems[_item]>0?"equipped":"unequipped")+" ["+ _item.nameWithAffixing + "]");
            }
        }

        public void OnEquippedItemUse(string _action, int _id, int _index)
        {
            bool _displayIcon = false;
            if (_action.Contains("Shuriken"))//Attack
            {
                AttackEnemy(ItemObject.instance.TryGetItem(_id).GetAttributeFloat("atk"));
                SoundManager.Play2D("Attack");
                _displayIcon = true;
            }

            if (_displayIcon)
            {
                DebugItem.transform.parent.SetAsLastSibling();
                GameObject _newItem = Instantiate(DebugItem.gameObject, DebugItem.transform.parent);
                _newItem.GetComponent<ListItem>().mImages[0].color = ItemObject.instance.TryGetItem(_id).GetTypeColor();
                _newItem.GetComponent<ListItem>().mRawimages[0].texture = ItemObject.instance.TryGetItem(_id).GetIcon();
                _newItem.GetComponent<ListItem>().mTexts[0].text = _action;
                _newItem.gameObject.SetActive(true);
            }
        }

        public void OnItemUse(string _action, int _id, int _index) //When player using an item, this callback will be called.
        {
            if (_action == "equip")
            {
                InventoryData.EquipItem(ItemObject.PlayerInventoryData, ItemObject.PlayerEquipmentData, _id, _index);//Equip item when click though action bar.
                return;
            }

            bool _displayIcon = false;
            if (_action.Contains("AddHp"))//change hp
            {
                PlayerHealthChange(ItemObject.instance.TryGetItem(_id).GetAttributeFloat("chp"));
                SoundManager.Play2D("UseItem");
                _displayIcon = true;
            }
            else if (_action.Contains("AddSp"))//change sp
            {
                GameManager.GetPlayer().AddAttributeValueClamp("sp", ItemObject.instance.TryGetItem(_id).GetAttributeFloat("sp"),0F,100F);
                SoundManager.Play2D("UseItem");
                _displayIcon = true;
            }
            else if (_action.Contains("ChangeBaseStats"))//change other stats
            {
                foreach (var obj in ItemObject.instance.TryGetItem(_id).attributes)
                {
                    float _final= GameManager.GetPlayer().AddAttributeValue(obj.uid, obj.GetFloat());
                    Debug.Log("Added "+ obj.uid+": "+ obj.GetFloat()+" > "+ _final);
                }
                SoundManager.Play2D("UseItem");
                _displayIcon = true;
            }
            else if (_action.Contains("Skill"))//Use Skill
            {
                GameManager.GetPlayer().AddAttributeValueClamp("sp", -ItemObject.instance.TryGetItem(_id).GetAttributeFloat("spcost"),0F,100F);//Reduce sp with "Sp Cost" arrtibute value of the skill item.
                float _damage = ItemObject.instance.TryGetItem(_id).GetAttributeFloat("dmg");//Get the "Damage" attribute value from the skill item
                float _attack = GameManager.GetPlayer().GetAttributeFloat("atk"); //Get player's "Attack" attribute value.
                bool _critical = (Random.Range(0, 100) < GameManager.GetPlayer().GetAttributeFloat("crit", true));//Use player's "Critical Chance" attribute value to check if this should be a critical strike.
                bool _stun = (Random.Range(0, 100) < GameManager.GetPlayer().GetAttributeFloat("stun", true));//Use player's "Stun Chance" attribute value to check if this should stun the enemy.
                DamageEnemy(_attack + _damage, _critical,_stun);
                SoundManager.Play2D("Attack");
                _displayIcon = true;
            } 
           


            //Create item icon in the center of the screen to show how the callback works.
            if (_displayIcon)
            {
                DebugItem.transform.parent.SetAsLastSibling();
                GameObject _newItem = Instantiate(DebugItem.gameObject, DebugItem.transform.parent);
                _newItem.GetComponent<ListItem>().mImages[0].color = ItemObject.instance.TryGetItem(_id).GetTypeColor();
                _newItem.GetComponent<ListItem>().mRawimages[0].texture = ItemObject.instance.TryGetItem(_id).GetIcon();
                _newItem.GetComponent<ListItem>().mTexts[0].text = _action;
                _newItem.gameObject.SetActive(true);
            }
        }
        /// End of the Callback examples ======================================================================


        ///Game Play examples =======================================================================

        private void PlayerStateUpdate()//Called by Update()
        {
            float _playerSp = GameManager.GetPlayer().AddAttributeValueClamp("sp", Time.unscaledDeltaTime * 2F,0F,100F);//Recorver 2 sp per second; We do not want to save this value, so we mark '_save' as false.
            PlayerSpBar.rectTransform.sizeDelta = new Vector2(390F * _playerSp / 100F, 25F);
            PlayerSpText.text = Mathf.RoundToInt(_playerSp).ToString();
            float _chp = GameManager.GetPlayer().GetAttributeFloat("chp");//Get the value of player's "Current HP" attribute.
            float _mhp = GameManager.GetPlayer().GetAttributeFloat("hp");//Get the value of player's "Health" attribute, this will be the maximum health value of the player.
            _chp = Mathf.Clamp(_chp, 0F, _mhp);
            PlayerHpBar.rectTransform.sizeDelta = new Vector2(390F * _chp / _mhp, 40F);
            PlayerHpText.text = Mathf.RoundToInt(_chp).ToString() + "/" + Mathf.RoundToInt(_mhp).ToString();
        }

        private void EnemyStateUpdate()//Called by Update()
        {
            EnemyHpBar.rectTransform.sizeDelta = new Vector2(300F * Monster.GetAttributeInt("chp") / Monster.GetAttributeInt("hp"), 25F);
            EnemyHpBarText.text = Monster.GetAttributeInt("chp").ToString() + "/" + Monster.GetAttributeInt("hp").ToString();
        }

        public void AttackEnemy(float _additionalAttack=0F)//When click on the enemy
        {
            float _damage = GameManager.GetPlayer().GetAttributeFloat("atk")+ _additionalAttack; //Use player's "Attack" attribute value as the damage value.
            bool _critical = (Random.Range(0, 100) < GameManager.GetPlayer().GetAttributeFloat("crit"));//Use player's "Critical Chance" attribute value to check if this should be a critical strike.
            bool _stun= (Random.Range(0, 100) < GameManager.GetPlayer().GetAttributeFloat("stun"));//Use player's "Stun Chance" attribute value to check if this should stun the enemy.
            DamageEnemy(_damage, _critical, _stun);
        }

        private void DamageEnemy(float _damage, bool _critical, bool _stun)
        {
            if (_critical) _damage *= 2;
            Monster.AddAttributeValueClamp("chp", -Mathf.CeilToInt(_damage), 0, Monster.GetAttributeInt("hp"));

            //Visual Effects
            SoundManager.Play2D("Kill");
            EnemyAnimation.Stop();
            EnemyAnimation.Play("DamageEffect");
            if (_stun)
            {
                EnemyStunAnimation.Stop();
                EnemyStunAnimation.Play();
                if (EnemyAttacking)
                {
                    StopAllCoroutines();
                    EnemyAttacking = false;
                }
            }
            GameObject _newDamagePop = Instantiate(EnemyDynamicText, EnemyDynamicText.transform.parent);//Damage text pop
            _newDamagePop.GetComponent<Text>().text = Mathf.CeilToInt(_damage).ToString();
            if (_critical)//Critical stike visual effect
            {
                _newDamagePop.GetComponent<Text>().fontSize = 80;
                _newDamagePop.GetComponent<Text>().fontStyle = FontStyle.Bold;
                _newDamagePop.GetComponent<Text>().color = new Color(1F, 0.7F, 0F, 1F);
                _newDamagePop.GetComponent<Text>().text += " !";
            }
            _newDamagePop.gameObject.SetActive(true);
            //End of Visual Effects

            if (Monster.GetAttributeInt("chp") <= 0) //If enemy gets killed
            {
                if (EnemyAttacking)
                {
                    StopAllCoroutines();
                    EnemyAttacking = false;
                }
                var _loot = Monster.GetModule<InventoryModule>().DropLootPack(); //Drop a loot pack.
                _loot.OpenPack();//Auto open the loot pack;
                EnemyAnimation.Play("EnemySpawn");//Respawn the enemy.
                int _enemyLevel = Monster.GetAttributeInt("lvl") + 1;
                SetMonsterLevel(_enemyLevel);//Level up the enemy
                int xp = GameManager.GetPlayer().AddAttributeValue("xp", 20+10* _enemyLevel);//Add xp to the "XP" attrubite of player.
                if (xp >= GameManager.GetPlayer().GetAttributeInt("mxp"))//If the "XP" attribute value is larger than "Max XP", then add 1 to the "Level" attribute value.
                { //Level up!
                    int _level = GameManager.GetPlayer().GetAttributeInt("lvl");
                    SoundManager.Play2D("CraftSuccess");
                    SetPlayerLevel(Mathf.FloorToInt(_level)+1);//Add 1 to the "Level"
                }
            }
            else //Enemy fight back
            {
                if (!EnemyAttacking)
                {
                    StartCoroutine(EnemyAttackCo(Random.Range(0.2F,1F)+(_stun?3F:0F)));
                }
            }
        }

        IEnumerator EnemyAttackCo(float _wait)
        {
            EnemyAttacking = true;
            yield return new WaitForSecondsRealtime(_wait);
            EnemyAttackAnimation.Stop();
            EnemyAttackAnimation.Play();
            yield return new WaitForSecondsRealtime(0.5F);
            PlayerAnimation.Play();
            SoundManager.Play2D("Attack");
            float _damage = Monster.GetAttributeFloat("atk");//Get monster's attack
            float _def = GameManager.GetPlayer().GetAttributeFloat("def");//Player's Defence attribute value
            float _agi= GameManager.GetPlayer().GetAttributeFloat("agi");//Player's Agility attribute value
            float _finalDamage= _damage * (1F-_def/(200+_def))*(Random.Range(0,100)<_agi?0F:1F);//Final damage formular
            if (_finalDamage > 0F)
            {
                PlayerHealthChange(-_finalDamage);//Change player's health value
            }
            else //parry
            {
                GameObject _newDamagePop = Instantiate(PlayerDynamicText, PlayerDynamicText.transform.parent);//Damage text pop
                _newDamagePop.GetComponent<Text>().text = "Parry!";
                _newDamagePop.gameObject.SetActive(true);
            }
            yield return new WaitForSecondsRealtime(0.5F);
            EnemyAttacking = false;
        }


        private void PlayerHealthChange(float _add)
        {
            float _chp= GameManager.GetPlayer().AddAttributeValueClamp("chp", _add, 0F, GameManager.GetPlayer().GetAttributeFloat("hp"));//Add  value to player's "Current HP" attribute. Negative value will be damage, postive value will be heal.

            GameObject _newDamagePop = Instantiate(PlayerDynamicText, PlayerDynamicText.transform.parent);//Damage text pop
            if (_add < 0F)//Damage
            {
                _newDamagePop.GetComponent<Text>().text = Mathf.CeilToInt(-_add).ToString();
            }
            else//Heal
            {
                _newDamagePop.GetComponent<Text>().text ="+"+ Mathf.CeilToInt(_add).ToString();
                _newDamagePop.GetComponent<Text>().color = new Color(0.1F,1F,0.4F,1F);
            }
            _newDamagePop.gameObject.SetActive(true);

            if (_chp <= 0F)//Player dead
            {
                SoundManager.Play2D("CraftFail");
                PlayerAnimation.Stop();
                PlayerAnimation.Play("PlayerDead");//Respawn the player.
                EnemyAnimation.Stop();
                EnemyAnimation.Play("EnemySpawn");//Respawn the enemy.
                //Reset player and monster's level to 1
                SetPlayerLevel(1);
                SetMonsterLevel(1);
            }
        }

        private void SetPlayerLevel(int _level)
        {
            GameManager.GetPlayer().AttributesUpgradeLevel = _level;
            GameManager.GetPlayer().SetAttributeValue("xp", 0); //Reset XP to 0;
            GameManager.GetPlayer().SetAttributeValue("lvl", _level);//Set the "Level" attribute.
            GameManager.GetPlayer().SetAttributeValue("chp", GameManager.GetPlayer().GetAttributeFloat("hp"));//refill the current health value.
        }

        private void SetMonsterLevel(int _level)
        {
            Monster.mData.AttributesUpgradeLevel = _level;
            Monster.SetAttributeValue("lvl", _level);
            Monster.SetAttributeValue("chp", Monster.GetAttributeInt("hp"));
            EnemyNameText.text = "Monster Lvl." + _level.ToString();
        }


        ///End of Game Play examples =======================================================================

        /// Open window examples ===================================================================

        public void OnWindowClose(InventoryData _key)
        {
            Debug.Log("Player window close");
        }


        public void OpenPlayerInventory()
        {
            if (ItemObject.PlayerInventoryData != null)
            {
                UiWindow _window= ItemObject.PlayerInventoryData.OpenWindow();//Open Inventory window for player.
                if (_window != null) _window.RegisterCloseCallback(OnWindowClose, ItemObject.PlayerInventoryData);
            }
        }
        public void OpenNpcWorker()
        {
            NpcWorker.GetModule<InventoryModule>().GetInventory().OpenForgeWindow(true,false,false,false,2F); //Open the crafting window to assign jobs to NPC or Crafting Table.
        }

        public void OpenPlayerEquipment()
        {
            if (ItemObject.PlayerEquipmentData != null) ItemObject.PlayerEquipmentData.OpenWindow(); //Open Equipment window for player.
        }

        public void OpenNpcTeamEquipment()
        {
            NpcEquipment.GetModule<InventoryModule>().GetInventoryDataByType(InventoryData.HolderType.NpcEquipment).OpenWindow();//Open the equipment window of your ally NPC.
        }

        public void OpenCraft()
        {
            ItemObject.PlayerInventoryData.OpenForgeWindow(true, true, true, true, 1F,"Forge"); //Open crafting window
        }

        public void KillMonster()
        {
            SoundManager.Play2D("Kill");
            var _loot = ItemObject.DropLootPack(Vector3.zero, "MenuLootPack");//Drop a loot pack.
            _loot.OpenPack();//Auto open the loot pack;
        }

        public void OpenSkills()
        {
            ItemObject.PlayerInventoryData.OpenWindowByName("Skills","Skills"); //An example to use "Hidden Items" to achive "Skills" management.
        }
        public void TalkToMerchant()
        {
            MerchantNpc.GetModule<InventoryModule>().GetInventory().OpenWindow();//Open the merchant trade window.
            SoundManager.Play2D("Merchant");
        }

        public void OpenStorage()
        {
            StorageContainer.GetModule<InventoryModule>().GetInventory().OpenWindow();//Open the crate window.
        }
        /// End of the Open window examples ===================================================================
#endif
    }
}
