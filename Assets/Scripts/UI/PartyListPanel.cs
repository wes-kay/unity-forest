using System;
using System.Collections.Generic;
using CharacterAttributes;
using PolyAndCode.UI;
using SoftKitty;
using UnityEngine;
using Zenject;

/// <summary>
/// Panel controller for the party list recyclable scroll rect.
/// Subscribes to IPartyService events and auto-refreshes the scroll rect when the party changes.
/// </summary>
public class PartyListPanel : MonoBehaviour
{
    [Header("References")]
    public RecyclableScrollRect scrollRect;

    [Inject] IPartyService partyService;

    private PartyListDataSource _dataSource;
    private int _selectedIndex = -1;

    private void Awake()
    {
        _dataSource = new PartyListDataSource();
        scrollRect.SelfInitialize = false;

        // Subscribe to party changes
        partyService.OnPartyChanged += OnPartyChanged;
    }

    private void OnDestroy()
    {
        partyService.OnPartyChanged -= OnPartyChanged;
    }

    /// <summary>
    /// Show the panel and populate the scroll rect with the current party.
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        UpdatePartyList();
    }

    /// <summary>
    /// Hide the panel.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Toggle the panel visibility.
    /// </summary>
    public void Toggle()
    {
        if (gameObject.activeSelf)
            Hide();
        else
            Show();
    }

    /// <summary>
    /// Feed the current party list to the scroll rect.
    /// </summary>
    private void UpdatePartyList()
    {
        if (scrollRect == null)
            return;

        List<Entity> members = partyService.GetMembers();
        _dataSource.SetEntities(members);
        _dataSource.OnCellClicked = OnCellClicked;
        _dataSource.SelectedIndex = _selectedIndex;

        if (!scrollRect.SelfInitialize)
        {
            scrollRect.Initialize(_dataSource);
        }
        else
        {
            scrollRect.DataSource = _dataSource;
            scrollRect.ReloadData();
        }
    }

    private void OnPartyChanged()
    {
        // Reload data so visible cells update (selection highlight, etc.)
        scrollRect.ReloadData();
    }

    private void OnCellClicked(int index, Entity entity)
    {
        _selectedIndex = index;
        _dataSource.SelectedIndex = index;
        scrollRect.ReloadData();

        string name = entity.GetAttributeString(AttributeKey.Name.GetUid());
        Debug.Log("[PartyList] Selected: " + name + " (UID: " + entity.uid + ")");
    }
}
