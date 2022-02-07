using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Aldentea.MP3Tag.Checker
{
	internal class LoadCommand : ICommand
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
				await viewModel.Load();
			}
		}

		public void ChangeCanExecute()
		{
			this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

	}
}
