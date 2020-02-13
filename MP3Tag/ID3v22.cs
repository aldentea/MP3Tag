using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;	// for BitArray
using System.Net;	// エンディアン変換のため．
using System.IO;
using System.Text.RegularExpressions;

namespace Aldentea.MP3Tag
{
	using Base;

	#region ID3v22Tagクラス
	public class ID3v22Tag : ID3v2Tag
	{
		// 05/17/2007 by aldente
		#region *新規作成用コンストラクタ(ID3v22Tag)
		public ID3v22Tag()
			: base()
		{
			frame_name_size = 3;
			id_Title = "TT2";
			id_Artist = "TP1";
		}
		#endregion

		#region *コンストラクタ(ID3v22Tag)
		public ID3v22Tag(ID3Reader reader, bool only_header)
			: base(reader, only_header, 3)
		{
			id_Title = "TT2";
			id_Artist = "TP1";
		}
		#endregion

		#region *[override]フレームを追加(AddFrame)
		/// <summary>
		/// タグにフレームを追加します．
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		protected override int AddFrame(string name, ID3Reader reader)
		{
			// イベントタイムコードフレーム
			if (name == "ETC")
			{
				return frames.Add(new ID3v22EventTimeCodeFrame(name, reader));
			}
			// テキストフレーム
			if (name[0] == 'T')
			{
				if (name != "TXX")
				{
					return frames.Add(new ID3v22StringFrame(name, reader, true));
				}
				else
				{
					return 0;
				}
			}
			else
			{
				return frames.Add(new ID3v22BinaryFrame(name, reader));
			}
		}
		#endregion

		// 05/16/2007 by aldente
		#region *[override]バージョンをbyte配列で取得(GetVersion)
		/// <summary>
		/// バージョン番号をbyte配列として取得します．
		/// </summary>
		/// <returns>バージョン番号のbyte配列．</returns>
		protected override byte[] GetVersion()
		{
			return new byte[] { 0x02, 0x00 };
		}
		#endregion

		protected override IStringFrame GenerateStringFrame(string name)
		{
			return new ID3v22StringFrame(name);
		}

		protected override IBinaryFrame GenerateBinaryFrame(string name)
		{
			return new ID3v22BinaryFrame(name);
		}

		protected override IEventTimeCodeFrame GenerateEventTimeCodeFrame()
		{
			return new ID3v22EventTimeCodeFrame("ETC", TimeUnit.Milliseconds);
		}

		#region [abstract]ID3v22Frameクラス
		abstract class ID3v22Frame : ID3v2Frame
		{
			#region *コンストラクタ(ID3v22Frame:1/2)
			public ID3v22Frame(string name)
				: base(name)
			{
			}
			#endregion

			#region *コンストラクタ(ID3v22Frame:2/2)
			public ID3v22Frame(string name, ID3Reader reader)
				: base(name)
			{
				// サイズ読み込み
				int size = reader.Read3ByteInteger();
				// フラグ読み込みは，フラグがないので割愛．
				// 本体読み込み
				ReadBody(reader, size);
			}
			#endregion

			//protected abstract void ReadBody(ID3Reader reader, int size);

			//public abstract byte[] GetBytes();

			// 06/18/2007 by aldente : 本体の長さが0のフレームは出力しないように変更．
			#region *フレームをバイト列化して取得(GetBytes)
			protected byte[] GetBytes(GetBodyDelegater getBody)
			{
				// 本体のバイト列を取得．
				byte[] body;
				getBody(out body);
				if (body.Length == 0)
				{
					// 本体の長さが0のフレームは出力しない．
					return null;
				}

				using (MemoryStream ms = new MemoryStream())
				{
					// ヘッダ出力
					int size = body.Length;
					byte[] buf = GetHeaderBytes(size);
					ms.Write(buf, 0, buf.Length);

					// 本体出力
					ms.Write(body, 0, size);
					return ms.ToArray();
				}
			}
			#endregion


