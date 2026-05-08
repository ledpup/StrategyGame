## Game rules
1. The game is turn-based, with players taking turns to perform actions.
1. A turn is resolved simultaneously for all players.
1. The play area is a hex-based board, with land and water hex tiles.


## Player rules
1. Players have a number of orders for each turn, defaulting to 10.
1. Players issue move commands, each step of which is resolved before the next step is executed.
1. Each move command for a unit costs one order.
1. When all orders are used, the player may not issue any more move commands.

## Movement rules
(M1) Units occupy a domain: airborne, land, or waterbound.
(M2) Land and waterbound units are restricted to land or water tiles on the board.
(M3) Airborne units can move over land or water, but must end their turn on a land tile.
(M4) A unit’s movement points determine the maximum number of hexes it may move per move command.
(M5) A unit may have multiple move commands each turn.
(M6) If two opposing units of the same domain step on the same tile simultaneously, a conflict occurs.
(M7) If a unit enters a conflict during a move command, all subsequent orders for that unit are cancelled.
(M8) The first move command for a unit does not affect morale, but subsequent move commands reduce morale, starting at 1 and increasing exponentially with each additional move command.

## Definitions
- Movement Points (MP): The maximum number of hexes a unit may move in a single move command.
- Move Command: A sequence of steps over hexes, resolved step-by-step.
- Conflict: A situation where two opposing units of the same domain attempt to occupy the same hex simultaneously.
- Domain: The movement type of a unit (airborne, land, waterbound).