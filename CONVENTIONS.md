# C# Coding Conventions for Desktop QR Tools

## General Guidelines

1. Always use types explicitly wherever possible.
2. Write inline documentation for all public members and complex logic.
3. Follow Microsoft's C# Coding Conventions: https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

## Naming Conventions

1. Use PascalCase for class names, method names, and public members.
2. Use camelCase for local variables and method parameters.
3. Prefix private fields with an underscore (_).
4. Use meaningful and descriptive names for all identifiers.

## Code Structure

1. Organize code into logical regions within classes.
2. Keep methods short and focused on a single task.
3. Use dependency injection for better testability and loose coupling.

## Error Handling

1. Use exception handling (try-catch blocks) for exceptional cases.
2. Avoid using exceptions for normal flow control.
3. Always include meaningful error messages in exceptions.

## XAML Conventions

1. Use meaningful names for XAML elements, especially those referenced in code-behind.
2. Organize XAML code with proper indentation and grouping.
3. Use data binding wherever possible to separate UI from logic.

## Documentation

1. Use XML comments for all public members.
2. Include <summary>, <param>, and <returns> tags in XML comments.
3. Write clear and concise comments for complex logic within methods.

Example:

```csharp
/// <summary>
/// Generates a QR code from the given text.
/// </summary>
/// <param name="text">The text to encode in the QR code.</param>
/// <returns>A bitmap image of the generated QR code.</returns>
public System.Drawing.Bitmap GenerateQRCode(string text)
{
    // Method implementation
}
```

## Version Control

1. Write clear and descriptive commit messages.
2. Make small, focused commits that address a single concern.
3. Use feature branches for new features or significant changes.

Remember, these conventions are guidelines to ensure consistency and readability across the project. They may be adjusted as needed for specific project requirements.
