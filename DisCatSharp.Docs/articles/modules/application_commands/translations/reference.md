---
uid: modules_application_commands_translations_reference
title: Translation Reference
---

# Translation Reference

> [!NOTE]
> DisCatSharp uses [JSON](https://www.json.org) to inject the translations of [Application Commands](https://discord.com/developers/docs/interactions/application-commands).

## Command Object

| Key                       | Value                                         | Description                                                                                                 |
| ------------------------- | --------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| name                      | string                                        | name of the application command                                                                             |
| description?              | string                                        | description of the application command                                                                      |
| type                      | int                                           | [type](#application-command-type) of application command, used to map command types, not valid for options  |
| name_translations         | array of [Translation KVPs](#translation-kvp) | array of translation key-value-pairs for the application command name                                       |
| description_translations? | array of [Translation KVPs](#translation-kvp) | array of translation key-value-pairs for the application command description, only valid for slash commands |
| options                   | array of [Option Objects](#option-object)     | array of option objects containing translations                                                             |

### Application Command Type

| Type                         | Value |
| ---------------------------- | ----- |
| Slash Command                | 1     |
| User Context Menu Command    | 2     |
| Message Context Menu Command | 3     |

## Command Group Object

| Key                      | Value                                                           | Description                                                                                                |
| ------------------------ | --------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| name                     | string                                                          | name of the application command group                                                                      |
| description              | string                                                          | description of the application command group                                                               |
| type                     | int                                                             | [type](#application-command-type) of application command, used to map command types, not valid for options |
| name_translations        | array of [Translation KVPs](#translation-kvp)                   | array of translation key-value-pairs for the application command group name                                |
| description_translations | array of [Translation KVPs](#translation-kvp)                   | array of translation key-value-pairs for the application command group description                         |
| commands                 | array of [Command Objects](#command-object)                     | array of command objects containing translations                                                           |
| groups                   | array of [Sub Command Group Objects](#sub-command-group-object) | array of sub command group objects containing translations                                                 |

## Sub Command Group Object

| Key                      | Value                                         | Description                                                                            |
| ------------------------ | --------------------------------------------- | -------------------------------------------------------------------------------------- |
| name                     | string                                        | name of the application command sub group                                              |
| description              | string                                        | description of the application command sub group                                       |
| name_translations        | array of [Translation KVPs](#translation-kvp) | array of translation key-value-pairs for the application command sub group name        |
| description_translations | array of [Translation KVPs](#translation-kvp) | array of translation key-value-pairs for the application command sub group description |
| commands                 | array of [Command Objects](#command-object)   | array of command objects containing translations                                       |

## Option Object

| Key                      | Value                                                   | Description                                                                         |
| ------------------------ | ------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| name                     | string                                                  | name of the application command option                                              |
| description              | string                                                  | description of the application command group                                        |
| name_translations        | array of [Translation KVPs](#translation-kvp)           | array of translation key-value-pairs for the application command option name        |
| description_translations | array of [Translation KVPs](#translation-kvp)           | array of translation key-value-pairs for the application command option description |
| choices                  | array of [Option Choice Objects](#option-choice-object) | array of option choice objects containing translations                              |

## Option Choice Object

| Key               | Value                                         | Description                                                                         |
| ----------------- | --------------------------------------------- | ----------------------------------------------------------------------------------- |
| name              | string                                        | name of the application command option choice                                       |
| name_translations | array of [Translation KVPs](#translation-kvp) | array of translation key-value-pairs for the application command option choice name |

## Translation KVP

A translation object is a key-value-pair of `"locale": "value"`.

### Example Translation Array:

```json
{
	"en-US": "Hello",
	"de": "Hallo"
}
```

## Valid Locales

| Locale | Language               |
| ------ | ---------------------- |
| id     | Indonesian             |
| da     | Danish                 |
| de     | German                 |
| en-GB  | English, UK            |
| en-US  | English, US            |
| es-ES  | Spanish                |
| es-419 | Spanish, Latin America |
| fr     | French                 |
| hr     | Croatian               |
| it     | Italian                |
| lt     | Lithuanian             |
| hu     | Hungarian              |
| nl     | Dutch                  |
| no     | Norwegian              |
| pl     | Polish                 |
| pt-BR  | Portuguese, Brazilian  |
| ro     | Romanian, Romania      |
| fi     | Finnish                |
| sv-SE  | Swedish                |
| vi     | Vietnamese             |
| tr     | Turkish                |
| cs     | Czech                  |
| el     | Greek                  |
| bg     | Bulgarian              |
| ru     | Russian                |
| uk     | Ukrainian              |
| hi     | Hindi                  |
| th     | Thai                   |
| zh-CN  | Chinese, China         |
| ja     | Japanese               |
| zh-TW  | Chinese, Taiwan        |
| ko     | Korean                 |
