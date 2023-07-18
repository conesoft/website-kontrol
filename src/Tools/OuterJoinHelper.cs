namespace Conesoft_Website_Kontrol.Tools
{
    public static class OuterJoinHelper
    {
        internal static IList<TR> FullOuterGroupJoin<TA, TB, TK, TR>(
            this IEnumerable<TA> a,
            IEnumerable<TB> b,
            Func<TA, TK> selectKeyA,
            Func<TB, TK> selectKeyB,
            Func<IEnumerable<TA>, IEnumerable<TB>, TK, TR> projection,
            IEqualityComparer<TK>? cmp = null)
        {
            cmp ??= EqualityComparer<TK>.Default;
            var alookup = a.ToLookup(selectKeyA, cmp);
            var blookup = b.ToLookup(selectKeyB, cmp);

            var keys = new HashSet<TK>(alookup.Select(p => p.Key), cmp);
            keys.UnionWith(blookup.Select(p => p.Key));

            var join = from key in keys
                       let xa = alookup[key]
                       let xb = blookup[key]
                       select projection(xa, xb, key);

            return join.ToList();
        }

        internal static IList<TR> FullOuterJoin<TA, TB, TK, TR>(
            this IEnumerable<TA> a,
            IEnumerable<TB> b,
            Func<TA, TK> selectKeyA,
            Func<TB, TK> selectKeyB,
            Func<TA, TB, TK, TR> projection,
            TA? defaultA = default,
            TB? defaultB = default,
            IEqualityComparer<TK>? cmp = null)
        {
            cmp ??= EqualityComparer<TK>.Default;
            var alookup = a.ToLookup(selectKeyA, cmp);
            var blookup = b.ToLookup(selectKeyB, cmp);

            var keys = new HashSet<TK>(alookup.Select(p => p.Key), cmp);
            keys.UnionWith(blookup.Select(p => p.Key));

            var join = from key in keys
                       from xa in alookup[key].DefaultIfEmpty(defaultA)
                       from xb in blookup[key].DefaultIfEmpty(defaultB)
                       select projection(xa, xb, key);

            return join.ToList();
        }
    }
}