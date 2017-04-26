namespace GameModel
{
    public class MoraleChangeEvent
    {
        public MoraleChangeEvent(int turn, double moraleChange, string reason)
        {
            Turn = turn;
            MoraleChange = moraleChange;
            Reason = reason;
        }

        public int Turn { get; set; }
        public double MoraleChange { get; set; }
        public string Reason { get; set; }
    }
}