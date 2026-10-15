# Pull Request Code Review: `CodeToReview.cs`

**Reviewer:** Hemant Panchal  
**Target File:** [`CodeToReview.cs`](CodeToReview.cs)  
**PR Review Status:** ❌ **CHANGES REQUESTED** (Cannot merge into production due to compilation blockers, runtime defects, architectural violations, and code quality issues)

---

## Table of Contents
- [Executive Summary](#executive-summary)
- [1. Blockers & Compilation Errors](#1-blockers--compilation-errors)
- [2. Bugs & Logic Flaws](#2-bugs--logic-flaws)
- [3. Architecture & SOLID Principles](#3-architecture--solid-principles)
- [4. Performance Considerations & Modern .NET 8](#4-performance-considerations--modern-net-8)
- [5. Code Style, Naming Conventions & Documentation](#5-code-style-naming-conventions--documentation)
- [6. Reusability & Code Extensibility](#6-reusability--code-extensibility)
- [7. Assumptions, Design Decisions & Trade-offs](#7-assumptions-design-decisions--trade-offs)
- [8. Generative AI Usage Disclosure](#8-generative-ai-usage-disclosure)
- [9. Defect Summary Matrix](#9-defect-summary-matrix)

---

## Executive Summary

This code review documents **compilation blockers**, **runtime bugs / logic flaws**, **architectural & SOLID violations**, **performance inefficiencies**, and **code style / documentation issues** in `CodeToReview.cs`.

Beyond the build failure and logic flaws, the current code exhibits significant readability, extensibility, and API design shortcomings:
- **Misleading Naming:** Using plural `People` for a singular domain entity and single-letter parameters (`i`, `j`, `p`).
- **Erroneous XML Documentation:** Parameter and return types in XML docs do not match actual method signatures (`param name="j"` vs `i`, `returns List<object>` vs `List<People>`).
- **Missing Guard Clauses:** No defensive boundary validation for method parameters.
- **Hidden Behavior:** A hard-coded `"test"` branch in `GetMarried`, a default date of birth that depends on the server time zone, and a local-versus-UTC time mismatch in `GetBobs` make results unpredictable.
- **Architectural Coupling:** Factory logic is tightly coupled to marital domain behaviors and lacks interface abstractions for unit testing and dependency injection.

---

## 1. Blockers & Compilation Errors

### 1.1 Namespace Typo in `using System.Collegctions.Generic;`
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L3)
* **Severity:** **BLOCKER** (Build Failure)
* **Issue:** 
  The namespace is misspelled as `Collegctions`. This produces compiler error `CS0234`:
  ```text
  error CS0234: The type or namespace name 'Collegctions' does not exist in the namespace 'System' (are you missing an assembly reference?)
  ```
* **Recommended Fix:**
  ```csharp
  using System.Collections.Generic;
  ```

---

## 2. Bugs & Logic Flaws

### 2.1 Stale Static Date & Under-15 vs Under-16 Boundary
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L13)
* **Severity:** **Major Bug**
* **Issue:**
  ```csharp
  private static readonly DateTimeOffset Under16 = DateTimeOffset.UtcNow.AddYears(-15);
  ```
  - `static readonly` fields are evaluated only once when the class is loaded. In long-running processes, this date becomes progressively stale.
  - `AddYears(-15)` represents an under-15 cutoff rather than under-16.
* **Recommended Fix:**
  ```csharp
  private static DateTimeOffset Under16Cutoff => DateTimeOffset.UtcNow.AddYears(-16);
  ```

### 2.2 Exclusive Upper Bound on `Random.Next(0, 1)` (Betty Unreachable)
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L71-L78)
* **Severity:** **Critical Logic Bug**
* **Issue:**
  `Random.Next(0, 1)` has an exclusive upper bound, returning only `0`. Name generation is hardcoded to `"Bob"`, and `"Betty"` is unreachable dead code.
* **Recommended Fix:**
  ```csharp
  string name = Random.Shared.Next(0, 2) == 0 ? "Bob" : "Betty";
  ```

### 2.3 Leap Year Omission & 356 Days Drift
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L82)
* **Severity:** **Major Bug**
* **Issue:**
  Using 356 days instead of 365 introduces a 9-day drift per year and disregards leap years.
* **Recommended Fix:**
  ```csharp
  int age = Random.Shared.Next(18, 86);
  DateTime dateOfBirth = DateTime.UtcNow.AddYears(-age);
  ```

### 2.4 Loss of Original Exception Stack Trace (`CS0168`)
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L84-L88)
* **Severity:** **Major Bug**
* **Issue:**
  Catching `e` and throwing a new `Exception` without setting `innerException` permanently erases the original stack trace and triggers `CS0168`.
* **Recommended Fix:**
  ```csharp
  catch (Exception ex)
  {
      throw new InvalidOperationException("Failed to generate simulated person profile.", ex);
  }
  ```

### 2.5 Inverted Age Filter Logic in `GetBobs`
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L100)
* **Severity:** **Critical Logic Bug**
* **Issue:**
  Using `x.DOB >= DateTime.Now.Subtract(...)` returns people *younger* than 30 instead of *older* than 30.
* **Recommended Fix:**
  ```csharp
  DateTime cutoff = DateTime.UtcNow.AddYears(-30);
  return olderThan30 
      ? _people.Where(x => x.Name == "Bob" && x.DOB <= cutoff) 
      : _people.Where(x => x.Name == "Bob");
  ```

### 2.6 Unhandled Null References in `GetMarried`
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L110-L111)
* **Severity:** **Major Bug**
* **Issue:**
  If `p` or `lastName` is `null`, calling `lastName.Contains(...)` or `p.Name` throws an unhandled `NullReferenceException`.
* **Recommended Fix:**
  ```csharp
  ArgumentNullException.ThrowIfNull(p);
  ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
  ```

### 2.7 Integer-String Concatenation in Length Check
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L114)
* **Severity:** **Critical Logic Bug**
* **Issue:**
  `(p.Name.Length + lastName).Length` concatenates an integer with a string (e.g., `"4Smith".Length == 6`), producing incorrect length validation.
* **Recommended Fix:**
  ```csharp
  if ((p.Name.Length + 1 + lastName.Length) > 255)
  ```

### 2.8 Discarded Substring Return Value (String Immutability)
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L115-L118)
* **Severity:** **Critical Logic Bug**
* **Issue:**
  `(p.Name + " " + lastName).Substring(0, 255);` computes a substring but discards it. The full untruncated name is returned.
* **Recommended Fix:**
  ```csharp
  string fullName = $"{p.Name} {lastName}";
  return fullName.Length > 255 ? fullName[..255] : fullName;
  ```

### 2.9 Hard-Coded `"test"` Back Door in `GetMarried`
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L110-L111)
* **Severity:** **Major Bug / Maintainability**
* **Issue:**
  ```csharp
  if (lastName.Contains("test"))
      return p.Name;
  ```
  Production behavior silently changes when the surname merely contains the substring `"test"`. The check is case-sensitive, matches any surname containing those letters, and returns the unmarried name without any explanation. It looks like leftover test-only logic, and it makes the method behave differently for real data.