			// 05/15/2007 by aldente
			#region *フレームヘッダをバイト列にエンコード(GetHeaderBytes)
			/// <summary>
			/// フレームヘッダ(の識別子以降)をバイト列に変換します．
			/// </summary>
			/// <param name="size">フレーム本体のサイズ．</param>
			/// <returns>変換したバイト列．</returns>
			public /*override*/ byte[] GetHeaderBytes(int frame_size)
			{
				using (MemoryStream ms = new MemoryStream())
				{
					// 識別名出力
					ms.Write(Encoding.ASCII.GetBytes(Name), 0, Name.Length);

					// サイズ出力
					ms.Write(SizeToBytes(frame_size), 0, 3);

					// フラグ出力
					// ※フラグなんてないから気にしない～．
					//byte s_byte = 0;
					//byte f_byte = 0;
					//if (s_flags[0]) s_byte += 0x80;
					//if (s_flags[1]) s_byte += 0x40;
					//if (s_flags[2]) s_byte += 0x20;
					//if (f_flags[0]) f_byte += 0x80;
					//if (f_flags[1]) f_byte += 0x40;
					//if (f_flags[2]) f_byte += 0x20;

					//ms.WriteByte(s_byte);
					//ms.WriteByte(f_byte);

					return ms.ToArray();
				}
			}
			#endregion

			// 05/15/2007 by aldente
			#region *サイズをbyte配列に変換する(SizeToByte)
			protected /*override*/ byte[] SizeToBytes(int frame_size)
			{
				//byte[] buf = new byte[4];
				//buf[0] = (byte)(frame_size / 0x1000000);
				//buf[1] = (byte)((frame_size / 0x10000) % 0x100);
				//buf[2] = (byte)((frame_size / 0x100) % 0x100);
				//buf[3] = (byte)(frame_size % 0x100);

				byte[] buf = new byte[3];
				buf[0] = (byte)(frame_size / 0x10000);
				buf[1] = (byte)((frame_size / 0x100) % 0x100);
				buf[2] = (byte)(frame_size % 0x100);

				return buf;
			}
			#endregion

		}
		#endregion

		#region ID3v22BinaryFrameクラス
		class ID3v22BinaryFrame : ID3v22Frame, IBinaryFrame
		{
			// ※v23とほとんど同じ．

			#region IBinaryFrame実装
			protected byte[] content;
			public byte[] Content
			{
				get
				{
					return content;
				}
				set
				{
					content = value;
				}
			}
			#endregion

			#region 初期化関連

			#region *コンストラクタ(ID3v22BinaryFrame:1/2)
			public ID3v22BinaryFrame(string name)
				: base(name)
			{
			}
			#endregion

			#region *コンストラクタ(ID3v22BinaryFrame:2/2)
			public ID3v22BinaryFrame(string name, ID3Reader reader)
				: base(name, reader)
			{
			}
			#endregion

			#region *[override]フレームの本体を読み込み(ReadBody)
			protected override void ReadBody(ID3Reader reader, int size)
			{
				content = reader.ReadBytes(size);
			}
			#endregion

			#endregion

			#region *[override]フレームをバイト列として出力(GetBytes)
			/// <summary>
			/// フレームをバイト列として出力します．
			/// </summary>
			/// <returns>フレームをバイト列化したもの．</returns>
			public override byte[] GetBytes()
			{
				return GetBytes(delegate(out byte[] body) { body = content; });
			}
			#endregion
		}
		#endregion

		#region ID3v22StringFrameクラス
		class ID3v22StringFrame : ID3v22Frame, IStringFrame
		{
			// ※v23とほとんど同じ！

			#region IStringFrame実装
			public string value;
			public string Value
			{
				get
				{
					return this.value;
				}
				set
				{
					this.value = value;
				}
			}

			StringFrameEncoder encoder;
			#endregion

			#region 初期化関連

			#region *コンストラクタ(ID3v22StringFrame:1/2)
			public ID3v22StringFrame(string name)
				: base(name)
			{
				encoder = new StringFrameEncoder(use_sjis);
			}
			#endregion

			#region *コンストラクタ(ID3v22StringFrame:2/2)
			public ID3v22StringFrame(string name, ID3Reader reader, bool use_sjis)
				: base(name, reader)
			{
			}
			#endregion

			#region *[override]フレームの本体を読み込み(ReadBody)
			protected override void ReadBody(ID3Reader reader, int size)
			{
				encoder = new StringFrameEncoder(use_sjis);
				value = encoder.Decode(reader.ReadBytes(size));
			}
			#endregion

			#endregion

			// 06/18/2007 by aldente : 文字列が空の場合は何も出力しないように変更．
			#region *[override]フレームをバイト列として出力(GetBytes)
			/// <summary>
			/// フレームをバイト列として出力します．
			/// </summary>
			/// <returns>フレームをバイト列化したもの．</returns>
			public override byte[] GetBytes()
			{
				return GetBytes(delegate(out byte[] body)
				{
					if (Value == string.Empty) { body = new byte[0]; }
					else { body = encoder.Encode(Value, Encoding.Unicode); }
				});
			}
			#endregion

		}
		#endregion

