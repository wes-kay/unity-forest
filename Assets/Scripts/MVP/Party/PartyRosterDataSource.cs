using System;
using System.Collections.Generic;
using PolyAndCode.UI;
using SoftKitty;
using UnityEngine;

namespace Domain.MVP.Party
{
    /// <summary>
    /// Data source for the party roster RecyclableScrollRect.
    /// Holds the entity list and wires up each cell when it's recycled.
    /// </summary>
    public class PartyRosterDataSource : IRecyclableScrollRectDataSource
    {
        private List<Entity> _entities;
        private int _selectedIndex = -1;

        /// <summary>
        /// Called when a cell is clicked. index = cell index, entity = bound entity.
        /// </summary>
        public Action<int, Entity> OnCellClicked;

        /// <summary>
        /// Called when the selected index changes (for updating visible cell highlights).
        /// </summary>
        public Action OnSelectionChanged;

        /// <summary>
        /// Selected cell index. Triggers reload when changed.
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                OnSelectionChanged?.Invoke();
            }
        }

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

        /// <summary>
        /// Get the entity at the given index.
        /// </summary>
        public Entity GetEntityAt(int index)
        {
            if (_entities != null && index >= 0 && index < _entities.Count)
                return _entities[index];
            return null;
        }

        public void SetCell(ICell cell, int index)
        {
            var partyCell = cell as PartyCell;
            if (partyCell == null || _entities == null || index < 0 || index >= _entities.Count)
                return;

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
