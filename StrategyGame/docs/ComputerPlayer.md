# Computer player implementation

## Connected regions
1. Adjacent hex tiles that are of the same base operational domain (land or water) are considered to be part of the same connected region.
1. The computer player can use connected regions when determining movement and strategy.

### Mountains and roads
1. Mountains are impassable terrain for most land units, they break-up land regions into multiple connected regions if there is no non-mountain hex or edge connecting them.
1. A road that creates a mountain pass allows land units to move through the mountain, connecting otherwise separated connected regions.

### Mountain folk
1. Some land units can operate in mountainous terrain. For these units, all connected land hexes are considered part of the same connected region.

## Pathfinding
1. Use the A* algorithm for pathfinding, which is efficient and can be adapted to account for terrain and movement rules.