* **Recommended Fix:**
  Remove test-specific branches from production code. If a test needs a different path, inject a dependency or use a test double instead of magic strings.

### 2.10 `GetMarried` Never Updates the Person (Misleading Contract)
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L106-L122)
* **Severity:** **Major Design Flaw**
* **Issue:**
  The method is named `GetMarried` (suggesting a state change) but only builds and returns a string. `People.Name` has a private setter, so the method cannot change the person at all. Callers who expect the person's surname to change will get no change, and the early return for `"test"` makes the result harder to predict.
* **Recommended Fix:**
  Either rename it to describe what it does (for example `FormatFullName(person, lastName)`), or move the behavior into the entity (`person.ChangeLastName(lastName)`) so the object owns its state.

### 2.11 Time Zone Mismatch in `GetBobs`
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L100)
* **Severity:** **Major Bug**
* **Issue:**
  `DOB` is built from `DateTime.UtcNow`, but `GetBobs` compares it with `DateTime.Now` (local time). When a local `DateTime` is converted to `DateTimeOffset` for the comparison, the machine's local offset is applied, so the 30-year boundary shifts by the server's UTC offset. People near the boundary can be classified incorrectly, and results differ between servers in different time zones.
* **Recommended Fix:**
  Use one time source consistently (`DateTime.UtcNow`, or an injected `TimeProvider`) for both generating and comparing dates.

