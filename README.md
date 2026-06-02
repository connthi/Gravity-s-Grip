# Gravity's Grip

## Project Overview

Gravity's Grip is a first-person puzzle and exploration game built around shifting gravity and environment-driven interactions. Players navigate a mysterious building of interconnected rooms where gravity can change directions, surfaces become new floors, and torches power switches and unlock paths.

Key features:
- First-person player controller with mouse look and movement
- Orthogonal gravity triggers that change gravity direction on floors, walls, and ceilings
- Interactive objects affected by gravity (blocks, spheres, moving platforms)
- Torch-carrying mechanic with dynamic lighting and particle effects
- Puzzle rooms with unlockable doors and gravity-based challenges
- Pause menu, room reset, and game state management

## Team

- Connor Thibault (conthi@uw.edu)
- Dylan Maring (dmaring@uw.edu)
- Jesse Shieh (shiehj3@uw.edu)

## Current Status

A basic Unity prototype layout has been added and is ready for demo testing. The project now includes a starter scene, core gameplay scripts, and a simple puzzle flow with torch pickup, gravity triggers, and a door switch.

## Repository Structure

- `Assets/Scenes/` – Unity scene files, including the starter `Main.unity`
- `Assets/Scripts/` – gameplay scripts such as `PlayerController`, `TorchPickup`, and `SimpleGravityTrigger`
- `Assets/Prefabs/` – reusable prefab assets for torches, doors, and triggers
- `Assets/Materials/` – material assets for level surfaces and objects
- `Assets/Textures/` – textures used for environment and props
- `Assets/Models/` – 3D models and static geometry
- `Assets/Audio/` – sound effects and music
- `Assets/Shaders/` – custom shader assets for visual polish
- `Assets/Animation/` – animation assets for game objects
- `Assets/UI/` – menu and HUD elements
- `Assets/Particles/` – particle systems for fire and atmosphere
- `Packages/` – Unity package manifest
- `ProjectSettings/` – Unity project settings files
- `docs/` – proposal, checkpoint, presentation, and artifact documentation

## Demo Layout

The current playable demo layout contains:
- A player object with first-person controller and camera setup
- A floor plane with collider for navigation
- A torch pickup object with light and pickup behavior
- A gravity trigger that changes player gravity direction
- A door with an openable state
- A door switch trigger that opens the door when the torch enters it

## How to Run the Demo

1. Open the project in Unity 2023.3 or later.
2. Open `Assets/Scenes/Main.unity`.
3. Assign the `PlayerController` script to the `Player` object.
4. Configure the `cameraHolder` reference to the camera transform and `torchHolder` to the torch carry position.
5. Add `TorchPickup`, `SimpleGravityTrigger`, and `DoorSwitch` scripts to the matching scene objects.
6. Press Play and use mouse + WASD to move.

## Week 1 Completion Plan

### Goals for the first week

Complete a playable checkpoint demonstrating the core gameplay systems and one working puzzle room:

1. First-person controller
2. Standard movement and mouse look
3. Orthogonal gravity system with at least one gravity-changing trigger
4. Torch carrying mechanic
5. Torch-based lighting in the environment
6. One puzzle room with a basic win condition

### Phase 1: Core Systems (Days 1-3)

- Set up the Unity project and source control structure
- Implement the player controller and camera input
- Create a basic sample room environment
- Build orthogonal gravity triggers and gravity direction logic

### Phase 2: Interactable Objects (Days 4-5)

- Add a gravity-affected object prefab (block or sphere)
- Ensure objects respond to gravity changes correctly
- Create a torch object that the player can pick up and carry

### Phase 3: Lighting and Puzzle Demo (Days 6-7)

- Add torch lighting and fire particle effects
- Build a simple thermal switch and door interaction
- Design and finalize one puzzle room to demo
- Test the checkpoint and prepare for the TA meeting

## Materials and Tools

- Unity Game Engine
- Blender for asset creation or adjustment
- Free Unity Asset Store models/materials as needed
- Royalty-free sound effects if time allows

## Risks and Mitigations

- Gravity system complexity: limit to axis-aligned directions and avoid nonlinear gravity changes early.
- Dynamic lighting performance: cap active lights, optimize torch light usage.
- Puzzle complexity: start with one simple room and keep mechanics focused.

## Future Work

After week 1, the project will expand to:
- Add more puzzle rooms and gravity mechanics
- Implement thermal switches, hold-open doors, and room-specific challenges
- Polish materials, lighting, and particles
- Add menu UI, room reset, and final presentation assets

## Notes

This README will be updated as development progresses and the Unity project files are added to the repository.
