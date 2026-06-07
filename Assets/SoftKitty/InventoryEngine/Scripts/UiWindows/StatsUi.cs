using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SoftKitty.InventoryEngine
{
    /// <summary>
    /// This module displays the total attributes of an InventoryData. 
    /// Attach this script to a UI panel, assign the necessary UI elements,
    /// and call Initialize (InventoryData _equipHolder, Attribute[] _baseAttributes) to display the stats. 
    /// _baseAttributes represents the player's attributes without any equipment.
    /// </summary>
    public class StatsUi : MonoBehaviour
    {
        #region Variables
        public GameObject StatPrefab;
        public string entity_UiD="player";
        public Entity entity
        {
            get
            {
                return GameManager.GetEntity(entity_UiD);
            }
        }
        private Dictionary<string, ListItem> StatList=new Dictionary<string, ListItem>();
        private Color StatColor;
        private bool Inited = false;
        #endregion

        #region MonoBehaviour
        private void Update()
        {
            if (entity == null || !Inited) return;

            for (int i = 0; i < GameManager.AttributeData.AttributeList.Count; i++)
            {
                if (GameManager.AttributeData.AttributeList[i].visibleInStatsPanel && GameManager.AttributeData.AttributeList[i].isNumber())
                {
                    float _currentValue = GetValue(GameManager.AttributeData.AttributeList[i].uid);
                    if (StatList[GameManager.AttributeData.AttributeList[i].uid].mValue != _currentValue)
                    {
                        if (StatList[GameManager.AttributeData.AttributeList[i].uid].mValue < _currentValue)
                        {
                            StartCoroutine(PopStat(StatList[GameManager.AttributeData.AttributeList[i].uid].mTexts[1],Color.green));
                        }
                        else
                        {
                            StartCoroutine(PopStat(StatList[GameManager.AttributeData.AttributeList[i].uid].mTexts[1], Color.red));
                        }
                        StatList[GameManager.AttributeData.AttributeList[i].uid].mValue = _currentValue;
                        StatList[GameManager.AttributeData.AttributeList[i].uid].mTexts[1].text = (Mathf.RoundToInt( _currentValue*10)*0.1F).ToString()+ GameManager.AttributeData.AttributeList[i].suffixes;
                    }
                }
            }
        }
        #endregion

        #region Internal Methods

        IEnumerator PopStat(Text _stat, Color _popColor)
        {
            float t = 0F;
            while (t<1F) {
                t += Time.unscaledDeltaTime * 10F;
                _stat.color = Color.Lerp(StatColor, _popColor,t);
                _stat.transform.localScale = Vector3.one * (1F + t * 0.3F);
                yield return 1;
            }
            yield return new WaitForSecondsRealtime(0.5F);
            while (t > 0F)
            {
                t -= Time.unscaledDeltaTime*3F;
                _stat.color = Color.Lerp(StatColor, _popColor, t);
                _stat.transform.localScale = Vector3.one * (1F + t * 0.3F);
                yield return 1;
            }
            _stat.color = StatColor;
            _stat.transform.localScale = Vector3.one;
        }
        private float GetValue(string _key)
        {
            return entity.GetAttributeFloat(_key);
        }
        #endregion

        /// <summary>
        /// Initialize to display the stats._baseAttributes represents the player's attributes without any equipment.
        /// </summary>
        /// <param name="_equipHolder"></param>
        /// <param name="_baseAttributes"></param>
        public void Init(string _entityUID)
        {
            StatColor = StatPrefab.GetComponent<ListItem>().mTexts[1].color;
            entity_UiD = _entityUID;
            for(int i=0;i< GameManager.AttributeData.AttributeList.Count;i++) {
                if (GameManager.AttributeData.AttributeList[i].visibleInStatsPanel && GameManager.AttributeData.AttributeList[i].isNumber() && !StatList.ContainsKey(GameManager.AttributeData.AttributeList[i].uid))
                {
                    GameObject _newStat = Instantiate(StatPrefab, StatPrefab.transform.parent);
                    _newStat.transform.localScale = Vector3.one;
                    _newStat.GetComponent<ListItem>().mTexts[0].text = GameManager.AttributeData.AttributeList[i].name+" : ";
                    _newStat.SetActive(true);
                    _newStat.GetComponent<ListItem>().mValue = GetValue(GameManager.AttributeData.AttributeList[i].uid);
                    _newStat.GetComponent<ListItem>().mTexts[1].text = (Mathf.RoundToInt(GetValue(GameManager.AttributeData.AttributeList[i].uid) * 10) * 0.1F).ToString()+ GameManager.AttributeData.AttributeList[i].suffixes;
                    StatList.Add(GameManager.AttributeData.AttributeList[i].uid, _newStat.GetComponent<ListItem>());
                }
            }
                Inited = true;
        }
       


    }
}
