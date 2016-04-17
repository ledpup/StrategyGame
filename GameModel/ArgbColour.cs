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
    }

    public static class Colours
    {
        public static ArgbColour Black { get { return new ArgbColour(255, 0, 0, 0); } }
        public static ArgbColour SaddleBrown { get { return new ArgbColour(255, 139, 69, 19); } }
        public static ArgbColour DodgerBlue { get { return new ArgbColour(255, 30, 144, 255); } }
        public static ArgbColour DarkGreen { get { return new ArgbColour(255, 0, 100, 0); } }
        public static ArgbColour SandyBrown { get { return new ArgbColour(255, 244, 164, 96); } }
        public static ArgbColour Brown { get { return new ArgbColour(255, 165, 42, 42); } }

        public static ArgbColour Red { get { return new ArgbColour(255, 255, 42, 42); } }

        public static ArgbColour Blue { get { return new ArgbColour(255, 42, 42, 255); } }
    }
}