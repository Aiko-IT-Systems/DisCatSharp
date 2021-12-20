---
uid: application_commands_translations_reference
title: Translation Reference
---

# Translation Reference

> [!NOTE]
 > DisCatSharp uses [JSON](https://www.json.org) to inject the translations of [Application Commands](https://discord.com/developers/docs/interactions/application-commands).


## Command Object

| Key                      | Value                                         | Description                                                                                                 |
| ------------------------ | --------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| name                     | string                                        | name of the application command                                                                             |
| type                     | int                                           | [type](#application-command-type) of application command, used to map command types                         |
| name_translations        | array of [Translation KVPs](#translation-kvp) | array of translation key-value-pairs for the application command name                                       |
| description_translations | array of [Translation KVPs](#translation-kvp) | array of translation key-value-pairs for the application command description, only valid for slash commands |
| options                  | array of [Option Objects](#option-object)     | array of option objects containing translations                                                             |

### Application Command Type

| Type                         | Value |
| ---------------------------- | ----- |
| Slash Command                | 1     |
| User Context Menu Command    | 2     |
| Message Context Menu Command | 3     |

## Command Group Object

| Key                      | Value                                                           | Description                                                                        |
| ------------------------ | --------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| name                     | string                                                          | name of the application command group                                              |
| name_translations        | array of [Translation KVPs](#translation-kvp)                   | array of translation key-value-pairs for the application command group name        |
| description_translations | array of [Translation KVPs](#translation-kvp)                   | array of translation key-value-pairs for the application command group description |
| commands                 | array of [Command Objects](#command-object)                     | array of command objects containing translations                                   |
| groups                   | array of [Sub Command Group Objects](#sub-command-group-object) | array of sub command group objects containing translations                         |

## Sub Command Group Object

| Key                      | Value                                         | Description                                                                            |
| ------------------------ | --------------------------------------------- | -------------------------------------------------------------------------------------- |
| name                     | string                                        | name of the application command sub group                                              |
| name_translations        | array of [Translation KVPs](#translation-kvp) | array of translation key-value-pairs for the application command sub group name        |
| description_translations | array of [Translation KVPs](#translation-kvp) | array of translation key-value-pairs for the application command sub group description |
| commands                 | array of [Command Objects](#command-object)   | array of command objects containing translations                                       |

## Option Object

| Key                      | Value                                                   | Description                                                                         |
| ------------------------ | ------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| name                     | string                                                  | name of the application command option                                              |
| name_translations        | array of [Translation KVPs](#translation-kvp)           | array of translation key-value-pairs for the application command option name        |
| description_translations | array of [Translation KVPs](#translation-kvp)           | array of translation key-value-pairs for the application command option description |
| choices                  | array of [Option Choice Objects](#option-choice-object) | array of option choice objects containing translations                              |

## Option Choice Object

| Key                      | Value                                         | Description                                                                         |
| ------------------------ | --------------------------------------------- | ----------------------------------------------------------------------------------- |
| name                     | string                                        | name of the application command option choice                                       |
| name_translations        | array of [Translation KVPs](#translation-kvp) | array of translation key-value-pairs for the application command option choice name |

## Translation KVP

A translation object is a key-value-pair of `lang: value`.

Valid languages are: `ru`, `fi`, `hr`, `de`, `hu`, `sv-SE`, `cs`, `fr`, `it`, `en-GB`, `pt-BR`, `ja`, `tr`, `en-US`, `es-ES`, `uk`, `hi`, `th`, `el`, `no`, `ro`, `ko`, `zh-TW`, `vi`, `zh-CN`, `pl`, `bg`, `da`, `nl` and `lt`.

### Example Translation Array:
```json
{
    "en-US": "Hello",
    "de": "Hallo"
}
```
