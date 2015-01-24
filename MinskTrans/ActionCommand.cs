using System;
using System.Windows.Input;

namespace MinskTrans.Library
{
	public class ActionCommand : ICommand
	{
		private readonly Action<Object> action;
		private readonly Predicate<Object> predicate;

		#region Implementation of ICommand

		/// <summary>
		///     Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <returns>
		///     true if this command can be executed; otherwise, false.
		/// </returns>
		/// <param name="parameter">
		///     Data used by the command.  If the command does not require data to be passed, this object can
		///     be set to null.
		/// </param>
		public bool CanExecute(object parameter)
		{
			if (predicate == null)
				return true;
			var temp = predicate.Invoke(parameter);
			
			return temp;
		}

		/// <summary>
		///     Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">
		///     Data used by the command.  If the command does not require data to be passed, this object can
		///     be set to null.
		/// </param>
		public void Execute(object parameter)
		{
			action.Invoke(parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		#endregion

		public ActionCommand(Action<object> action, Predicate<object> predicate = null)
		{
			this.action = action;
			this.predicate = predicate;
		}
	}
}