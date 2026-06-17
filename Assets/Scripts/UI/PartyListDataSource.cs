using System;
using System.Collections.Generic;
using PolyAndCode.UI;
using SoftKitty;
using UnityEngine;

/// <summary>
/// Data source for the party list recyclable scroll rect.
/// Holds the entity list and wires up each cell when it's recycled.
/// </summary>
public class PartyListDataSource : IRecyclableScrollRectDataSource
{
    private List<Entity> _entities;
    private int _selectedIndex = -1;

    /// <summary>
    /// Called when a cell is clicked. index = cell index, entity = bound entity.
    /// </summary>
    public Action<int, Entity> OnCellClicked;

    /// <summary>
    /// Set the selected cell index. Triggers a reload so all visible cells can update their highlight.
    /// </summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _selectedIndex = value;
            OnCellSelected?.Invoke(value);
        }
    }

    /// <summary>
    /// Event fired when selection changes. Used by the panel to reload visible cells.
    /// </summary>
    public event Action<int> OnCellSelected;

    /// <summary>
    /// Set the entity list and refresh the scroll rect.
    /// </summary>
    public void SetEntities(List<Entity> entities)
    {
        _entities = entities;
    }

    public int GetItemCount()
    {
        return _entities != null ? _entities.Count : 0;
    }

    public void SetCell(ICell cell, int index)
    {
        var partyCell = cell as PartyCell;
        if (partyCell != null && _entities != null && index >= 0 && index < _entities.Count)
        {
            var entity = _entities[index];
            partyCell.ConfigureCell(entity, index, index == _selectedIndex);

            // Wire up the click callback
            partyCell.OnCellClicked = (idx, ent) =>
            {
                OnCellClicked?.Invoke(idx, ent);
            };
        }
    }
}
