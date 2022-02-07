using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aldentea.MP3Tag.Checker.ViewModel
{
	internal class MainWindowViewModel : INotifyPropertyChanged
	{

		#region *FileNameプロパティ
		public string FileName
		{
			get { return _fileName; }
			set
			{
				if (_fileName != value)
				{
					_fileName = value;
					NotifyPropertyChanged();
					LoadCommand.ChangeCanExecute();
					SaveCommand.ChangeCanExecute();
				}
			}
		}
		string _fileName = string.Empty;
		#endregion

		#region *Titleプロパティ
		public string Title
		{
			get { return _title; }
			set
			{
				if (value != _title)
				{
					_title = value;
					NotifyPropertyChanged();
				}
			}
		}
		string _title = string.Empty;
		#endregion

		#region *Artistプロパティ
		public string Artist
		{
			get { return _artist; }
			set
			{
				if (_artist != value)
				{
					_artist = value;
					NotifyPropertyChanged();
				}
			}
		}
		string _artist = string.Empty;
		#endregion

		public LoadCommand LoadCommand { get; private set; }

		public SaveCommand SaveCommand { get; private set; }


		public MainWindowViewModel()
		{
			LoadCommand = new LoadCommand();
			SaveCommand = new SaveCommand();
			NotifyPropertyChanged("LoadCommand");
			NotifyPropertyChanged("SaveCommand");
		}



		public event PropertyChangedEventHandler? PropertyChanged = delegate { };

		protected void NotifyPropertyChanged([CallerMemberNameAttribute] string propertyName = "")
		{
			if (!string.IsNullOrEmpty(propertyName))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}


		public async Task Load()
		{
			if (!string.IsNullOrEmpty(this.FileName))
			{
				var tag = await MP3TagAccessor.ReadFile(FileName);
				this.Title = tag.Title;
				this.Artist = tag.Artist;
			}
		}

		public async Task Save()
		{
			if (!string.IsNullOrEmpty(this.FileName))
			{
				var tag = await MP3TagAccessor.ReadFile(FileName);
				tag.Title = this.Title;
				tag.Artist = this.Artist;
				await MP3TagAccessor.UpdateInfo(this.FileName, tag);
			}
		}
	}
}
