namespace WaterColorSort.Classes
{
    internal readonly struct ClickData
    {
        private const double DefaultSleep = 0.15;

        internal readonly int x;
        internal readonly int y;
        internal readonly double sleep;
        internal readonly (int x, int y) coord;

        internal ClickData(int x, int y, double sleep = DefaultSleep)
        {
            this.x = x;
            this.y = y;
            coord = (x, y);
            this.sleep = sleep;
        }

        internal ClickData((int x, int y) coord, double sleep = DefaultSleep) : this(coord.x, coord.y, sleep)
        {

        }

        internal ClickData(System.Drawing.Point point, double sleep = DefaultSleep) : this(point.X, point.Y, sleep)
        {

        }
    };
}
