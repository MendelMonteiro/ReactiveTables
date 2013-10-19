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
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace ReactiveTables.Demo
{
    internal static class PersonColumns
    {
        public const string IdColumn = "Person.PersonId";
        public const string NameColumn = "Person.Name";
        public const string IdNameColumn = "Person.IdName";
    }

    public static class AccountColumns
    {
        public const string IdColumn = "Account.AccountId";
        public const string PersonId = "Account.PersonId";
        public const string AccountBalance = "Account.AccountBalance";
    }

    public static class PersonAccountColumns
    {
        public const string AccountDetails = "PersonAccount.AccountDetails";
    }
}
