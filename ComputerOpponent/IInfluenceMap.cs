using Hexagon;

namespace ComputerOpponent
{
    /// <summary>
    /// Defines a reusable influence field for reading, writing, and spreading tactical pressure across a board.
    /// </summary>
    public interface IInfluenceMap
    {
        int Width { get; }
        int Height { get; }
        int Length { get; }

        float GetValue(int index);
        float GetValue(Hex hex);

        void SetValue(int index, float value);
        void AddValue(int index, float value);
        void Clear();

        /// <summary>
        /// Diffuses current values to adjacent hexes while reducing strength by decayFactor each step.
        /// </summary>
        void Propagate(float decayFactor, int steps = 1);

        /// <summary>
        /// Writes a radial influence gradient around an origin using strength at the centre and radius falloff.
        /// </summary>
        void AddRadialInfluence(Hex origin, float strength, int radius);

        float[] CopyValues();
    }
}
