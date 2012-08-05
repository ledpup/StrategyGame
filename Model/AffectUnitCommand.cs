using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public abstract class AffectUnitCommand : ICommand
    {
        public bool HasExecuted { get; private set; }
        public Unit Unit { get; private set; }
        public float Value { get; private set; }
        protected Action Action;
        public string MessageBase;

        public string Message { get { return string.Format(MessageBase, -Value); } }

        public AffectUnitCommand(Unit unit, float value)
        {
            Unit = unit;
            Value = value;
        }
        
        public void Execute()
        {
            if (HasExecuted)
                return;

            Action();

            HasExecuted = true;
        }
    }
}
