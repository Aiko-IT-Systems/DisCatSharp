# Contributing to DisCatSharp
We're really happy to accept contributions. However we also ask that you follow several rules when doing so.

# Proper base
When opening a PR, please make sure your branch targets the latest release branch, in this case it would be `main`. Also make sure your branch is even with the target branch, to avoid unnecessary surprises.

# Versioning
We follow custom [SemVer](https://semver.org/) versioning when it comes to pushing stable releases. Ideally, this means you should only be creating PRs for `PATCH` and `MINOR` changes. If you wish to introduce a `major` (breaking) change, please discuss it beforehand so we can determine how to integrate it into our next major version. If this involves removing a public facing property/method, mark it with the `Deprecated` attribute instead on the latest release branch.

Warning: We use a slightly different method for SemVer. {DISCORD_API_VERSION}.{MAJOR}.{MINOR}-{PATCH}

# Proper titles
When opening issues, make sure the title reflects the purpose of the issue or the pull request. Prefer past tense, and
be brief. Further description belongs inside the issue or PR.

# Descriptive changes
We require the commits describe the change made. It can be a short description. If you fixed or resolved an open issue,
please reference it by using the # notation.

Examples of good commit messages:

* `Fixed a potential memory leak with cache entities. Fixes #142.`
* `Implemented new command extension. Resolves #169.`
* `Changed message cache behaviour. It's now global instead of per-channel.`
* `Fixed a potential NRE.`
* ```
  Changed message cache behaviour:

  - Messages are now stored globally.
  - Cache now deletes messages when they are deleted from discord.
  - Cache itself is now a ring buffer.
  ```

# Code style
We use [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
throughout the repository, with several exceptions:

* Preference of `this`. While this one is not required, it's ill-advised to remove the existing instances thereof.
* When working with async code, and your method consists of a single `await` statement not in any `if`, `while`, etc.
  blocks, pass the task through instead of awaiting it. For example:

  ```cs
  public Task DoSomethingAsync()
    => this.DoAnotherThingAsync();

  public Task DoAnotherThingAsync()
  {
      Console.WriteLine("42");
      return this.DoYetAnotherThingAsync(42);
  }

  public async Task DoYetAnotherThingAsync(int num)
  {
      if (num == 42)
          await SuperAwesomeMethodAsync();
  }
  ```

In addition to these, we also have several preferences:

* Use initializer syntax when possible:

  ```cs
  var a = new Class
  {
      StringNumber = "forty-two",
      Number = 42
  };

  var b = new Dictionary<string, int>()
  {
      ["forty-two"] = 42,
      ["sixty-nine"] = 69
  };

  var c = new List<int>() { 42, 69 };

  var d = new[] { 42, 69 };
  ```
* Inline `out` declarations when possible: `SomeOutMethod(42, out var stringified);`
* Members in classes should be ordered as follows (with few exceptions):
   * Public `const` fields.
   * Non-public `const` fields.
   * Public static properties.
   * Public static fields.
   * Non-public static properties.
   * Non-public static fields.
   * Public properties.
   * Public fields.
   * Private properties.
   * Private fields.
   * Static constructor.
   * Public constructors.
   * Non-public constructors.
   * Public methods (with the exception of methods overridden from `System.Object`).
   * Non-public methods.
   * Methods overridden from `System.Object`.
   * Public static methods.
   * Non-public static methods.
   * Operator overloads.
   * Public events.
   * Non-public events.

# Code changes
One of our requirements is that all code change commits must build successfully. This is verified by our CI. When you
open a pull request, AppVeyor will start a build. You can view its summary by visiting it from the checks section on
the PR overview page.

PRs that do not build will not be accepted.

Furthermore we require that methods you implement on Discord entities have a reflection in the Discord API.

# Developer Certificate of Origin (DCO)
```
Version 1.1

Copyright (C) 2004, 2006 The Linux Foundation and its contributors.

Everyone is permitted to copy and distribute verbatim copies of this
license document, but changing it is not allowed.


Developer's Certificate of Origin 1.1

By making a contribution to this project, I certify that:

(a) The contribution was created in whole or in part by me and I
    have the right to submit it under the open source license
    indicated in the file; or

(b) The contribution is based upon previous work that, to the best
    of my knowledge, is covered under an appropriate open source
    license and I have the right under that license to submit that
    work with modifications, whether created in whole or in part
    by me, under the same open source license (unless I am
    permitted to submit under a different license), as indicated
    in the file; or

(c) The contribution was provided directly to me by some other
    person who certified (a), (b) or (c) and I have not modified
    it.

(d) I understand and agree that this project and the contribution
    are public and that a record of the contribution (including all
    personal information I submit with it, including my sign-off) is
    maintained indefinitely and may be redistributed consistent with
    this project or the open source license(s) involved.
```
