# Audio placeholders

Binary audio assets have been removed from version control. To enable in-editor sound playback, add your own clips named:

- `card_draw`
- `card_flip`
- `card_play`
- `invalid`
- `win`
- `lose`
- `music_loop`

Place them in this folder as `.wav` or `.mp3` files so `AudioManager` can load them via `Resources.Load`.
