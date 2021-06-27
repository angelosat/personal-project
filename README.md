# personal-project
 Start-a-town! (working title)
* 2d isometric pixel-art voxel colony sim
* Angelo Tsimidakis personal project preview code
* Work in progress
* Dream game being worked on on and off throughout the years as a hobby

# History
Intrigued by game concepts like The Sims and Dwarf Fortress, began playing around with Game Maker in 2008 with only some basic coding knowledge. A few years later started from scratch in C# with Microsoft XNA. Game concept went from farming sim, to "pixel-art isometric minecraft", to colony sim, with the latest direction specifically being an "RPG town simulator whereas you manage the town member's skills, needs, and relationships, to provide rpg-like services to out of town visitors/heroes, such as trading, resting, repairing, training, healing, questing, and more".

# Goal
Despite being overscoped, continued working on this project as a hobby while driven by these goals: 
1. Finish this project and release as a commercial game given enough time
2. Develop scalable and modular components that can be reused for a smaller game, should an idea come up
3. Develop skills to be utilized for the possibility of a job as a C# developer

# Current state
* Iterating on different systems in a rotation in order to prevent burn-out
* Still a lot of legacy code from early iterations that is slowly phased out/refactored, in order to not break everything at once
* A lot of leftover code and comments that are being cleaned up to make the project more readable, since the decision to upload to github was hurried.

# Highlights
Εvery system built in C# from the ground up with just Microsoft XNA. Rediscovering the wheel in some cases but eliminating unnecessary overhead and gaining valuable exercise and insight.

## Voxel world structure
* Bounded map divided by chunks
* Perlin noise terrain generation
* Cell based light diffusion
* Area/room detection, pathable regions

## User interface
* Custom built ui framework
* Window manager system
* Graphical elements
* Implemented most basic controls such as buttons, tick boxes, sliders, progress bars, scroll bars, scrollable/collapsible lists/tables

## Rendering
* Mesh building by chunk and by horizontal slice
* Custom HLSL shader with custom vertex declarations
* Pseudo-3D effects using depth textures and z-buffer manipulation
* Effects like per-side cell illumination, fog, water

## Networking
* Custom UDP networking framework
* Server-side game logic
* Lockstep
* Client prediction

## AI
* Pathfinding using Rimworld-inspired regions system, implemented for a 3D graph
* Behavior trees using IEnumerable coroutines

## Physics
* Gridless entity movement
* Terrain material physics properties (ie. friction)
* Bounding boxes collision detection
* Particle system

## Animations
* 2D skeletal animations
* Bone system 
* Weighted animations, layered
* Events

## Gameplay systems
* Crafting system (inspired by Dwarf Fortress reactions)
* Inventory/gear system
* Stats/attributes
* Needs, mood, personality
* Experienced based skill system
