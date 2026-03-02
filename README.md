# Scream and Shout

Scream and Shout is a Unity 2D platformer that combines Arduino serial input with core gameplay systems. The project explores how Unity can be used to build a complete game while also integrating external hardware input.

## Overview

The player moves through a platform level, avoids traps and monsters, and reaches the end point to win. The game includes movement, jumping, moving platforms, health, respawn, monster spawning, obstacle spawning, and win/fail UI.

Arduino data is received through serial communication and used to control gameplay actions such as movement, jumping, speed, and spawn triggers.

## Main Features

- Unity and Arduino serial communication
- Player movement and double jump
- Moving platforms
- Trap and monster damage system
- Health and respawn system
- Monster and obstacle spawning
- Success and fail UI pages
- Cinemachine camera follow

## Main Scenes

- `Assets/Scenes/Menu.unity`
- `Assets/Scenes/SampleScene.unity`

## How to Run

1. Open the project in Unity `2022.3.61f1c1`.
2. Open `Menu.unity` or `SampleScene.unity`.
3. Connect Arduino if hardware input is required.
4. Check the serial settings in `Assets/SerialInput.cs`.
5. Press Play.

## Notes

- Default serial port: `COM3`
- Default baud rate: `230400`
- The project uses modular C# scripts for player, trap, monster, UI, and respawn systems.

## Repository

<https://github.com/sylviaaaai/Scream-and-shout>
