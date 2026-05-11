## Game description
1. The game is a turn-based fantasy wargame.
1. A turn is resolved simultaneously for all players.
1. The play area is a hex-based board, with land and water hex tiles.
1. The objective of the game is to capture settlements on the board.

## Game board
1. The game board is a grid of hexagon tiles.
1. Each hex has a terrain type: grassland, forest, hill, mountain, swamp, desert, sea or reef.
1. Hex edges may have a river, forest, hill, mountain, reef, wall or port.
1. Two adjacent hex tiles may be linked by a road.
1. If a road crosses a river edge, it's a bridge.
1. Roads cannot exist on edges that have a reef, wall or port.
1. Hex tiles may contain a settlement, which is a city, fortress or outpost.
1. Hexes have a stack limit, it is determined by the terrain type and the presence of a settlement. A hex cannot contain more units than its stack limit.
1. The total number of units in a hex (i.e., the count of all player's units) during a conflict may exceed the stack limit, but after the conflict is resolved, the number of units in the hex must be reduced to the stack limit.

## Player rules
1. Players control units on the board, which can be moved and used to capture settlements.
1. Players have a number of orders for each turn, defaulting to 10.
1. Players issue move commands to units, each step of which is resolved before the next step is executed.
1. Each move command for a unit costs one order.
1. When all orders are used, the player may not issue any more move commands.

## Movement rules
(M1) Units occupy a domain: airborne, land, or waterbound.
(M2) Land and waterbound units are restricted to land or water tiles on the board.
(M3) Airborne units can move over land or water, but must end their turn on a land tile.
(M4) If two opposing units of the same domain step on the same tile simultaneously, a conflict occurs.
(M5) A unit’s movement points determine the maximum number of hexes it may move per move command.
(M6) A unit may have multiple move commands each turn.
(M7) If a unit enters a conflict during a move command, all subsequent steps and commands for that unit are cancelled.
(M8) The first move command for a unit does not affect morale, but subsequent move commands reduce morale, starting at 1 and increasing exponentially with each additional move command.

### Road movement rules
(RM1) A land unit may move along a road if the road connects the hexes it is moving between.
(RM2) Airborne and waterbound units do not use roads for movement.
(RM3) A road permits a land unit to move through a hex that is normally impassable due to terrain.
(RM4) If a unit's movement is solely along roads and it has road movement bonus (most land units have this bonus), it may move additional hex(es) beyond its normal movement points.

### Naval movement rules
(NM1) A waterbound unit may only move on water and reef hexes.
(NM2) Exception to (NM1): a waterbound unit may stop on a coastal settlement tile if there is a port edge between the water and the settlement tile.
(NM3) A land unit may embark on a waterbound unit if they are in a coastal settlement and there is a port edge between the land and water hexes.
(NM4) A land unit may embark on a waterbound unit if they are adjacent to a water hex and there is a port edge between the land and water hexes.
(NM5) A waterbound unit may only ferry land units if their transport capacity is sufficient to carry land units.
(NM6) A waterbound unit may transport multiple land units.
(NM7) If an airborne unit is lost during a conflict, all land units it was transporting are lost.

### Airborne movement rules
(AM1) An airborne unit may move over any terrain type, but must end its turn on a land tile.
(AM2) An airborne unit may airlift land units if they are in the same hex and the airborne unit has sufficient transport capacity.
(AM3) If an airborne unit is lost during a conflict, all land units it was transporting are lost.

## Settlement rules
(S1) A settlement can be built on any land hex tile.
(S2) Settlements may be owned by a player or neutral.
(S3) A settlement can be captured by moving an opposing unit into the settlement's hex tile.
(S4) Most units may move into and occupy a settlement tile, but only one player may own the settlement at a time.
(S5) Further clarification to (S4): a waterbound unit may occupy a coastal settlement tile if there is a port edge between the water and the settlement tile (NM2).
(S6) Settlements have a garrison: a stationary unit used solely for defense.
(S7) Additional friendly units may garrison a settlement, adding to the settlement's defense.
(S8) When an enemy force moves into a settlement tile, the settlement will be in a state of siege.
(S9) A settlement is captured when the garrison (including any additional units (S6)) is defeated.
(S10) If a settlement is captured (S9), the player who captured it becomes the new owner of the settlement.
(S11) A siege is lifted when the enemy force is defeated or retreats from the settlement tile.
(S12) Until the settlement is captured (S9), the owner of the settlement retains control of it, including any benefits it provides.
(S13) While a siege is ongoing, friendly and enemy units may move into and out of the settlement tile.
(S14) Settlements have a defence modifier that increases the defensive strength of units occupying the settlement tile.
(S15) Sappers and siege engines damage fortifications in settlements, reducing the settlement's defence modifier.

## Definitions
- Orders: The number of actions a player may take in a turn.
- Movement Points (MP): The maximum number of hexes a unit may move in a single move command.
- Move Command: A sequence of steps over hexes, resolved step-by-step.
- Conflict: A situation where two opposing units of the same domain occupy the same hex.
- Siege: A conflict that occurs when an unit is on a tile of a neutral or enemy settlement.
- Domain: The movement type of a unit (airborne, land, waterbound).