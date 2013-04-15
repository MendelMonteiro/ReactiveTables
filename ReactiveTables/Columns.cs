/*This file is part of ReactiveTables.

ReactiveTables is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ReactiveTables is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*/
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

    public static class HumanAccountColumns
    {
        public const string AccountDetails = "HumanAccount.AccountDetails";
    }
}
