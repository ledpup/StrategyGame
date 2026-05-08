namespace GameModel
{
    public class Player
    {
        public int Id;

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
