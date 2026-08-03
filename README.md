# HyRest.OnCmd

A delightfully silly, retro-inspired terminal client for OnBase that brings the nostalgia of 90s command-line interfaces to document management. 🕹️ This example project demonstrates the **HyRest Library** with a focus on:

- **Basic Username/Password Authentication**: Simple credential-based login without OpenID complexity
- **Document Retrieval**: Search and fetch documents from OnBase
- **Document Import**: Store and upload new documents to OnBase
- **Pretty Terminal UI**: Built with Spectre.Console for a colorful, interactive experience

> **Warning**: This is intentionally silly and meant to be fun. Not recommended for production use. 😄

## Overview

HyRest.OnCmd is a .NET console application that showcases the core functionality of the HyRest library through an interactive terminal interface. It demonstrates how to authenticate with OnBase using credentials and perform common document operations—all from the comfort of your terminal.

Perfect for:
- Learning how HyRest handles basic authentication
- Understanding document storage and retrieval patterns
- Experiencing the joy of 1990s-style computing
- Impressing your friends with retro tech skills

## Key Features

- **Interactive Terminal UI**: Built with Spectre.Console for rich console output, prompts, and navigation
- **Basic Authentication**: Simple username/password login (no OAuth complexity)
- **Document Retrieval**: Search and retrieve documents from OnBase with an easy-to-use menu
- **Document Import**: Upload and store new documents directly from the terminal
- **Pretty Colors and Panels**: Because terminal aesthetics matter
- **Error Handling**: Graceful handling of connection and authentication failures

## Configuration

### Environment Variables

Create a `.env` file in the application directory with the following variables:

```bash
HYREST_USERNAME=<your-onbase-username>
HYREST_PASSWORD=<your-onbase-password>
HYREST_CLIENTID=<client-id>
HYREST_CLIENTSECRET=<client-secret>
HYREST_APIURL=<onbase-api-server-url>
HYREST_IDSURL=<ids-server-url>
HYREST_USE_QUERY_LIC=true
```

Use the provided `example.env` file as a template:

```bash
cp example.env .env
# Edit .env with your OnBase credentials and server URLs
```

**Important**: The `.env` file is automatically copied to the output directory and is ignored by Git. Keep your actual credentials safe!

## Getting Started

### Prerequisites

- .NET 10.0 SDK or higher
- A running OnBase environment with API access
- Valid OnBase username and password

### Installation & Running

1. **Clone and navigate to the project**:
   ```bash
   cd samples/HyRest.OnCmd
   ```

2. **Set up your environment**:
   ```bash
   cp example.env .env
   # Edit .env with your actual OnBase credentials
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

4. **Follow the prompts**:
   - Enter your username and password at the login screen
   - Select an option from the main menu:
     - **Document Retrieval**: Search for and view documents
     - **Document Import**: Upload new documents
     - **Back**: Logout and exit

## Project Structure

- **Program.cs**: Entry point that builds and starts the CLI host
- **CliHost.cs**: Core application host managing the OnBase session and navigation flow
- **Configuration/CliHostBuilder.cs**: Sets up dependency injection and environment configuration
- **UserInterface/**: Screen-based UI components using Spectre.Console
  - **LoginScreen.cs**: Authentication screen with username/password input
  - **MainMenu.cs**: Main navigation menu
  - **DocumentRetrieval.cs**: Document search and retrieval interface
  - **DocumentImport.cs**: Document upload interface
  - **UIDefaults.cs**: Shared UI styling and defaults
  - **IScreen.cs**: Base interface for screen navigation

## Authentication Flow

1. **Application starts** and displays the login screen
2. **User enters credentials** (username and password)
3. **HyRest authenticates** with OnBase using the provided credentials
4. **Session is established** and user is taken to the main menu
5. **User can interact** with OnBase through the menu options
6. **Logout** disconnects the session when user exits

This is notably simpler than the OpenID Connect flow used in HyRest.Relay!

## Common Tasks

### Adding a New Feature

1. Create a new class in `UserInterface/` inheriting from `Screen`
2. Implement the `RunScreen()` method for your UI
3. Add a menu option to `MainMenu.cs`
4. Use the HyRest library to interact with OnBase

### Modifying the UI Style

Edit `UIDefaults.cs` to customize colors, fonts, and layout options throughout the application.

### Changing Authentication

To use a different authentication method, modify the login logic in `LoginScreen.cs` and the credential handling in `CliHost.cs`.

## Limitations & Notes

- **Not Production Ready**: This is a demo/sample application for learning purposes
- **Basic Auth Only**: Uses simple username/password authentication
- **Single Session**: Only one active OnBase session at a time
- **Console Only**: No web UI or desktop application
- **Educational Tool**: Designed to teach HyRest concepts, not as a real document management solution

## Dependencies

- **Spectre.Console**: Rich terminal UI components
- **Spectre.Console.ImageSharp**: Image rendering in terminal (for the Ternary logo)
- **DotNetEnv**: Environment variable loading from .env files
- **HyRest**: The core library being demonstrated
- **HyRest.DependencyInjection**: DI extensions for HyRest

## Troubleshooting

### "Invalid credentials" error
- Verify your username and password in the `.env` file
- Confirm the OnBase server is accessible

### Connection timeout
- Check that `HYREST_APIURL` and `HYREST_IDSURL` are correct
- Verify network connectivity to your OnBase environment

### Missing .env file
- The `.env` file must be in the working directory when running the application
- Copy it from the project directory or create one using `example.env` as a template

## Resources

- [HyRest Documentation](../../docs/)
- [Spectre.Console Documentation](https://spectreconsole.net/)
- [OnBase API Documentation](https://developer.onbase.com/)

## License

See LICENSE for license information.

---

**Enjoy your retro terminal experience!** 🖥️✨
