using System;
using System.Collections.Generic;
using System.Linq;
using KellermanSoftware.CompareNetObjects;

namespace Dalion.HttpMessageSigning {
    public static partial class ExtensionsForT {
        public static bool HasSamePropertyValuesAs<T>(this T first, T second) {
            return HasSamePropertyValuesAs(first, second, false, null);
        }

        public static bool HasSamePropertyValuesAs<T>(this T first, T second, bool ignoreCollectionOrder) {
            return HasSamePropertyValuesAs(first, second, ignoreCollectionOrder, null);
        }

        public static bool HasSamePropertyValuesAs<T>(this T first, T second, IEnumerable<string> membersToIgnore) {
            return HasSamePropertyValuesAs(first, second, false, membersToIgnore);
        }

        public static bool HasSamePropertyValuesAs<T>(this T first, T second, bool ignoreCollectionOrder, IEnumerable<string> membersToIgnore) {
            return HasSamePropertyValuesAs(first, second, config => {
                config.IgnoreCollectionOrder = ignoreCollectionOrder;
                config.MembersToIgnore = membersToIgnore?.ToList() ?? new List<string>();
            });
        }

        public static bool HasSamePropertyValuesAs<T>(this T first, T second, Action<ComparisonConfig> config) {
            var comparison = first.CompareTo(second, config);
            return comparison.AreEqual;
        }
    }
}