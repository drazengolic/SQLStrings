# SQLStrings

SQLStrings is a [Source Generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) for C#, inspired by [Yesql](https://github.com/krisajenkins/yesql) for Clojure. 

Compared to Yesql and similar libraries, it does not try to act as a (Micro)ORM. Instead, all SQL queries found in files will be stored as static class fields that can be referenced from anywhere within the code, so you can continue using all features of your favorite ORM, but with less clutter.

## Rationale

Why keep SQL queries in separate files? Here are some of the possible reasons you'd want that:

* Ease of access to SQL queries, for both developers and DBAs
* Easily shareable across the codebase
* Your editor/IDE does not handle syntax highlighting of SQL in strings
* Embedding another language in strings feels clunky and unnatural
* SQL is code. Why not keep it in it's own files, just like the rest of the code?

**Note:** If your application needs to support multiple database providers, writing raw SQL queries makes little sense, whether as strings or in separate files, so you may not benefit from using this library.

## Installation

Library is avaliable as a NuGet package `SQLStrings`, so it can be installed for example via CLI:

```
dotnet add package SQLStrings
```

## Usage

### 1. Create .sql files

Then reference your SQL files in `.csproj` file as `AdditionalFiles` elements, and set `SQLStrings_Enable` attribute to `true` for every file you want to be parsed. For example:

```xml
<ItemGroup>
    <AdditionalFiles Include="users.sql" SQLStrings_Enable="true" />
    <!-- more files -->
</ItemGroup>
```

Since the name of the file is `users.sql`, this will become a `Users` class in `SQLStrings` namespace. File path is not taken into account.

### 2. Write SQL queries with a lightweight annotation

Every SQL comment line that starts with `-- Name:` indicates a new query. The provided name will be used as a class field name, so it must match C# field naming rules (i.e. no spaces). For example, comment `-- Name: GetUsersQuery` will create a field named `GetUsersQuery`.

After the name indicator, the rest of the comments in the comment block will be used as a field summary. After the comment block, all text that remains until the next `-- Name:` indicator (or the end of file) will be read as an SQL query.

Full example:

SQL file **users.sql**
```sql
-- Name: GetUsersQuery
-- Select all users.
-- Does not require input.
select * from user;

-- Name: DeleteUserQuery
-- Deletes a user via given @id
delete from user where id = @id;
```

generates the following C# code:

```cs
namespace SQLStrings
{
	public static class Users
	{
		///<summary>
		///Select all users.
		///Does not require input.
		///</summary>
		public static readonly string GetUsersQuery = @"select * from user;";

		///<summary>
		///Deletes a user via given @id
		///</summary>
		public static readonly string DeleteUserQuery = @"delete from user where id = @id;";

	}
}
```

You can also check the [ConsoleDemo](./ConsoleDemo/) app.

## Caveats

1. Source generation may not always be up-to-date with the edited file when the analyzer is slow to kick in, so sometimes it may be necessary to restart OmniSharp (in VSCode) or Visual Studio
2. If `SQLStrings_Enable` doesn't seem to work, you may need to add the following setting before the referenced files:
```xml
<CompilerVisibleItemMetadata Include="AdditionalFiles" MetadataName="SQLStrings_Enable" />
```

## Licence

MIT