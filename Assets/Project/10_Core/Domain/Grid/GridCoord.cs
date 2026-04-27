namespace Project.Core.Domain.Grid
{
    public readonly struct GridCoord
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public GridCoord(int x, int y, int z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
