using System;
using Verse;

namespace RPEF
{
    [Flags]
    public enum ConstraintRuleFlag : byte
    {
        None = 0,

        /// <summary>
        /// 현재 Def가 Pawn에 등록될때 Pawn의 상태가 Constraint와 충돌하는 경우 등록하지 않음
        /// </summary>
        OnApplyPawn = 1 << 0,

        /// <summary>
        /// 현재 Def가 Pawn에 등록되어있을때 새로 등록되는 Def가 Constraint와 충돌하는 경우 등록하지 않음
        /// </summary>
        OnAppliedPawn = 1 << 1,

        Exclusive = OnApplyPawn | OnAppliedPawn,
    }

    public enum ConstraintType : byte
    {
        /// <summary>
        /// 항상 true
        /// </summary>
        NoOp,

        Required,
        Conflict,

        Whitelist,
        Blacklist,
    }

    public abstract class Constraint : DefModExtension
    {
        public ConstraintRuleFlag rule = ConstraintRuleFlag.OnApplyPawn;
        public ConstraintType type = ConstraintType.NoOp;
        public string failReason = "";

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