		#region ID3v22EventTimeCodeFrameクラス
		class ID3v22EventTimeCodeFrame : ID3v22Frame, IEventTimeCodeFrame
		{
			// ※v23とほとんど同じ！

			protected EventTimeCodeCollection event_time_collection = new EventTimeCodeCollection();
			protected ArrayList event_time_units = new ArrayList();

			#region IEventCodeTimeFrame実装

			#region *TimeStampUnitプロパティ
			public TimeUnit TimeStampUnit
			{
				get
				{
					return event_time_collection.TimeStampUnit;
				}
				set
				{
					event_time_collection.TimeStampUnit = value;
				}
			}
			#endregion

			#region *SabiPosプロパティ
			/// <summary>
			/// サビの開始位置を取得／設定します．単位は秒です．
			/// </summary>
			public decimal SabiPos
			{
				get
				{
					return event_time_collection.SabiPos;
				}
				set
				{
					event_time_collection.SabiPos = value;
				}
			}
			#endregion

			// 06/22/2007 by aldente
			#region *StartPosプロパティ
			/// <summary>
			/// イントロの開始位置を取得／設定します．単位は秒です．
			/// </summary>
			public decimal StartPos
			{
				get
				{
					return event_time_collection.StartPos;
				}
				set
				{
					event_time_collection.StartPos = value;
				}
			}
			#endregion

			// 06/22/2007 by aldente
			#region *StopPosプロパティ
			/// <summary>
			/// イントロの停止位置を取得／設定します．単位は秒です．
			/// </summary>
			public decimal StopPos
			{
				get
				{
					return event_time_collection.StopPos;
				}
				set
				{
					event_time_collection.StopPos = value;
				}
			}
			#endregion

			// 11/10/2008 by aldente
			#region *イベントを追加(AddEvent)
			public void AddEvent(byte type, decimal time)
			{
				event_time_collection.AddEvent(type, time);
				//event_time_collection.AddEvent(type, Convert.ToInt32(Decimal.Truncate(time * 1000)));
			}
			#endregion

			// 11/10/2008 by aldente
			#region *イベントを更新(UpdateEvent)
			public void UpdateEvent(byte type, decimal time)
			{
				event_time_collection.UpdateEvent(type, time);
				//event_time_collection.UpdateEvent(type, Convert.ToInt32(Decimal.Truncate(time * 1000)));
			}
			#endregion

			#region *イベントの時刻を取得(GetTime)
			public decimal GetTime(byte type)
			{
				return event_time_collection.GetTime(type);
			}
			#endregion


			#endregion

			#region *コンストラクタ(ID3v22EventFrame:1/2)
			public ID3v22EventTimeCodeFrame(string name, TimeUnit timeUnit)
				: base(name)
			{
				InitializeTimeUnits();
				this.TimeStampUnit = timeUnit;
			}
			#endregion

			#region *コンストラクタ(ID3v22EventFrame:2/2)
			public ID3v22EventTimeCodeFrame(string name, ID3Reader reader)
				: base(name, reader)
			{
			}
			#endregion

			protected void InitializeTimeUnits()
			{
				event_time_units.Add(null);
				event_time_units.Add(TimeUnit.Frames);
				event_time_units.Add(TimeUnit.Milliseconds);
			}

			#region *[override]本体を読み込み(ReadBody)
			protected override void ReadBody(ID3Reader reader, int size)
			{
				InitializeTimeUnits();
				this.TimeStampUnit = (TimeUnit)event_time_units[reader.ReadByte()];

				event_time_collection.ReadBody(reader, size - 1);
			}
			#endregion

			#region *[override]フレームをバイト列として出力(GetBytes)
			/// <summary>
			/// フレームをバイト列として出力します．
			/// </summary>
			/// <returns>フレームをバイト列化したもの．</returns>
			public override byte[] GetBytes()
			{
				return GetBytes(delegate(out byte[] body) { body = GetBodyBytes(); });
			}
			#endregion

			// 06/18/2007 by aldente : イベントがなければ何も返さないように変更．
			protected byte[] GetBodyBytes()
			{
				if (event_time_collection.Count == 0)
				{
					return new byte[0];
				}
				else
				{
					using (MemoryStream ms = new MemoryStream())
					{
						byte time_unit_code = Convert.ToByte(event_time_units.IndexOf(TimeStampUnit));
						// time_unit_code == -1 なんてありえないですよね？
						ms.WriteByte(time_unit_code);

						byte[] buf = event_time_collection.GetBytes();
						ms.Write(buf, 0, buf.Length);

						return ms.ToArray();
					}
				}
			}

		}
		#endregion

	}
	#endregion
}