### 2.12 Default Date of Birth Loses Its Time Zone Information
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L23)
* **Severity:** **Major Bug**
* **Issue:**
  `People(string name) : this(name, Under16.Date)` passes `DateTimeOffset.Date`, which returns a `DateTime` with `Kind = Unspecified` and the time removed. The second constructor then converts it back to `DateTimeOffset` using the machine's local offset. The default DOB therefore depends on the server's time zone and is truncated to midnight. The constructor also silently invents a date of birth, which hides missing data.
* **Recommended Fix:**
  Require an explicit date of birth, or store the value as `DateOnly` so there is no time or offset to lose.

### 2.13 Unnecessary and Overly Broad `try`/`catch`
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L61-L88)
* **Severity:** **Minor Design Flaw**
* **Issue:**
  Nothing inside the `try` is expected to fail, so the block adds noise. Catching the base `Exception` and rethrowing a generic `Exception("Something failed in user creation")` hides programming errors and gives no detail about which iteration failed, even after the inner exception is preserved (see 2.4).
* **Recommended Fix:**
  Remove the `try`/`catch`, or catch only specific exceptions and include context (such as the index) in the message.

---

## 3. Architecture & SOLID Principles

### 3.1 Single Responsibility Principle (SRP) Violation
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L106-L122)
* **Severity:** **Major Architectural Flaw**
* **Issue:**
  `BirthingUnit` is designed as a factory/generator responsible for generating new people profiles. Including `GetMarried` inside `BirthingUnit` violates SRP. Marriage, surname changes, and character constraints are domain entity behaviors or domain service concerns, not birthing unit responsibilities.
* **Recommended Fix:**
  Relocate marriage and name change logic into the `Person` class (e.g., `person.ChangeLastName(lastName)`) or a dedicated `MarriageService`.

### 3.2 State Leaks & Accumulation Across Invocations
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L40, #L91)
* **Severity:** **Major Design Flaw**
* **Issue:**
  - `_people` is stored as an instance field. Each call to `GetPeople(n)` appends new records to the existing list. Calling `GetPeople(10)` twice leaves the instance holding 20 records.
  - `GetPeople` returns the internal mutable `List<People>`, allowing external callers to clear or modify the class's internal state directly (`unit.GetPeople(5).Clear()`).
* **Recommended Fix:**
  Make generator methods stateless and return `IReadOnlyList<Person>`:
  ```csharp
  public IReadOnlyList<Person> GetPeople(int count)
  {
      var people = new List<Person>(count);
      // populate...
      return people;
  }
  ```

### 3.3 Immutability of Historical Entity Attributes
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L19)
* **Severity:** **Best Practice / Design**
* **Issue:**
  `DOB` (Date of Birth) is a historical fact that never changes after birth. It should not expose a public or private setter that allows mutation.
* **Recommended Fix:**
  Use getter-only properties or `init` accessors:
  ```csharp
  public DateOnly DateOfBirth { get; init; }
  ```

### 3.4 Multi-Class File Bundling
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L10, #L32)
* **Severity:** **Maintainability / Clean Code**
* **Issue:**
  Both `People` (domain entity) and `BirthingUnit` (service/factory) are bundled into a single file. This violates C# conventions and decreases maintainability as the system grows.
* **Recommended Fix:**
  Split classes into distinct files: `Person.cs` and `BirthingUnit.cs`.

### 3.5 Field Should Be `readonly`
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L40)
* **Severity:** **Minor / Clean Code**
* **Issue:**
  `_people` is assigned only in the constructor but is not marked `readonly`, so the compiler cannot protect it from accidental reassignment. (This becomes moot if the state is removed as recommended in 3.2.)
* **Recommended Fix:**
  `private readonly List<People> _people;`

---

## 4. Performance Considerations & Modern .NET 8

### 4.1 Repeated `new Random()` Allocation in Loops
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L68)
* **Severity:** **Performance**
* **Issue:**
  Instantiating `new Random()` on every iteration inside a loop creates excessive heap allocations and GC overhead. In high-throughput scenarios, tight allocation loops can also generate identical seed sequences on older runtimes.
* **Recommended Fix:**
  Use .NET 6+ thread-safe `Random.Shared`:
  ```csharp
  Random.Shared.Next(0, 2);
  ```

