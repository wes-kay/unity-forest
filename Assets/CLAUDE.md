# Project: [Your Game Name]
## Theme & Tone
- Dark fantasy / Grim dark
- Narrative tone: grim, morally ambiguous, no comic relief NPCs

## Quest Design Rules
- All quests must have 3 outcomes: success, failure, and a morally grey third path
- Quest IDs follow format: QST_[FACTION]_[NUMBER] e.g. QST_ASH_001
- Every quest needs: title, giver NPC, objective, reward, consequences, dialogue hooks

## Quest System
- Quest definitions: ScriptableObject assets in Assets/Resources/Quests/
- Quest IDs: QST_[FACTION]_[NUMBER] (e.g. QST_ASH_001)
- Each quest SO has: questId, title, description, giverEntityUid, targetSceneName, conditions[], rewardEntityUid, rewardXp
- Condition types: KillCondition, CollectCondition, AttributeCondition, SceneCondition
- Quest flow: Available → Active (accepted) → Completed/Failed
- QuestService uses event-driven progress tracking (RecordKill, RecordCollect, RecordSceneVisit, CheckAttributeConditions)
- Quest progress saved via ES3 (Easy Save 3)

## Asset Conventions
- Sprites: 1024x1024 for icons, 1024x1024 for portraits, 1080 × 1920 for others, named [type]_[name]_[variant], live in Assets\Assets\Images
- ScriptableObjects live in /Assets/Data/[category]/
- Scenes named: [zone]_[area] e.g. Valdenmoor_Catacombs