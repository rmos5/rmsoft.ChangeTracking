using System;
using System.Linq;
using System.Runtime.ExceptionServices;

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

        private readonly Random _random = new Random();
        private void ExecuteGetDataCommand(object? parameter)
        {
            this.Clear();
            int num = _random.Next(5, 20);
            int num1 = 1;
            NamedObject obj;
            for (int i = 0; i < num; i++)
            {
                obj = CreateNewData(i);
                AddData(obj, false);
                num1++;
            }

            SelectedItem = this.FirstOrDefault();
        }

        public class AddNewDataCommandImpl : ContextCommandBase<MainContext>
        {
            public AddNewDataCommandImpl(MainContext context) : base(context)
            {
            }

            public override bool CanExecute(object parameter)
            {
                return Context.CanExecuteAddNewDataCommand(parameter);
            }

            public override void Execute(object parameter)
            {
                Context.ExecuteAddNewDataCommand(parameter);
            }
        }

        private bool CanExecuteAddNewDataCommand(object parameter)
        {
            return this.IsTracking;
        }

        static int Number = 1000;
        private void ExecuteAddNewDataCommand(object parameter)
        {
            if (Count == 0)
                Number = 1000;

            NamedObject obj = CreateNewData(Number++);
            AddData(obj, true);
        }
    }
}
