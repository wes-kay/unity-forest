

[] new attributes 

[] new characters

[] new enemies

[] new scenes


scene system
scene interaction

quest system
harvest system 

/ deployment system

team system 

team ui 
quest ui
character ui
scene ui

Quest System — Complete

┌──────────────────────────┬────────────────────────────────────────────────────────────────────────────┐
│           File           │                                  Purpose                                   │
├──────────────────────────┼────────────────────────────────────────────────────────────────────────────┤
│ QuestState.cs            │ Enum: Available → Active → Completed/Failed                                │
├──────────────────────────┼────────────────────────────────────────────────────────────────────────────┤
│ QuestDefinitionSO.cs     │ ScriptableObject asset — one per quest, created via Unity inspector        │
├──────────────────────────┼────────────────────────────────────────────────────────────────────────────┤
│ QuestCondition.cs        │ Base class + 4 condition types: KillCondition, CollectCondition,           │
│                          │ AttributeCondition, SceneCondition                                         │
├──────────────────────────┼────────────────────────────────────────────────────────────────────────────┤
│ QuestProgress.cs         │ Runtime tracker for a single quest's progress                              │
├──────────────────────────┼────────────────────────────────────────────────────────────────────────────┤
│ IQuestService.cs         │ Interface — all quest operations                                           │
├──────────────────────────┼────────────────────────────────────────────────────────────────────────────┤
│ QuestService.cs          │ Implementation — event-driven progress tracking                            │
├──────────────────────────┼────────────────────────────────────────────────────────────────────────────┤
│ QuestServiceInstaller.cs │ Zenject installer — place on a scene GameObject                            │
├──────────────────────────┼────────────────────────────────────────────────────────────────────────────┤
│ SampleQuest.json         │ Template showing quest SO structure                                        │
└──────────────────────────┴────────────────────────────────────────────────────────────────────────────┘

Usage flow:
1. Create quest SO assets in Assets/Resources/Quests/ via Unity (right-click → Quest → New Quest)
2. Drag QuestServiceInstaller onto a GameObject in the scene
3. Call questService.RecordKill("MON_ASH_WYRM") when an enemy is defeated
4. Call questService.RecordCollect("ITEM_BURNING_COAL") when items are gathered
5. Quest auto-completes when all conditions are met

Key design decisions:
- Event-driven progress (not polling) — conditions fire when events occur
- QuestService auto-loads SO assets from Resources/Quests/ at runtime
- Progress saved via ES3 (Easy Save 3), matching the project's existing save pattern
- Follows the existing Zenject installer pattern (simple MonoInstaller → Container.Bind)
