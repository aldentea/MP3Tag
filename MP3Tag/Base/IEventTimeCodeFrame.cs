using System;
using System.Collections.Generic;
using System.Text;

namespace Aldentea.MP3Tag.Base
{

	#region IEventTimeCodeFrameインターフェイス
	public interface IEventTimeCodeFrame
	{
		TimeUnit TimeStampUnit { get; set; }
		decimal SabiPos { get; set; }
		decimal StartPos { get; set; }
		decimal StopPos { get; set; }
		void AddEvent(byte type, decimal time);
		void UpdateEvent(byte type, decimal time);
		decimal GetTime(byte type);

	}
	#endregion

	#region TimeUnit列挙体
	public enum TimeUnit
	{
		Frames,
		Milliseconds
	}
	#endregion

}
