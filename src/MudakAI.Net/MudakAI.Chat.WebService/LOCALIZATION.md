# Localization Configuration

The MudakAI Discord Bot supports multiple languages through resource files. This document explains how to configure and use localization.

## Configuration

### appsettings.json

Add or modify the `Localization` section in your `appsettings.json`:

```json
{
  "Localization": {
    "DefaultCulture": "uk-UA",
    "SupportedCultures": [ "uk-UA", "en-US" ]
  }
}
```

**Settings:**
- `DefaultCulture`: The default language/culture to use for the bot (e.g., "uk-UA" for Ukrainian, "en-US" for English)
- `SupportedCultures`: Array of supported culture codes

## Supported Languages

Currently supported languages:

| Language | Culture Code | Resource File |
|----------|--------------|---------------|
| Ukrainian | uk-UA | Strings.resx (default) |
| English | en-US | Strings.en-US.resx |

## Adding New Languages

To add support for a new language:

1. **Create a new resource file** in `MudakAI.Chat.WebService/Resources/`
   - Name it `Strings.[culture-code].resx` (e.g., `Strings.pl-PL.resx` for Polish)

2. **Copy the structure** from `Strings.en-US.resx` or `Strings.resx`

3. **Translate all values** while keeping the same resource keys

4. **Update the project file** (`MudakAI.Chat.WebService.csproj`):
   ```xml
   <ItemGroup>
     <EmbeddedResource Update="Resources\Strings.pl-PL.resx">
       <DependentUpon>Strings.resx</DependentUpon>
     </EmbeddedResource>
   </ItemGroup>
   ```

5. **Add the culture** to `SupportedCultures` in `appsettings.json`:
   ```json
   "SupportedCultures": [ "uk-UA", "en-US", "pl-PL" ]
   ```

6. **Rebuild the project**

## Runtime Culture Switching

The bot's language is set at application startup based on the `DefaultCulture` setting. The culture is applied globally to:
- All Discord command responses
- Chat messages
- Error messages
- System messages

### Changing Language at Runtime

To change the language without restarting:

1. Modify the `DefaultCulture` value in `appsettings.json`
2. Restart the application

## Resource Keys

All localized strings are organized by feature:

| Prefix | Purpose | Example Keys |
|--------|---------|--------------|
| `SetPersona_` | Set persona command | `SetPersona_Success`, `SetPersona_CommandDescription` |
| `GetPersona_` | Get persona query | `GetPersona_CurrentDescription` |
| `Announcement_` | Announcement feature | `Announcement_ChannelNotFound`, `Announcement_Success` |
| `Speak_` | Text-to-speech feature | `Speak_NotVoiceChannel`, `Speak_Success` |
| `Chat_` | Chat message formatting | `Chat_ChannelInfo`, `Chat_UserMessage` |

## Format Strings

Some resources contain placeholders for dynamic values:

```csharp
// {0} and {1} are replaced at runtime
Strings.SetPersona_Success // "Successfully set persona description: "{0}", with voice: "{1}""
```

When translating, maintain the same placeholders in the same order.

## Testing

To test a language:

1. Set `DefaultCulture` to the desired culture code
2. Start the application
3. Trigger Discord commands to verify translations
4. Check that all dynamic values (placeholders) appear correctly

## Best Practices

1. **Keep keys consistent** across all language files
2. **Test all placeholders** to ensure proper formatting
3. **Use culturally appropriate** phrases and terminology
4. **Maintain the same tone** across translations (formal/informal)
5. **Update all language files** when adding new features

## Troubleshooting

**Problem**: Bot shows English text when configured for Ukrainian
- **Solution**: Verify `DefaultCulture` is set to "uk-UA" (not "uk" or "UA")

**Problem**: Missing translations appear as resource keys
- **Solution**: Ensure all resource keys exist in the culture-specific .resx file

**Problem**: Build errors about missing resources
- **Solution**: Verify all .resx files have the correct encoding (UTF-8) and XML structure
