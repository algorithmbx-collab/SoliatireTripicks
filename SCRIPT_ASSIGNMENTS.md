# Script Assignments

Use this table to reattach MonoBehaviour scripts in the Unity Editor. The **Component/GameObject** column tells you where to add the script. The **Script Name** column is the class to assign. The **Key Serialized Fields** column lists the inspector fields you should wire up for correct behavior.

## Scene objects
| Component/GameObject | Script Name | Key Serialized Fields to Set |
| --- | --- | --- |
| Empty GameObject (e.g., `GameController`) | `GameController` | `LayoutSpawner` (link the layout spawner object), `Waste Pile View` (optional `CardView` for waste display), `Play Particle Prefab`, `Win Burst Prefab` |
| Layout root object (e.g., `LayoutSpawner`) | `LayoutSpawner` | `Layout Definition` (TriPeaks `LayoutDefinition` asset), `Card Prefab` (`CardView` prefab), `Card Parent` (optional container `Transform`), `Initial Cards` (optional list for testing), `Spawn On Start` |
| Canvas or UI root | `GameUIController` | `Game Controller` reference, HUD texts (`Score Text`, `Streak Text`, `Stock Text`), `Waste Image`, panels (`Win Panel`, `Lose Panel`, `Pause Panel`), buttons (`Draw`, `Undo`, `Restart`, `Pause`, `Resume`, `Win Restart`, `Lose Restart`) |
| Audio singleton object (can be empty GameObject named `AudioManager`) | `AudioManager` | `Music Source`, `SFX Source`, optional clips (`Music Clip`, `Flip`, `Draw`, `Play`, `Invalid`, `Win`, `Lose`), volume sliders |

## Card prefab
| Component/GameObject | Script Name | Key Serialized Fields to Set |
| --- | --- | --- |
| Card prefab root | `CardView` | `Front Renderer`, `Back Renderer`, `Flip Duration`, `Hover Scale Multiplier`, `Selected Tint`, `Start Face Up`, label options (`Label`, `Create Label If Missing`), feedback (`Valid Play Scale`, `Valid Play Duration`) |
| Card prefab root (same object as above) | `CardInteraction` | `Card View` (link the same `CardView`), `Game Controller` (scene controller) |
| Card prefab root | `BoxCollider2D` | Ensure collider covers the card for clicks/taps |

## ScriptableObject assets
| Asset Type | Script Name | Key Serialized Fields to Set |
| --- | --- | --- |
| Card data assets | `CardData` | `Rank`, `Suit`, `Face Sprite`, `Back Sprite` |
| Layout asset | `LayoutDefinition` | `Nodes` list with `Card Id`, `Position`, and `Blocked By` references |
