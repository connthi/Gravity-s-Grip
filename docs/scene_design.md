# Scene Design and World Depth

This document outlines the immersive world-building elements for Gravity's Grip.

## Environment Goals

- Make rooms feel lived-in with furniture, props, and architectural detail.
- Use ambient lighting, fog, and particle effects to create a warm, mysterious atmosphere.
- Place breakable and throwable objects naturally on tables, shelves, and ledges.
- Build a believable manor or laboratory style interior with curved hallways and gravity-altered rooms.

## Recommended Props

- Tables, chairs, shelves, crates
- Vases, lanterns, books, scrolls, and small bottles
- Loose debris, fallen papers, and dust particles
- Wall panels, pipes, and mechanical fixtures near gravity switches

## Throwables and Interactable Objects

- Add `ThrowableObject` to props that can be grabbed and thrown.
- Tag those objects as `Throwable` and place them in reachable areas.
- Use `BreakableProp` on decorative vases and glass objects to give the world reactive physics.
- Place one or more throwable objects on a table near the player start.

## Atmospheric Effects

- Add an `AmbientAtmosphere` object in the scene to enable fog and ambient audio.
- Use warm point lights, spotlights, and particle systems for dust motes.
- Keep color palettes muted and use contrasting torchlight to highlight interactable areas.
- Use soft directional light through windows or vents and add a subtle bloom effect.

## Puzzle Integration

- Use environment props to lead players through each puzzle room.
- Example progression:
  1. First room: pick up torch and light a switch.
  2. Second room: navigate through a gravity-shifted corridor.
  3. Third room: use a throwable prop to trigger a pressure plate or break a barrier.

## Developer Notes

- Update `README.md` when asset types and prop placement are finalized.
- Keep interactive props consistent with the scene's visual theme.
