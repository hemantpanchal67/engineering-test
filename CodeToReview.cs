using System;
// [Blocker - Compilation Error]: Typo in namespace 'System.Collegctions.Generic' ('Collegctions' -> 'Collections'). This prevents compilation with CS0234.
using System.Collegctions.Generic;
using System.Linq;

namespace Utility.Valocity.ProfileHelper
{
    // [Architecture / SOLID]: Move classes into separate files (e.g., Person.cs, BirthingUnit.cs). Grouping multiple distinct classes in one file harms maintainability.
    // [Naming / Code Style]: The class represents a single individual entity and should be named 'Person' (singular), not 'People'.
    public class People
    {
        // [Bug - Logic Flaw]: 'static readonly' evaluates only once at class initialization; if the process runs continuously, Under16 becomes stale. Also, AddYears(-15) represents an Under-15 cutoff rather than Under-16 (should be AddYears(-16)). Fix: 'private static DateTimeOffset Under16 => DateTimeOffset.UtcNow.AddYears(-16);'
        private static readonly DateTimeOffset Under16 = DateTimeOffset.UtcNow.AddYears(-15);

        public string Name { get; private set; }

        // [Architecture / Immutability]: DOB never changes after birth; it should be immutable ({ get; } or { get; init; }).
        // [.NET 8]: Use 'DateOnly' instead of DateTime/DateTimeOffset for birth dates to eliminate time-of-day and timezone ambiguities.
        public DateTimeOffset DOB { get; private set; }

        // [Architecture / Best Practices]: Constructor parameters mix DateTime with DateTimeOffset. Standardize on one type (or DateOnly), and add null/whitespace validation for 'name'.
        // [Bug - Time Zone / Hidden Default]: Under16.Date returns a DateTime with Kind=Unspecified and drops the time; the second constructor then converts it using the server local offset, so the default DOB depends on the machine time zone. The constructor also silently invents a date of birth instead of requiring one.
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
        // [Code Style / Docs]: Misleading XML doc on private field ('MaxItemsToRetrieve' for a list of People). Remove or provide accurate documentation.
        // [Architecture / Encapsulation]: Storing generated people in an instance field '_people' causes state accumulation across repeated calls to GetPeople. BirthingUnit should be a stateless generator or return a new list per invocation.
        // [Clean Code]: The field is only assigned in the constructor; mark it readonly (moot if the state is removed as recommended above).
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
        // [Code Style / Docs]: Inaccurate XML doc: method summary is 'GetPeoples' (should be 'GetPeople'), param name is documented as 'j' (actual is 'i'), and return type is 'List<object>' (actual is 'List<People>').
        // [Naming]: Parameter 'i' is non-descriptive; rename to 'count' or 'numberOfPeople'.
        // [Defensive Programming]: Add validation to prevent negative count arguments (e.g. ArgumentOutOfRangeException.ThrowIfNegative(i)).
        public List<People> GetPeople(int i)
        {
            // [Performance]: Pre-allocate list capacity: 'new List<People>(i)' to avoid repeated dynamic array resizing during generation.
            for (int j = 0; j < i; j++)
            {
                // [Design]: Unnecessary and overly broad try/catch. Nothing inside is expected to fail, and catching the base Exception to rethrow a generic one hides programming errors. Remove it, or catch specific exceptions and add context (e.g. the index).
                try
                {
                    // Creates a dandon Name
                    // [Code Style]: Typo in comment above ('dandon' -> 'random').
                    string name = string.Empty;

                    // [Performance]: Instantiating 'new Random()' inside a tight loop causes unnecessary heap allocations. Use thread-safe 'Random.Shared' instead.
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
            // [Architecture / Encapsulation]: Returning the private mutable list exposes internal state to external modification. Return IReadOnlyList<People> or a fresh list.
            return _people;
        }

        // [Architecture / Dead Code]: Unused private method; verify if filtering functionality is required. If needed, expose it publicly or through a domain service.
        // [Performance]: Compute cutoff date once before the LINQ query rather than calling DateTime.Now.Subtract(...) repeatedly for each element.
        private IEnumerable<People> GetBobs(bool olderThan30)
        {
            // [Bug - Logic Flaw]: Inverted comparison logic. 'DOB >= 30 years ago' filters people younger than 30. For older than 30, condition must be 'DOB <= cutoff' (or '<'). Also uses 356 days instead of 365.
            // [Bug - Time Zone]: DOB is generated from DateTime.UtcNow, but it is compared here with DateTime.Now (local time). The local offset is applied during the DateTimeOffset comparison, so the 30-year boundary shifts with the server time zone. Use one time source (UtcNow or an injected TimeProvider).
            return olderThan30 ? _people.Where(x => x.Name == "Bob" && x.DOB >= DateTime.Now.Subtract(new TimeSpan(30 * 356, 0, 0, 0))) : _people.Where(x => x.Name == "Bob");
        }

        // [Architecture / SOLID - SRP]: Single Responsibility Principle violation. Marriage and name-changing logic belongs to the Person class or a domain service, not BirthingUnit.
        // [Naming]: Parameter 'p' is non-descriptive; rename to 'person'.
        // [Design / Misleading Contract]: Despite its name, GetMarried never marries anyone: it only builds and returns a string and never updates the person (Name has a private setter). Rename it (e.g. FormatFullName) or move the behavior into the entity (person.ChangeLastName).
        public string GetMarried(People p, string lastName)
        {
            // [Bug - Null Safety]: If 'p' or 'lastName' is null, accessing properties/methods throws an unhandled NullReferenceException. Add null/empty validation checks.
            // [Bug - Hidden Behavior]: Hard-coded "test" back door. If the surname merely contains the substring "test" (case-sensitive), the method silently returns the unmarried name. This looks like leftover test logic; remove it from production code and use test doubles instead.
            if (lastName.Contains("test"))
                return p.Name;

            // [Bug - Logic Flaw]: '(p.Name.Length + lastName).Length' concatenates an integer with a string (e.g. "3Smith"). Should be '(p.Name.Length + 1 + lastName.Length) > 255'.
            if ((p.Name.Length + lastName).Length > 255)
            {
                // [Bug - Logic Flaw]: Substring operation result is discarded because strings are immutable in .NET; the method always returns the full un-truncated name.
                (p.Name + " " + lastName).Substring(0, 255);
            }

            // [Performance & Modern C#]: Use string interpolation ($"{p.Name} {lastName}") and range slicing ([..255]) to avoid repeated allocations and explicit Substring calls.
            return p.Name + " " + lastName;
        }
    }
}