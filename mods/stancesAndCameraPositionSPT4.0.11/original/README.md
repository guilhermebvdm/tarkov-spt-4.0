# FpsCameraStances for SPT

Take full control of your first-person view and weapon handling with this highly customizable mod.

## Features

### Camera Position Control
- Adjust your camera forward/backward, up/down, and sideways
- Fine-tune your view to your personal preference
- Toggle camera position on/off independently

### 3 Custom Weapon Stances
Create up to **3 unique weapon ready positions**, each with:
- **Rotation control**: Pitch (up/down tilt), Yaw (left/right turn), Roll (weapon cant)
- **Position control**: Forward/backward, Up/down, Sideways offsets
- Enable/disable each stance individually in the cycle
- Smooth, adjustable transitions between stances

### Flexible Stance Controls
- **Hotkey Toggle**: Press a single key (default: V) to cycle through stances
  - Works even while holding movement keys (W, A, S, D)
- **Mouse Wheel Cycling** (optional): Hold a modifier key (default: Left Alt) and scroll to cycle stances
  - Scroll up = next stance, scroll down = previous stance
- **Use Only Stances mode**: Skip the default position and cycle only through your custom stances

### Tactical Sprint Animations
- Enable compact tac sprint animations per stance (like Modern Warfare style)
- Configurable limits based on:
  - Weapon weight
  - Weapon length (inventory cells)
  - Weapon ergonomics
  - Special allowance for bullpup weapons

### ADS Behavior
- Option to smoothly reset positions when aiming down sights
- Customizable ADS hand positions and rotations
- Adjustable transition speed for smooth or snappy feel

### Advanced ADS Transitions (Shouldering Effect)
- Optional weapon shouldering animation when aiming down sights
- Simulates the physical motion of bringing a weapon up to aim (Modern Warfare style)
- **Affect Stance Transitions**: Apply the same shouldering effect when switching between stances
  - Separate intensity slider for stance transitions
  - Plays aim rattle sound on stance change (adjustable volume)
- **Weapon Stats Scaling**: Automatically adjusts based on weapon weight and ergonomics
  - Heavy/low-ergo weapons = slower, more dramatic shouldering
  - Light/high-ergo weapons = fast, subtle shouldering
  - Speed sliders act as base values that get scaled by weapon stats
- Fully customizable throw and settle phases:
  - Forward/upward throw amounts
  - Throw duration and speed
  - Settle speed to ADS position

### Fully Configurable
- All settings adjustable in-game via **BepInEx Configuration Manager** (F1)
- No file editing required - tweak values and see changes instantly
- Advanced settings available for fine-tuning

## Installation

1. Download the release or build from source
2. Copy `shwngFpsCameraStances4.dll` to `BepInEx/plugins/`
3. Launch the game
4. Press F1 to open Configuration Manager and customize settings

## Configuration Options

### Settings
| Option | Default | Description |
|--------|---------|-------------|
| Enable Camera Position | true | Toggle camera position offsets |
| Reset Positions When Aiming | true | Smoothly transition to defaults when ADS |
| ADS Transition Speed | 0.5 | How quickly hands transition (0.5-20) |
| Stance Toggle Hotkey | V | Key to cycle through stances |
| Enable Mouse Wheel Stance Cycle | false | Use mouse wheel + modifier to cycle |
| Mouse Wheel Modifier Key | Left Alt | Hold while scrolling to cycle stances |
| Use Only Stances | false | Skip default position in cycle |
| Enable Stance 1/2/3 in Cycle | true/true/false | Include each stance in the cycle |
| Stance Transition Speed | 1 | Speed of stance transitions (0.5-20) |

### Advanced ADS Transitions Settings
| Option | Default | Description |
|--------|---------|-------------|
| Enable Advanced ADS Transitions | false | Enable shouldering effect when aiming |
| Affect Stance Transition Too | true | Apply shouldering effect to stance transitions |
| ADS Shoulder Throw Intensity | 1 | Overall intensity of ADS throw effect (0-2) |
| Stance Shoulder Throw Intensity | 0.75 | Overall intensity of stance throw effect (0-2) |
| Stance Change Sound Volume | 1 | Volume of aim rattle sound on stance switch (0-2) |
| Advanced Stance Transition Intensity | 1 | How strongly weapon stats affect stance transitions (0.01-2) |
| Scale by Weapon Stats | true | Adjust ADS effect based on weapon weight/ergonomics |
| Weapon Stats Scale Intensity | 1 | How strongly weapon stats affect ADS shouldering (0-2) |
| Shoulder Throw Forward Amount | 0.02 | Forward throw distance (0-0.3) |
| Shoulder Throw Up Amount | -0.015 | Vertical offset during throw (-0.15 to 0.15) |
| Shoulder Throw Yaw | 6 | Yaw rotation during throw (-15 to 15 degrees) |
| Shoulder Throw Pitch | -3 | Pitch rotation during throw (-15 to 15 degrees) |
| Shoulder Throw Roll | -1.5 | Roll rotation during throw (-15 to 15 degrees) |
| Shoulder Throw Duration | 0.15 | Throw phase duration in seconds (0.01-0.5) |
| Shoulder Throw Speed | 2 | Speed of throw motion (0.5-5) |
| Shoulder Settle Speed | 1.5 | Speed of settling to ADS (0.5-5) |

### Per-Stance Settings
Each stance (1, 2, 3) has:
- **Rotation**: Pitch, Yaw, Roll (-45 to +45 degrees)
- **Position**: Forward/Backward, Up/Down, Sideways (-0.5 to +0.5)
- **Sprint Animation**: Enable tac sprint for this stance

### Tac Sprint Settings (Advanced)
- Weight limits (standard and bullpup weapons)
- Length limit (inventory cells)
- Ergonomics minimum requirement

## Building from Source

1. Clone the repository
2. Ensure reference DLLs are in the `References/` folder
3. Run `dotnet build -c Release`
4. Output: `bin/Release/net472/shwngFpsCameraStances4.dll`

## Known Issues

- Stance transition speed modifier affects ADS → stance transition too 
- ~~Ending tactical sprint causes weapon to warp towards low sprint position~~ Fixed with a configurable delay
- ~~It's possible to switch stances while running - because of that, if a stance has tactical sprint enabled, the weapon instantly switches to tactical sprint mode~~ (Fixed by removing ability to switch stances while sprinting)
- Very Rarely, using very high position/rotation offsets causes shouldering the weapon to take full stamina
- ~~Rarely, extreme position/rotation offsets may cause the weapon to briefly fly out of hand (2-3 frames) before safeguard logic corrects it~~ Fixed after physics update
- ~~Add "aim rattle" sound effect when switching stances~~ Implemented with adjustable volume

## Compatibility

- **SPT Version**: 4.0 +
- Compatible with most other mods
- May conflict with other stance/weapon position mods

## Credits

- Original camera offset concept inspired by hazelify's VCO mod
- Author: shwng

## Support

[Buy Me a Coffee](https://buymeacoffee.com/shengzhanzhe)
