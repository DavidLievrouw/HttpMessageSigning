using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure {
    internal class UniqueDbName {
        private const string DebugSpec = "DEBUG";
        private const string RegexForDate = @"(?<date>20[0-9]{2}[0-1]{1}[0-9]{1}[0-3]{1}[0-9]{1})";
        private const string RegexForId = @"(?<id>[0-9A-z\-]+)";
        private readonly bool _isDebugMode;
        private readonly string _template;
        private DateTime _creationDate;

        public UniqueDbName(string template, bool isDebugMode) {
            _template = template ?? throw new ArgumentNullException(nameof(template));
            _isDebugMode = isDebugMode;
            UniqueId = Guid.NewGuid();
            _creationDate = DateTimeOffset.UtcNow.LocalDateTime.Date;
        }

        public Guid UniqueId { get; private set; }

        public DateTime CreationDate => _creationDate;

        public static UniqueDbName Parse(string uniqueName, string template) {
            if (!TryParse(uniqueName, template, out var result)) throw new FormatException("The specified unique database name is incorrectly formatted.");
            return result;
        }

        public static bool TryParse(string uniqueName, string template, out UniqueDbName result) {
            if (uniqueName == null) throw new ArgumentNullException(nameof(uniqueName));
            if (template == null) throw new ArgumentNullException(nameof(template));

            result = null;

            var patternString = "^" + string.Format(template, RegexForId + "_" + RegexForDate) + "$";
            var regexForEligibleDatabases = new Regex(patternString, RegexOptions.IgnoreCase);
            var match = regexForEligibleDatabases.Match(uniqueName);
            if (!match.Success) return false;

            Guid? uniqueId = null;
            if (Guid.TryParse(match.Groups["id"].Value, out var parsedId)) uniqueId = parsedId;

            var isDebugMode = uniqueName.Contains(string.Format(template, $"{DebugSpec}_"));

            result = new UniqueDbName(template, isDebugMode) {
                _creationDate = DateTime.ParseExact(match.Groups["date"].Value, "yyyyMMdd", CultureInfo.InvariantCulture)
            };
            result.UniqueId = uniqueId ?? result.UniqueId;

            return true;
        }

        public override string ToString() {
            var replacement = _isDebugMode
                ? $"{DebugSpec}_{_creationDate:yyyyMMdd}"
                : $"{UniqueId}_{_creationDate:yyyyMMdd}";
            return FormatOptional(_template, replacement);
        }

        private static string FormatOptional(string stringToFormat, params string[] args) {
            if (stringToFormat == null) return null;
            if (args == null) throw new ArgumentNullException(nameof(args));

            var numReplacements = 0;
            while (stringToFormat.Contains("{" + numReplacements + "}")) {
                numReplacements++;
            }

            var parts = new List<string>(args);
            for (var c = parts.Count; c < numReplacements; ++c) {
                parts.Add(string.Empty);
            }
            
            var replacements = parts.Cast<object>().ToArray();

            return string.Format(stringToFormat, replacements);
        }
    }
}