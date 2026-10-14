using System;
// [Blocker - Compilation Error]: Typo in namespace 'System.Collegctions.Generic' ('Collegctions' -> 'Collections'). This prevents compilation with CS0234.
using System.Collegctions.Generic;
using System.Linq;

namespace Utility.Valocity.ProfileHelper
{
    public class People
    {
        // [Bug - Logic Flaw]: 'static readonly' evaluates only once at class initialization; if the process runs continuously, Under16 becomes stale. Also, AddYears(-15) represents an Under-15 cutoff rather than Under-16 (should be AddYears(-16)). Fix: 'private static DateTimeOffset Under16 => DateTimeOffset.UtcNow.AddYears(-16);'
        private static readonly DateTimeOffset Under16 = DateTimeOffset.UtcNow.AddYears(-15);

        public string Name { get; private set; }
        public DateTimeOffset DOB { get; private set; }

        public People(string name) : this(name, Under16.Date) { }

        public People(string name, DateTime dob)
        {
            Name = name;
            DOB = dob;
        }
    }

    public class BirthingUnit
    {
        /// <summary>
        /// MaxItemsToRetrieve
        /// </summary>
        private List<People> _people;

        public BirthingUnit()
        {
            _people = new List<People>();
        }

        /// <summary>
        /// GetPeoples
        /// </summary>
        /// <param name="j"></param>
        /// <returns>List<object></returns>
        public List<People> GetPeople(int i)
        {
            for (int j = 0; j < i; j++)
            {
                try
                {
                    // Creates a dandon Name
                    string name = string.Empty;
                    var random = new Random();

                    // [Bug - Logic Flaw]: Random.Next(min, max) has an exclusive upper bound. Next(0, 1) always returns 0, so 'name' will always be Bob and 'Betty' is unreachable dead code. Should be Random.Shared.Next(0, 2).
                    if (random.Next(0, 1) == 0)
                    {
                        name = "Bob";
                    }
                    else
                    {
                        name = "Betty";
                    }
                    // Adds new people to the list

                    // [Bug - Logic Flaw]: Using 356 days instead of 365 introduces a 9-day error per year. Additionally, TimeSpan day multiplication ignores leap years. Use 'DateTime.UtcNow.AddYears(-age)' instead.
                    _people.Add(new People(name, DateTime.UtcNow.Subtract(new TimeSpan(random.Next(18, 85) * 356, 0, 0, 0))));
                }
                catch (Exception e)
                {
                    // [Bug - Exception Handling]: Discards 'e' without setting it as innerException (new Exception("...", e)), destroying original stack trace. Also causes CS0168 unused variable warning.
                    throw new Exception("Something failed in user creation");
                }
            }
            return _people;
        }

        private IEnumerable<People> GetBobs(bool olderThan30)
        {
            // [Bug - Logic Flaw]: Inverted comparison logic. 'DOB >= 30 years ago' filters people younger than 30. For older than 30, condition must be 'DOB <= cutoff' (or '<'). Also uses 356 days instead of 365.
            return olderThan30 ? _people.Where(x => x.Name == "Bob" && x.DOB >= DateTime.Now.Subtract(new TimeSpan(30 * 356, 0, 0, 0))) : _people.Where(x => x.Name == "Bob");
        }

        public string GetMarried(People p, string lastName)
        {
            // [Bug - Null Safety]: If 'p' or 'lastName' is null, accessing properties/methods throws an unhandled NullReferenceException. Add null/empty validation checks.
            if (lastName.Contains("test"))
                return p.Name;

            // [Bug - Logic Flaw]: '(p.Name.Length + lastName).Length' concatenates an integer with a string (e.g. "3Smith"). Should be '(p.Name.Length + 1 + lastName.Length) > 255'.
            if ((p.Name.Length + lastName).Length > 255)
            {
                // [Bug - Logic Flaw]: Substring operation result is discarded because strings are immutable in .NET; the method always returns the full un-truncated name.
                (p.Name + " " + lastName).Substring(0, 255);
            }

            return p.Name + " " + lastName;
        }
    }
}