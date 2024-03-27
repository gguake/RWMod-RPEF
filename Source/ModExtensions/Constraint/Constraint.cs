using Verse;

namespace RPEF
{
    public enum ConstraintType : byte
    {
        NoOp,
        Required,
        Conflict,

        Whitelist,
        Blacklist,
    }

    public abstract class Constraint : DefModExtension
    {
        public ConstraintType type = ConstraintType.NoOp;

        public virtual void Validate()
        {
        }

        public bool Check(Pawn pawn)
        {
            var matched = Match(pawn);

            switch (type)
            {
                case ConstraintType.NoOp:
                    return true;

                case ConstraintType.Required:
                case ConstraintType.Whitelist:
                    return matched;

                case ConstraintType.Conflict:
                case ConstraintType.Blacklist:
                    return !matched;
            }

            return false;
        }

        public abstract bool Check(Def def);

        protected abstract bool Match(Pawn pawn);
    }

    public abstract class Constraint<TDef> : Constraint 
        where TDef : Def
    {
        public sealed override bool Check(Def def)
        {
            if (def is TDef typedDef)
            {
                var matched = Match(typedDef);

                switch (type)
                {
                    case ConstraintType.NoOp:
                        return true;

                    case ConstraintType.Required:
                    case ConstraintType.Whitelist:
                        return matched;

                    case ConstraintType.Conflict:
                    case ConstraintType.Blacklist:
                        return !matched;
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        protected abstract bool Match(TDef def);
    }
}
