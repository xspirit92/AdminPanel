namespace CubArt.Infrastructure.Extentions
{
    public static class SqlColumnTypes
    {
        public const string Date = "date";
        public const string TimeStamp = "timestamp";
        public const string TimeStampWithTimeZone = "timestamp with time zone";
        public const string JsonB = "jsonb";
        public const string Json = "json";
        public const string Text = "text";
        public const string Xid = "xid";

        public static string Varchar(int length)
        {
            return $"varchar({length})";
        }
    }
}
