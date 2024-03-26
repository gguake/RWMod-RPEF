using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RPEF
{
    public abstract class DefConstraint<T> : DefModExtension
        where T : Def
    {
        public HashSet<T> Whitelist
        {
            get
            {
                if (!_cachedWhitelistGenerated)
                {
                    _cachedWhitelistGenerated = true;
                    var whitelist = WhitelistInner;
                    if (whitelist != null)
                    {
                        _cachedWhitelist = whitelist is HashSet<T> hashSet ? hashSet : whitelist.ToHashSet();
                    }
                }

                return _cachedWhitelist;
            }
        }

        public HashSet<T> Blacklist
        {
            get
            {
                if (!_cachedBlacklistGenerated)
                {
                    _cachedBlacklistGenerated = true;
                    var blacklist = BlacklistInner;
                    if (blacklist != null)
                    {
                        _cachedBlacklist = blacklist is HashSet<T> hashSet ? hashSet : blacklist.ToHashSet();
                    }
                }

                return _cachedBlacklist;
            }
        }

        protected abstract IEnumerable<T> WhitelistInner { get; }
        protected abstract IEnumerable<T> BlacklistInner { get; }

        protected bool _cachedWhitelistGenerated;
        protected bool _cachedBlacklistGenerated;
        protected HashSet<T> _cachedWhitelist;
        protected HashSet<T> _cachedBlacklist;
    }

    public static class DefConstraintExtension
    {
        public static bool IsAllowedFor(this Def def, Pawn pawn)
        {
            if (def?.modExtensions == null || pawn == null) { return true; }

            foreach (var extension in def.modExtensions)
            {
                switch (extension)
                {
                    case RaceConstraint raceConstraint:
                        if (!raceConstraint.IsAllowedFor(pawn.def))
                        {
                            return false;
                        }
                        break;

                    case BackstoryConstraint backstoryConstraint:
                        if (pawn.story?.Childhood != null && !backstoryConstraint.IsAllowedFor(pawn.story?.Childhood))
                        {
                            return false;
                        }
                        if (pawn.story?.Adulthood != null && !backstoryConstraint.IsAllowedFor(pawn.story?.Adulthood))
                        {
                            return false;
                        }
                        break;

                    case HairConstraint hairConstraint:
                        if (pawn.story?.hairDef != null && !hairConstraint.IsAllowedFor(pawn.story?.hairDef))
                        {
                            return false;
                        }
                        break;

                    case HediffConstraint hediffConstraint:
                        var hediffs = pawn.health?.hediffSet?.hediffs;
                        if (hediffs != null)
                        {
                            foreach (var hediff in hediffs)
                            {
                                if (!hediffConstraint.IsAllowedFor(hediff.def))
                                {
                                    return false;
                                }
                            }
                        }
                        break;

                    case XenotypeConstraint xenotypeConstraint:
                        var xenotype = pawn.genes?.Xenotype;
                        if (xenotype != null && !xenotypeConstraint.IsAllowedFor(xenotype))
                        {
                            return false;
                        }
                        break;
                }
            }

            return true;
        }

        public static bool IsAllowedFor(this Def def, ThingDef raceThingDef)
        {
            if (def?.modExtensions == null || raceThingDef == null) { return true; }

            foreach (var extension in def.modExtensions)
            {
                if (extension is RaceConstraint raceConstraint && !raceConstraint.IsAllowedFor(raceThingDef))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsAllowedFor<T>(this Pawn pawn, T def) where T : Def
        {
            if (pawn?.def.modExtensions == null || def == null) { return true; }

            foreach (var extension in pawn.def.modExtensions)
            {
                if (extension is DefConstraint<T> constraint && !constraint.IsAllowedFor(def))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsAllowedFor<T>(this DefConstraint<T> constraint, T def)
            where T : Def
        {
            if (constraint == null) { return true; }

            if (constraint.Whitelist != null)
            {
                if (!constraint.Whitelist.Contains(def)) { return false; }

                if (constraint.Blacklist != null && constraint.Blacklist.Contains(def))
                {
                    return true;
                }
            }

            if (constraint.Blacklist != null && constraint.Blacklist.Contains(def))
            {
                return false;
            }

            return true;
        }
    }
}
