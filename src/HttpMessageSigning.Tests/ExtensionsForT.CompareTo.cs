using System;
using System.Collections.Generic;
using System.Linq;
using KellermanSoftware.CompareNetObjects;

namespace Dalion.HttpMessageSigning {
    public static partial class ExtensionsForT {
        public static ComparisonResult CompareTo<T>(this T first, T second) {
            return CompareTo(first, second, false, null);
        }

        public static ComparisonResult CompareTo<T>(this T first, T second, bool ignoreCollectionOrder) {
            return CompareTo(first, second, ignoreCollectionOrder, null);
        }

        public static ComparisonResult CompareTo<T>(this T first, T second, IEnumerable<string> membersToIgnore) {
            return CompareTo(first, second, false, membersToIgnore);
        }

        public static ComparisonResult CompareTo<T>(this T first, T second, bool ignoreCollectionOrder, IEnumerable<string> membersToIgnore) {
            return CompareTo(first, second, config => {
                config.IgnoreCollectionOrder = ignoreCollectionOrder;
                config.MembersToIgnore = membersToIgnore?.ToList() ?? new List<string>();
            });
        }

        public static ComparisonResult CompareTo<T>(this T first, T second, Action<ComparisonConfig> config) {
            var defaultConfig = new ComparisonConfig {
                IgnoreObjectTypes = true // allows anonymous types to be compared
            };
            config?.Invoke(defaultConfig);
            var compareLogic = new CompareLogic(defaultConfig);
            return compareLogic.Compare(first, second);
        }
    }
}