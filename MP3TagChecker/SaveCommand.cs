using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;

namespace Aldentea.MP3Tag.Checker
{
	internal class SaveCommand : ICommand
	{
		public event EventHandler? CanExecuteChanged = delegate { };

		public bool CanExecute(object? parameter)
		{
			return parameter is ViewModel.MainWindowViewModel viewModel && !string.IsNullOrEmpty(viewModel.FileName);
		}

		public async void Execute(object? parameter)
		{
			// こういう書き方ができるんだね！
			if (parameter is ViewModel.MainWindowViewModel viewModel)
			{
				await viewModel.Save();
			}
		}

		public void ChangeCanExecute()
		{
			this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

	}
}
