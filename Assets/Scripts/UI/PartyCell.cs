using System;
using CharacterAttributes;
using PolyAndCode.UI;
using SoftKitty;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A single row in the party list. Plain UI panel — no Button component.
/// Click handling is done via IPointerClickHandler.
/// </summary>
public class PartyCell : MonoBehaviour, ICell, IPointerClickHandler
{
    [Header("UI Elements")]
    public TMP_Text nameText;
    public TMP_Text vitalityText;
    public Image background;

    private Entity _entity;
    private int _index;

    /// <summary>
    /// Invoked when this cell is clicked.
    /// </summary>
    public Action<int, Entity> OnCellClicked;

    /// <summary>
    /// Called by the data source when a cell is recycled.
    /// Binds the given entity and updates the UI.
    /// </summary>
    public void ConfigureCell(Entity entity, int index, bool selected)
    {
        _entity = entity;
        _index = index;

        if (nameText != null)
        {
            string name = entity.GetAttributeString(AttributeKey.Name.GetUid());
            nameText.text = string.IsNullOrEmpty(name) ? entity.uid : name;
        }

        if (vitalityText != null)
        {
            float vitality = _entity.GetAttributeFloat("cvit");
            float maxVitality = _entity.GetAttributeFloat("mvit");
            vitalityText.text = vitality.ToString("F0") + " / " + maxVitality.ToString("F0");
        }

        SetSelected(selected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCellClicked?.Invoke(_index, _entity);
    }

    /// <summary>
    /// Highlight or unhighlight the cell row.
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (background != null)
        {
            background.color = selected ? new Color(0.75f, 0.15f, 0.15f, 0.4f) : new Color(0f, 0f, 0f, 0.3f);
        }
    }
}
