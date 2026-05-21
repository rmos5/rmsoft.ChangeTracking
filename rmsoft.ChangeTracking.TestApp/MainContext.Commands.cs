using System;
using System.Linq;

namespace rmsoft.ChangeTracking.TestApp
{
    public partial class MainContext
    {
        public class GetDataCommandImpl : ContextCommandBase<MainContext>
        {
            public GetDataCommandImpl(MainContext context)
                : base(context)
            {
            }

            public override bool CanExecute(object? parameter)
            {
                return Context.CanExecuteGetDataCommand(parameter);
            }

            public override void Execute(object? parameter)
            {
                Context.ExecuteGetDataCommand(parameter);
            }
        }

        private bool CanExecuteGetDataCommand(object? parameter)
        {
            return !IsTracking;
        }

        private readonly Random random = new Random();

        private void ExecuteGetDataCommand(object? parameter)
        {
            Clear();
            int count = random.Next(5, 20);
            for (int i = 0; i < count; i++)
            {
                AddData(CreateNewData(i), false);
            }

            SelectedItem = this.FirstOrDefault();
        }

        public class AddNewDataCommandImpl : ContextCommandBase<MainContext>
        {
            public AddNewDataCommandImpl(MainContext context) : base(context)
            {
            }

            public override bool CanExecute(object? parameter)
            {
                return Context.CanExecuteAddNewDataCommand(parameter);
            }

            public override void Execute(object? parameter)
            {
                Context.ExecuteAddNewDataCommand(parameter);
            }
        }

        private bool CanExecuteAddNewDataCommand(object? parameter)
        {
            return IsTracking;
        }

        private static int Number = 1000;

        private void ExecuteAddNewDataCommand(object? parameter)
        {
            if (Count == 0)
                Number = 1000;

            AddData(CreateNewData(Number++), true);
        }
    }
}
