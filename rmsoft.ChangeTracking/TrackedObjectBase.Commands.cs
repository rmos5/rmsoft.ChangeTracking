namespace rmsoft.ChangeTracking
{
    public partial class TrackedObjectBase
    {
        public class ToggleEditingCommandImpl : ContextCommandBase<TrackedObjectBase>
        {
            public ToggleEditingCommandImpl(TrackedObjectBase context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return true;
            }

            public override void Execute(object parameter)
            {
                if (Context.IsTracking)
                    Context.StopTracking(true);
                else
                    Context.StartTracking();
            }
        }

        public abstract class TrackingCommandBase : ContextCommandBase<TrackedObjectBase>
        {
            protected TrackingCommandBase(TrackedObjectBase context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return Context.IsTracking;
            }
        }

        public class UndoChangesCommandImpl : TrackingCommandBase
        {
            public UndoChangesCommandImpl(TrackedObjectBase context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanUndo();
            }

            public override void Execute(object parameter)
            {
                Context.Undo();
            }
        }

        public class RedoChangesCommandImpl : TrackingCommandBase
        {
            public RedoChangesCommandImpl(TrackedObjectBase context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanRedo();
            }

            public override void Execute(object parameter)
            {
                Context.Redo();
            }
        }

        public class ApplyChangesCommandImpl : TrackingCommandBase
        {
            public ApplyChangesCommandImpl(TrackedObjectBase context)
                : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return base.CanExecute(parameter)
                    && Context.CanUndo();
            }

            public override void Execute(object parameter)
            {
                Context.StopTracking(false);
            }
        }
    }
}