### 4.2 Missing List Capacity Pre-allocation
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L55-L58)
* **Severity:** **Performance**
* **Issue:**
  When generating `i` records, the default `List` capacity starts at 0 or 4 and doubles repeatedly as elements are added, causing multiple array allocations and memory copies.
* **Recommended Fix:**
  Pre-allocate capacity:
  ```csharp
  var people = new List<Person>(count);
  ```

### 4.3 Repeated LINQ Date Recalculations
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L100)
* **Severity:** **Performance**
* **Issue:**
  Evaluating `DateTime.Now.Subtract(...)` inside the `.Where()` predicate recomputes the date difference on every single element in the collection during iteration.
* **Recommended Fix:**
  Calculate the cutoff date once before executing the query:
  ```csharp
  DateTime cutoff = DateTime.UtcNow.AddYears(-30);
  return olderThan30 ? _people.Where(x => x.DOB <= cutoff) : ...;
  ```

### 4.4 Modern .NET 8 Idioms: `DateOnly` and String Range Slicing
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L19, #L117)
* **Severity:** **Modern .NET Best Practice**
* **Issue:**
  - Date of birth represents a calendar date with no time or timezone component. Mixing `DateTime` and `DateTimeOffset` causes time-of-day edge cases.
  - Substring string manipulations allocate intermediate strings unnecessarily.
* **Recommended Fix:**
  - Use modern .NET `DateOnly`.
  - Use string interpolation and range indexing `fullName[..255]`.

---

## 5. Code Style, Naming Conventions & Documentation

### 5.1 Plural Entity Class Naming (`People` vs `Person`)
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L10)
* **Severity:** **Naming / Code Style**
* **Issue:**
  The class represents a single individual entity holding one `Name` and one `DOB`. Standard .NET naming conventions specify singular nouns for entity classes (`Person`), reserving plural names for collections or namespaces.
* **Recommended Fix:**
  Rename `People` to `Person`.

### 5.2 Non-Descriptive Parameter Names (`i`, `j`, `p`)
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L55, #L58, #L106)
* **Severity:** **Clean Code / Readability**
* **Issue:**
  - In `GetPeople(int i)`, parameter `i` obscures the parameter's meaning.
  - In `for (int j = 0; j < i; j++)`, `j` is unconventional for an outer loop.
  - In `GetMarried(People p, string lastName)`, parameter `p` is an abbreviated identifier that reduces code readability.
* **Recommended Fix:**
  Use descriptive identifiers: `count` (or `numberOfPeople`), `index`, and `person`.

### 5.3 Inaccurate & Outdated XML Documentation
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L34-L36, #L47-L51)
* **Severity:** **Documentation Quality**
* **Issue:**
  - Line 33: `<summary>MaxItemsToRetrieve</summary>` on private field `_people` is inaccurate and misleading.
  - Lines 44-47: XML documentation for `GetPeople` contains multiple errors:
    * Summary reads `GetPeoples` (plural typo).
    * `<param name="j">` documents parameter `j`, but the actual method parameter is `i`.
    * `<returns>List<object></returns>` documents return type as `List<object>`, but the actual return type is `List<People>`.
* **Recommended Fix:**
  Correct the XML documentation tags:
  ```csharp
  /// <summary>
  /// Generates a collection of simulated people.
  /// </summary>
  /// <param name="count">The number of individuals to generate.</param>
  /// <returns>A list of generated <see cref="Person"/> instances.</returns>
  ```

### 5.4 Typo in Source Comments (`dandon`)
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L63)
* **Severity:** **Minor Style**
* **Issue:**
  `// Creates a dandon Name` contains an obvious spelling error (`dandon` instead of `random`).
* **Recommended Fix:**
  Change to `// Creates a random name`.

