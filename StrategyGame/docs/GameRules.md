## Game rules
1. The game is turn-based, with players taking turns to perform actions.
1. A turn is resolved simultaneously for all players.
1. The play area is a hex-based board, with land and water hex tiles.

## Game board
1. The game board is a grid of hexagon tiles.
1. Each hex has a terrain type: grassland, forest, hills, etc.
1. Hex edges may have a river, forest, hill, mountain, reef, wall or port.
1. Two adjacent hex tiles may be linked by a road.
1. If a road crosses a river edge, it's a bridge.
1. Roads may not cross edges that have a reef, wall or port.

## Player rules
1. Players have a number of orders for each turn, defaulting to 10.
1. Players issue move commands, each step of which is resolved before the next step is executed.
1. Each move command for a unit costs one order.
1. When all orders are used, the player may not issue any more move commands.

## Movement rules
(M1) Units occupy a domain: airborne, land, or waterbound.
(M2) Land and waterbound units are restricted to land or water tiles on the board.
(M3) Airborne units can move over land or water, but must end their turn on a land tile.
(M4) If two opposing units of the same domain step on the same tile simultaneously, a conflict occurs.
(M5) A unit’s movement points determine the maximum number of hexes it may move per move command.
(M6) A unit may have multiple move commands each turn.
(M7) If a unit enters a conflict during a move command, all subsequent commands for that unit are cancelled.
(M8) The first move command for a unit does not affect morale, but subsequent move commands reduce morale, starting at 1 and increasing exponentially with each additional move command.

## Definitions
- Orders: The number of actions a player may take in a turn.
- Movement Points (MP): The maximum number of hexes a unit may move in a single move command.
- Move Command: A sequence of steps over hexes, resolved step-by-step.
- Conflict: A situation where two opposing units of the same domain occupy the same hex.
- Domain: The movement type of a unit (airborne, land, waterbound).