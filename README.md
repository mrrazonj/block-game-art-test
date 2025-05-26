# Block Game Art Test Project

## Getting Started
### Scriptable Objects
For reskinning / theming purposes: graphic assets, sounds, and block shapes can be modified via Scriptable Objects without touching and script/scene.
<ol>
    <li>Game Settings (Assets/_BlockGame/Data/GameSettings/) - Developers can set score per line cleared at one instance, and remove/add block shapes here</li>
    <li>Game Theme (Assets/_BlockGame/Data/GameTheme/) - Developers can set the sprite visuals, particle systems, and audio clip asset here for gameplay objects</li>
    <li>Block Data/Shapes (Assets/_BlockGame/Data/BlockShapes/) - Developers can create their own custom block shapes here and their associated color</li>
</ol>

### Limitations
Since the gameplay is made via 2D gameobjects, for proper scaling sprite replacements must follow the current size of the implemented one.
<ol>
    <li>Gameplay/Grid Panel - 940px x 940px</li>
    <li>Grid Cell - 100px x 100px</li>
    <li>Block Cell - 100px x 100px (For proper coloring, sprite must be grayscaled)</li>
    <li>Line clear effects (Must cover whole column/row of panel when placed at center)
</ol>


## Implementation checklist
### Done
<ul> 
    <li>Game Settings and Themes Framework</li>
        <ul>
            <li>Flow to configure game settings (grid, score per line cleared, and eligible block data)</li>
            <li>Flow to configure game visuals via SO (sprites of gameplay elements)</li>
        </ul>
    <li>Tetromino Block Framework</li>
        <ul>
            <li>Scriptable object-based data oriented instancing</li>
            <li>Spawning 3 blocks at a time whenever the user has placed all three</li>
            <li>OnTouch, OnDrag, OnDrop handlers</li>
        </ul>
    <li>Scoring system</li>
    <li>Game over condition</li>
    <li>Serialization (local highscore saving)</li>
    <li>Canvas UI</li>
        <ul>
            <li>Score and highscore view</li>
            <li>Game over panel</li>
        </ul>
    <li>Line clear logic</li>
    <li>Visuals + Polish</li>
        <ul>
            <li>Animations/particles on line clear</li>
            <li>Menu and panels animations</li>
            <li>Sound FX</li>
        </ul>
    <li>Automatic adjustment of play area based on device screen resolution / aspect ratio</li>
</ul>

## Tools and Assets Used
<ol>
    <li>Unity 6 - 6000.0.23f1</li>
    <li>VS 2022 w/ Github Copilot - For scripting, and AI-powered context-aware code auto-completion for efficiency.</li>
    <li>Photoshop 2025 - For creation/modification of default gameplay assets (blocks, panels, etc)</li>
    <li>Leantween - For animation</li>
    <li>TextMeshPro - For text assets</li>
    <li>Newtonsoft Json - For data serialization</li>
    <li>2D Casual UI - For basic UI assets</li>
    <li>Stylized Rainbow Trails - For line clear particles</li>
    <li>UniTask - For async operation helpers</li>
</ol>

## Samples / Preview
APK link: https://drive.google.com/file/d/1fQJZB7ZZxT_hfvvPt6zL5GKcB73EUVmM/view?usp=drive_link
Preview Video: https://drive.google.com/file/d/1KPHvKWgzktcP0DlmRDqovu4YLV6UkD7h/view?usp=sharing