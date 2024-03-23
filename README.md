# StarBot
## Requirements
- Dotnet 6.x
- A Discord Bot Token

## Supported Platforms
- Linux

## Build/Deploy Instructions
- Pull the Repo
- Edit Configuration File (System/Config.cs)
- Run: `dotnet publish -r linux-x64`
- Run: `chmod +x {path to generated executable} (Optional, depending on permissions)`
- Execute generated executable with the Discord Bot Token as an argument (`./starbot {bot token}`)

## Information
This bot was custom built for a Discord server called StarHub. It can
- Search for XKCDs, and Reddit Posts, populating channels based on cron schedules
- Handle user reports
- (more to come)

Let me know if you'd like to contribute.