# Gravity's Grip

## Project Overview

Gravity's Grip is a first-person puzzle and exploration game built around shifting gravity and environmental interaction. Players navigate a mysterious building of interconnected rooms where gravity can change direction, walls become floors, and torches activate thermal mechanisms.

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

This repository contains the project proposal and planning notes for the Gravity's Grip artifact. The Unity project itself will be added and linked as development progresses.

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
