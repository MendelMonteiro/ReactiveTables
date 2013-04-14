namespace ReactiveTables.Framework.Joins
{
    public enum JoinSide
    {
        Left,
        Right
    }
    
    public static class JoinSideExtension
    {
        public static JoinSide GetOtherSide(this JoinSide joinSide)
        {
            return joinSide == JoinSide.Left ? JoinSide.Right : JoinSide.Left;
        }
    }
}