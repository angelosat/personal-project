# personal-project
* Start-a-town! (working title)
* 2d isometric pixel-art voxel colony sim
* Angelo Tsimidakis personal project preview code
* Work in progress

# History
Intrigued by game concepts like The Sims and Dwarf Fortress, began playing around with Game Maker in 2008 with only some basic coding knowledge. A few years later started from scratch in C# with Microsoft XNA. Game concept went from farming sim, to "pixel-art isometric minecraft", to colony sim, with the latest direction specifically being an "RPG town simulator whereas you manage the town member's skills, needs, and relationships, to provide rpg-like services to out of town visitors/heroes, such as trading, resting, repairing, training, healing, questing, and more".

# Goal
Realizing how overscoped this project is for one person, I rationalized continuing working on it as a hobby by setting these goals: 
1. Finish this project and release as a commercial game given enough time
2. Develop scalable and modular components that can be reused for a smaller game, should an idea come up
3. Develop skills to be utilized for finding a job as a game (ideally) developer, in case a career change was in order

#
* Systems being iterated upon as needs arise
* Being worked on on and off as a hobby project

# Workflow
* Iterating on different systems in a rotation in order to prevent burn-out

# Current state
* Still a lot of legacy code from early iterations that is slowly phased out/refactored, in order to not break everything at once

# Highlights
* Voxel-based Isometric pixel art Colony Sim (Rimworld + Minecraft)
* Component based (scalable design)
* Custom engine (every system built in C# from the ground up with just Microsoft XNA)
* Networking (custom built UDP framework, server side, lockstep, client prediction)
* UI Framework (window manager system inspired by Transport Tycoon / OpenTTD)
* Procedural Generation (terrain, perlin noise)
* Voxel grid (3D, chunk based, light diffusion, room/area detection)
* Shaders (HLSL, cell illumination, fog, water, palette swapping, z-buffer manipulation)
* Pathfinding (Rimworld’s regions implemented for a 3D graph, gridless traversal)
* AI (behavior trees, data driven by personality, needs, assigned jobs)
* Physics engine (gridless entity movement, hitboxes, collisions, particles)
* 2D skeletal animation system (bone system, layers, weights, events)
* Gameplay (jobs, crafting, inventory, gear, stats, interactions, materials, definition driven)

# Voxel world structure
## Bounded map divided by chunks
## Perlin noise terrain generation
## Cell based light diffusion
