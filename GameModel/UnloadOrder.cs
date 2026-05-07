namespace GameModel
{
    public class UnloadOrder(MilitaryUnit unit, Tile destination = null) : IUnitOrder
    {
        public MilitaryUnit Unit { get; set; } = unit;

        public Tile Destination { get; set; } = destination;
    }
}
