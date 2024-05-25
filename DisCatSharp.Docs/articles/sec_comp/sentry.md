---
uid: sec_comp_sentry
title: Sentry Vulnerability Report
author: DisCatSharp Team
---

# Disclaimer on Recent Claims and Sentry Integration Measures

## Overview

Recently, claims were made regarding the logging of tokens within our Sentry integration in the DisCatSharp library. These claims were taken seriously and thoroughly investigated. This document outlines our findings and the steps taken to further ensure the security and privacy of our users.

## Claims and Investigation

The claims suggested that our Sentry integration was logging sensitive information, including tokens. After a comprehensive review, including chasing the logs through a proxy, it was confirmed that no sensitive information such as tokens was being transmitted.

### Validation Process

1. **Proxy Analysis**: Logs sent to Sentry were inspected using a proxy to ensure no sensitive data was being transmitted.
2. **Code Review**: The relevant sections of the code were reviewed to verify that no tokens or sensitive information were included in the logs.

## Measures Taken

Despite the validation that no sensitive data was being logged, additional measures have been implemented to further enhance security:

1. **StripTokens Utility**: A utility function to remove any Discord-based tokens from strings before they are sent to Sentry.
   ```csharp
   public static string? StripTokens(string? str)
   {
      if (string.IsNullOrWhiteSpace(str))
         return str;

      str = Regex.Replace(str, @"([a-zA-Z0-9]{68,})", "{WEBHOOK_OR_INTERACTION_TOKEN}"); // Any alphanumeric string this long is likely to be sensitive information anyways
      str = Regex.Replace(str, @"(mfa\\.[a-z0-9_-]{20,})|((?<botid>[a-z0-9_-]{23,28})\\.(?<creation>[a-z0-9_-]{6,7})\\.(?<enc>[a-z0-9_-]{27,}))", "{BOT_OR_USER_TOKEN}");

      return str;
   }
   ```

2. **Breadcrumb Filter**: Filters out sensitive information from breadcrumb logs before sending them to Sentry.
   ```csharp
   options.SetBeforeBreadcrumb(b
      => new Breadcrumb(Utilities.StripTokens(b.Message),
         b.Type,
         b.Data?.Select(x => new KeyValuePair<string, string>(x.Key, Utilities.StripTokens(x.Value)))
         .ToDictionary(x => x.Key, x => x.Value),
         b.Category,
         b.Level));
   ```

3. **Transaction Filter**: Ensures that sensitive information is not included in transaction data sent to Sentry.
   ```csharp
   options.SetBeforeSendTransaction(tr =>
   {
      if (tr.Request.Data is string str)
         tr.Request.Data = Utilities.StripTokens(str);

      return tr;
   });
   ```

## Conclusion

The initial claims were found to be unsubstantiated. However, we have taken this opportunity to enhance our security measures further to ensure the utmost safety and privacy for our users. We appreciate the community's vigilance and remain committed to maintaining a secure and reliable library.

For more details on the specific pull requests and discussions related to these measures, please refer to the following:
- [Pull Request 494](https://github.com/Aiko-IT-Systems/DisCatSharp/pull/494)
- [Pull Request 493](https://github.com/Aiko-IT-Systems/DisCatSharp/pull/493)
- [Pull Request 495](https://github.com/Aiko-IT-Systems/DisCatSharp/pull/495)
- [Pull Request 501](https://github.com/Aiko-IT-Systems/DisCatSharp/pull/501)

Thank you for your continued support and understanding.
