using System;
using System.Collections.Generic;
using FakeItEasy;
using KellermanSoftware.CompareNetObjects;

namespace Dalion.HttpMessageSigning {
    public static class ExtensionsForArgumentConstraintManagerOfT {
        public static T HasSamePropertyValuesAs<T>(this IArgumentConstraintManager<T> manager, object value) {
            return HasSamePropertyValuesAs(manager, value, false, null);
        }

        public static T HasSamePropertyValuesAs<T>(this IArgumentConstraintManager<T> manager, object value, IEnumerable<string> membersToIgnore) {
            return HasSamePropertyValuesAs(manager, value, false, membersToIgnore);
        }

        public static T HasSamePropertyValuesAs<T>(this IArgumentConstraintManager<T> manager, object value, bool ignoreCollectionOrder) {
            return HasSamePropertyValuesAs(manager, value, ignoreCollectionOrder, null);
        }

        public static T HasSamePropertyValuesAs<T>(this IArgumentConstraintManager<T> manager, object value, bool ignoreCollectionOrder, IEnumerable<string> membersToIgnore) {
            return manager.Matches(
                x => x.HasSamePropertyValuesAs(value, ignoreCollectionOrder, membersToIgnore),
                x => x.Write("object that matches by property values as ").WriteArgumentValue(value));
        }

        public static T HasSamePropertyValuesAs<T>(this IArgumentConstraintManager<T> manager, object value, Action<ComparisonConfig> config) {
            return manager.Matches(
                x => x.HasSamePropertyValuesAs(value, config),
                x => x.Write("object that matches by property values as ").WriteArgumentValue(value));
        }
    }
}