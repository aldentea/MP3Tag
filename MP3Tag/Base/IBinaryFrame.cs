using System;
using System.Collections.Generic;
using System.Text;

namespace Aldentea.MP3Tag.Base
{

	#region IBinaryFrameインターフェイス
	public interface IBinaryFrame
	{
		byte[] Content { get; set; }
	}
	#endregion
}
