using System;
using System.Collections.Generic;
using System.Text;

namespace Aldentea.MP3Tag.Base
{
	// 12/25/2007 by aldente : Mergeメソッドを追加．
	// 05/23/2007 by aldente : WriteTo()を追加．
	// 05/15/2007 by aldente
	#region IID3Tagインターフェイス
	public interface IID3Tag
	{
		string Title { get; set; }
		string Artist { get; set; }
		decimal SabiPos { get; set; }
		decimal StartPos { get; set; }
		decimal StopPos { get; set; }

		void WriteTo(string dstFilename);
		void Merge(IID3Tag another_tag);
	}
	#endregion
}
