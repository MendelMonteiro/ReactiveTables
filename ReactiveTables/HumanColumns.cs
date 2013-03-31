namespace ReactiveTables
{
    internal static class HumanColumns
    {
        public const string IdColumn = "Human.HumanId";
        public const string NameColumn = "Human.Name";
        public const string IdNameColumn = "Human.IdName";
    }

    public static class AccountColumns
    {
        public const string IdColumn = "Account.AccountId";
        public const string HumanId = "Account.HumanId";
        public const string AccountBalance = "Account.AccountBalance";
    }
}