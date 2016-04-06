namespace GameModel
{
    public struct ArgbColour
    {
        public short Alpha, Red, Green, Blue;

        public ArgbColour(short alpha, short red, short green, short blue)
        {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }

        public static ArgbColour Black { get { return new ArgbColour(255, 0, 0, 0); } }
        public static ArgbColour SaddleBrown { get { return new ArgbColour(255, 139, 69, 19); } }
        public static ArgbColour DodgerBlue { get { return new ArgbColour(255, 30, 144, 255); } }
    }
}