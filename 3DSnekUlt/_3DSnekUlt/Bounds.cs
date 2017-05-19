namespace _3DSnekUlt
{
    public struct Bounds
    {
        public int xmax { get; set; }
        public int zmax { get; set; }
        public int xmin { get; set; }
        public int zmin { get; set; }

        public void set(int newXMax, int newXMin, int newZMax, int newZMin)
        {
            xmax = newXMax;
            xmin = newXMin;
            zmax = newZMax;
            zmin = newZMin;
        }
    }
}