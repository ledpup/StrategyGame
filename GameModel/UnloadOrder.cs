namespace GameModel
{
    public class UnloadOrder : IUnitOrder
    {
        public UnloadOrder(MilitaryUnit unit, Tile destination = null)
        {
            Unit = unit;
            Destination = destination;
        }
        public MilitaryUnit Unit { get; set; }

        public Tile Destination { get; set; }
    }
}