### 5.5 Missing Defensive Parameter Validation
* **File:** [`CodeToReview.cs`](CodeToReview.cs#L55)
* **Severity:** **Defensive Programming**
* **Issue:**
  `GetPeople(int i)` does not validate against negative inputs. Calling `GetPeople(-5)` executes the method without error but creates a confusing runtime contract.
* **Recommended Fix:**
  Add standard guard clauses:
  ```csharp
  ArgumentOutOfRangeException.ThrowIfNegative(count);
  ```

---

## 6. Reusability & Code Extensibility

### 6.1 Composable LINQ Extension Methods
* **Problem:** `GetBobs(bool olderThan30)` is hardcoded to a specific name ("Bob") and a specific age threshold (30). It cannot be reused for any other filtering requirement.
* **Solution:** Replace `GetBobs` with generic, composable LINQ extension methods:
  ```csharp
  public static class PersonExtensions
  {
      public static IEnumerable<Person> FilterByName(this IEnumerable<Person> people, string name) =>
          people.Where(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

      public static IEnumerable<Person> WhereOlderThan(this IEnumerable<Person> people, int age, DateOnly? asOf = null)
      {
          var today = asOf ?? DateOnly.FromDateTime(DateTime.UtcNow);
          var cutoff = today.AddYears(-age);
          return people.Where(p => p.DateOfBirth <= cutoff);
      }
  }
  ```
* **Impact:** Enables expressive queries like `people.FilterByName("Bob").WhereOlderThan(30)`.

### 6.2 Interface Abstraction (`IPersonGenerator`)
* **Problem:** `BirthingUnit` is a concrete class with no interface, preventing mocking in unit tests and violating the Dependency Inversion Principle (DIP).
* **Solution:** Extract an `IPersonGenerator` interface:
  ```csharp
  public interface IPersonGenerator
  {
      IReadOnlyList<Person> GeneratePeople(int count);
  }
  ```
* **Impact:** Allows dependency injection (`services.AddTransient<IPersonGenerator, BirthingUnit>()`) and enables test stubbing without random number generation.

### 6.3 Configurable Name Provider Strategy (`INameProvider`)
* **Problem:** First names `"Bob"` and `"Betty"` are hardcoded, preventing localization or larger data pools.
* **Solution:** Extract an `INameProvider` abstraction that injects name datasets dynamically into the generator.

### 6.4 .NET 8 `TimeProvider` Abstraction
* **Problem:** Direct calls to `DateTime.UtcNow` create non-deterministic test behavior when testing time-sensitive boundary conditions (such as age calculations).
* **Solution:** Inject .NET 8's built-in `TimeProvider` into services, allowing tests to inject `FakeTimeProvider` for 100% deterministic unit tests.

---

## 7. Assumptions, Design Decisions & Trade-offs

### 7.1 Key Assumptions
* **7.1.1 Target Runtime Environment:** Assumed the solution runs on modern .NET 8.0 / C# 12 as specified in the recruitment test instructions.
* **7.1.2 Pure Calendar Nature of Date of Birth:** Assumed date of birth represents a calendar date rather than an exact timestamp, eliminating time zone conversions and time-of-day offsets.
* **7.1.3 Simulation Generator Intent:** Assumed `BirthingUnit` is intended to be a transient simulation generator/factory rather than an in-memory repository that stores created entities.
* **7.1.4 Domain Separation of Concerns:** Assumed marital status and surname alterations are domain entity behaviors belonging to `Person` or a dedicated domain service, not a factory service.

### 7.2 Core Design Decisions
* **7.2.1 Adopting `DateOnly`:** Replaced ambiguous `DateTime`/`DateTimeOffset` types with `DateOnly` to guarantee date-only semantics and prevent timezone drift.
* **7.2.2 Historical Attribute Immutability:** Made `DateOfBirth` immutable (`{ get; }`) because birth dates cannot be altered post-birth.
* **7.2.3 Stateless Factory Lifecycle:** Refactored `BirthingUnit` to generate and return fresh collections per invocation, eliminating the `_people` instance state leak.
* **7.2.4 Fail-Fast Defensive Guard Clauses:** Implemented modern .NET guard clauses (`ArgumentException.ThrowIfNullOrWhiteSpace`, `ArgumentOutOfRangeException.ThrowIfNegative`) to surface contract violations immediately.

### 7.3 Evaluated Trade-offs
* **7.3.1 `DateOnly` vs Legacy Framework Compatibility:** 
  * *Trade-off:* `DateOnly` requires .NET 6+. Callers using older frameworks would require conversions.
  * *Decision:* Accepted because the project explicitly targets .NET 8.0.
* **7.3.2 Silent Truncation vs Validation Exception in `ChangeLastName`:**
  * *Trade-off:* Throwing an exception when exceeding 255 characters would be strictly defensive, but might break existing simulation behavior.
  * *Decision:* Preserved the original business requirement of truncating at 255 characters using clean range slicing `[..255]`.
* **7.3.3 Stateless Generation vs In-Memory Persistence:**
  * *Trade-off:* Callers must now store generated lists themselves.
  * *Decision:* Accepted to prevent memory leaks and adhere to the Single Responsibility Principle.

---

## 8. Generative AI Usage Disclosure

In compliance with the Valocity recruitment test guidelines regarding generative AI usage:
* **8.1 Where Used:** AI was utilized as a pair-review assistant to cross-reference C# compiler diagnostics (`CS0234`, `CS0168`), format comparison markdown tables, and verify modern .NET 8 API conventions (`DateOnly`, `TimeProvider`, `ArgumentException.ThrowIfNullOrWhiteSpace`).
* **8.2 Why Used:** To revalidate manually identified code review feedback, cross validating.
* **8.3 Feedback Reframing & Skill-Mapping Framework:** AI was leveraged to reframe inline code comments into a structured, categorized feedback hierarchy (Bugs, Architecture/SOLID, Performance, Clean Code). This structured categorization makes it easy for engineering leadership to identify an individual developer's technical strengths and coaching opportunities, establishing an objective baseline for future pull request reviews and targeted code quality improvements.

---

## 9. Defect Summary Matrix

| # | File Ref | Category | Description | Severity |
|---|----------|----------|-------------|----------|
| 1 | Line 3 | Blocker | Typo `System.Collegctions.Generic` | **Blocker** (`CS0234`) |
| 2 | Line 13 | Logic Bug | Stale static `Under16` & `-15` threshold | **Major** |
| 3 | Line 71 | Logic Bug | `Random.Next(0, 1)` exclusive upper bound | **Critical** |
| 4 | Line 82 | Logic Bug | 356 days instead of 365, leap year omission | **Major** |
| 5 | Line 84 | Error Handling | Discarded exception stack trace & unused variable | **Major** (`CS0168`) |
| 6 | Line 100 | Logic Bug | Inverted comparison in `GetBobs` (`>=` vs `<=`) | **Critical** |
| 7 | Line 110 | Null Safety | Missing null validation for `p` and `lastName` | **Major** |
| 8 | Line 114 | Logic Bug | `(p.Name.Length + lastName).Length` concatenation | **Critical** |
| 9 | Line 115 | Logic Bug | Discarded `Substring(0, 255)` operation | **Critical** |
| 10 | Line 106 | Architecture | `GetMarried` violates SRP in `BirthingUnit` | **Major** |
| 11 | Line 40 | Architecture | State accumulation & leaking mutable collection | **Major** |
| 12 | Line 19 | Architecture | Mutable Date of Birth property | **Minor** |
| 13 | Line 10 | Clean Code | Multiple classes bundled in single file | **Minor** |
| 14 | Line 68 | Performance | `new Random()` allocated inside loop | **Minor** |
| 15 | Line 55 | Performance | Missing list capacity pre-allocation | **Minor** |
| 16 | Line 100 | Performance | Repeated date arithmetic inside LINQ predicate | **Minor** |
| 17 | Line 10 | Naming | Plural entity name `People` instead of `Person` | **Minor** |
| 18 | Lines 55, 58, 106 | Naming | Single-letter parameter names (`i`, `j`, `p`) | **Minor** |
| 19 | Lines 34, 36, 47, 51 | Documentation | Misleading XML docs (`param name="j"`, `List<object>`) | **Minor** |
| 20 | Line 63 | Clean Code | Comment typo (`dandon` -> `random`) | **Trivial** |
| 21 | Line 55 | Robustness | Missing guard clause for negative counts | **Minor** |
| 22 | Line 110 | Maintainability | Hard-coded `"test"` back door in `GetMarried` | **Major** |
| 23 | Line 106 | Design | `GetMarried` returns a string and never updates the person | **Major** |
| 24 | Line 100 | Logic Bug | `DateTime.Now` (local) compared with UTC-based DOB | **Major** |
| 25 | Line 23 | Logic Bug | Default DOB from `Under16.Date` loses Kind and time, depends on server time zone | **Major** |
| 26 | Line 61 | Design | Unnecessary, overly broad `try`/`catch` | **Minor** |
| 27 | Line 40 | Clean Code | `_people` field could be `readonly` | **Minor** |
