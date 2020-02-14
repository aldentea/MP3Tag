using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;	// for BitArray
using System.Net;	// エンディアン変換のため．
using System.IO;
using System.Threading.Tasks;

namespace Aldentea.MP3Tag
{
	using Base;

	#region ID3v23Tagクラス
	public class ID3v23Tag : ID3v2Tag
	{
		#region *HaveExtendHeaderプロパティ
		/// <summary>
		/// 拡張ヘッダを持つならtrue，持たなければfalseを返します．
		/// </summary>
		protected bool HaveExtendedHeader
		{
			get
			{
				return flags.Get(1);
			}
		}
		#endregion

		// 05/17/2007 by aldente
		#region *新規作成用コンストラクタ(ID3v23Tag)
		public ID3v23Tag()
			: base(4)
		{
			id_Title = "TIT2";
			id_Artist = "TPE1";
		}
		#endregion

		#region *コンストラクタ(ID3v23Tag)
		//public ID3v23Tag(ID3Reader reader, bool only_header)
		//	: base(reader, only_header, 4)
		//{

		//	id_Title = "TIT2";
		//	id_Artist = "TPE1";
		//}
		#endregion

		// 05/16/2007 by aldente
		#region *[override]ヘッダの後半を読み込み(ReadHeader)
		protected override async Task ReadHeader(ID3Reader reader)
		{
			await base.ReadHeader(reader);
			if (HaveExtendedHeader)
			{
				// ※拡張ヘッダを読み込む．
				int size = await reader.ReadInt32(true);
				extended_header = await reader.ReadBytes(size);
			}

		}
		#endregion

		#region *[override]フレームを追加(AddFrame)
		/// <summary>
		/// タグにフレームを追加します．
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		protected override async Task<int> AddFrame(string name, ID3Reader reader)
		{
			// イベントタイムコードフレーム
			if (name == "ETCO")
			{
				// とりあえずTimeUnitを決め打ちにする。
				var tag = new ID3v23EventTimeCodeFrame(name, TimeUnit.Milliseconds);
				await tag.Initialize(reader);
				return frames.Add(tag);
			}
			// テキストフレーム
			if (name[0] == 'T')
			{
				if (name != "TXXX")
				{
					var tag = new ID3v23StringFrame(name, true);
					await tag.Initialize(reader);
					return frames.Add(tag);
				}
				else
				{
					return 0;
				}
			}
			else
			{
				var tag = new ID3v23BinaryFrame(name);
				await tag.Initialize(reader);
				return frames.Add(tag);
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
			return new byte[] { 0x03, 0x00 };
		}
		#endregion

		protected override IStringFrame GenerateStringFrame(string name)
		{
			return new ID3v23StringFrame(name, use_sjis);
		}

		protected override IBinaryFrame GenerateBinaryFrame(string name)
		{
			return new ID3v23BinaryFrame(name);
		}

		protected override IEventTimeCodeFrame GenerateEventTimeCodeFrame()
		{
			return new ID3v23EventTimeCodeFrame("ETCO", TimeUnit.Milliseconds);
		}

		#region ID3v23Frameクラス
		abstract class ID3v23Frame : ID3v2Frame
		{
			#region フラグ関連

			protected BitArray s_flags;
			protected BitArray f_flags;

			#region *DestroyOnUpdatingTagプロパティ
			public bool DestroyOnUpdatingTag
			{
				get
				{
					return s_flags.Get(0);
				}
				set
				{
					s_flags.Set(0, value);
				}
			}
			#endregion

			#region *DestroyOnUpdatingFileプロパティ
			public bool DestroyOnUpdatingFile
			{
				get
				{
					return s_flags.Get(1);
				}
				set
				{
					s_flags.Set(1, value);
				}
			}
			#endregion

			#region *ReadOnlyプロパティ
			public bool ReadOnly
			{
				get
				{
					return s_flags.Get(2);
				}
				set
				{
					s_flags.Set(2, value);
				}
			}
			#endregion

			#region *Compressedプロパティ
			public bool Compressed
			{
				get
				{
					return f_flags.Get(0);
				}
				set
				{
					f_flags.Set(0, value);
				}
			}
			#endregion

			#region *Encryptedプロパティ
			public bool Encrypted
			{
				get
				{
					return f_flags.Get(1);
				}
				set
				{
					f_flags.Set(1, value);
				}
			}
			#endregion

			#region *Groupedプロパティ
			public bool Grouped
			{
				get
				{
					return f_flags.Get(2);
				}
				set
				{
					f_flags.Set(2, value);
				}
			}
			#endregion

			#endregion

			#region 初期化関連メソッド

			#region *コンストラクタ(ID3v23Frame:1/2)
			public ID3v23Frame(string name)
				: base(name)
			{
				// フラグ初期化
				s_flags = new BitArray(8);
				s_flags.SetAll(false);
				f_flags = new BitArray(8);
				f_flags.SetAll(false);
			}
			#endregion
/*
			#region *コンストラクタ(ID3v23Frame:2/2)
			public ID3v23Frame(string name, ID3Reader reader)
				: base(name)
			{
				// サイズ読み込み→エンディアン変換
				int size = IPAddress.NetworkToHostOrder(reader.ReadInt32());
				// フラグ読み込み
				s_flags = new BitArray(reader.ReadBytes(1));
				f_flags = new BitArray(reader.ReadBytes(1));
				// 本体読み込み
				ReadBody(reader, size);
			}
			#endregion
*/
			// ↑のコンストラクタを廃止して、Initializeメソッドにする。

			public async Task Initialize(ID3Reader reader)
			{
				// サイズ読み込み→エンディアン変換
				int size = await reader.ReadInt32(true);
				// フラグ読み込み
				s_flags = new BitArray(await reader.ReadBytes(1));
				f_flags = new BitArray(await reader.ReadBytes(1));
				// 本体読み込み
				await ReadBody(reader, size);

			}


			//protected abstract void ReadBody(ID3Reader reader, int size);

			#endregion

			#region 出力関連メソッド

			// 06/18/2007 by aldente : bodyが空であれば空配列を返すように変更．
			#region *フレームをバイト列として出力(GetBytes)
			/// <summary>
			/// フレームをバイト列として出力します．
			/// </summary>
			/// <param name="getBody">本体を出力するためのメソッド．</param>
			/// <returns>フレームをバイト列化したもの．</returns>
			protected byte[] GetBytes(GetBodyDelegater getBody)
			{
				// ※v22,v23で共通．

				// 本体のバイト列を取得．
				byte[] body;
				getBody(out body);
				if (body.Length == 0)
				{
					return new byte[0];
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
					ms.Write(SizeToBytes(frame_size), 0, 4);

					// フラグ出力
					byte s_byte = 0;
					byte f_byte = 0;
					if (s_flags[0]) s_byte += 0x80;
					if (s_flags[1]) s_byte += 0x40;
					if (s_flags[2]) s_byte += 0x20;
					if (f_flags[0]) f_byte += 0x80;
					if (f_flags[1]) f_byte += 0x40;
					if (f_flags[2]) f_byte += 0x20;

					ms.WriteByte(s_byte);
					ms.WriteByte(f_byte);

					return ms.ToArray();
				}
			}
			#endregion

			// 05/15/2007 by aldente
			#region *サイズをbyte配列に変換する(SizeToByte)
			protected /*override*/ byte[] SizeToBytes(int frame_size)
			{
				byte[] buf = new byte[4];
				buf[0] = (byte)(frame_size / 0x1000000);
				buf[1] = (byte)((frame_size / 0x10000) % 0x100);
				buf[2] = (byte)((frame_size / 0x100) % 0x100);
				buf[3] = (byte)(frame_size % 0x100);
				return buf;
			}
			#endregion

			#endregion

		}
		#endregion

		#region ID3v23BinaryFrameクラス
		class ID3v23BinaryFrame : ID3v23Frame, IBinaryFrame
		{
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

			#region *コンストラクタ(ID3v23BinaryFrame:1/2)
			public ID3v23BinaryFrame(string name)
				: base(name)
			{
			}
			#endregion

			#region *コンストラクタ(ID3v23BinaryFrame:2/2)
			//public ID3v23BinaryFrame(string name, ID3Reader reader)
			//	: base(name, reader)
			//{
			//}
			#endregion

			#region *[override]フレームの本体を読み込み(ReadBody)
			protected override async Task ReadBody(ID3Reader reader, int size)
			{
				content = await reader.ReadBytes(size);
			}
			#endregion

			#endregion

			#region 出力関連メソッド

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

			#endregion

		}
		#endregion

		#region ID3v23StringFrameクラス
		class ID3v23StringFrame : ID3v23Frame, IStringFrame
		{
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

			#region 初期化関連メソッド

			#region *コンストラクタ(ID3v23StringFrame:1/2)
			public ID3v23StringFrame(string name, bool use_sjis)
				: base(name)
			{
				encoder = new StringFrameEncoder(use_sjis);
			}
			#endregion

			#region *コンストラクタ(ID3v23StringFrame:2/2)
			//public ID3v23StringFrame(string name, ID3Reader reader, bool use_sjis)
			//	: base(name, reader)
			//{
			//}
			#endregion

			#region *[override]フレームの本体を読み込み(ReadBody)
			protected override async Task ReadBody(ID3Reader reader, int size)
			{
				encoder = new StringFrameEncoder(use_sjis);
				value = encoder.Decode(await reader.ReadBytes(size));
			}
			#endregion

			#endregion

			#region 出力関連メソッド

			// 06/18/2007 by aldente : 値が空文字列であれば，空配列を返すように変更．
			#region *[override]フレームをバイト列として出力(GetBytes)
			/// <summary>
			/// フレームをバイト列として出力します．
			/// </summary>
			/// <returns>フレームをバイト列化したもの．</returns>
			public override byte[] GetBytes()
			{
				if (Value == string.Empty)
				{
					return new byte[0];
				}
				else
				{
					return GetBytes(delegate(out byte[] body) { body = encoder.Encode(value, Encoding.Unicode); });
				}
			}
			#endregion

			#endregion

		}
		#endregion

		#region ID3v23EventTimeCodeFrameクラス
		class ID3v23EventTimeCodeFrame : ID3v23Frame, IEventTimeCodeFrame
		{
			protected EventTimeCodeCollection event_time_collection = new EventTimeCodeCollection();
			protected ArrayList event_time_units = new ArrayList();

			#region IEventCodeTimeFrame実装

			#region TimeStampUnitプロパティ
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

			#region *イベントを追加(AddEvent)
			public void AddEvent(byte type, decimal time)
			{
				event_time_collection.AddEvent(type, time);
			}
			#endregion

			#region *イベントを更新(UpdateEvent)
			public void UpdateEvent(byte type, decimal time)
			{
				event_time_collection.UpdateEvent(type, time);
			}
			#endregion

			#region *イベントの時刻を取得(GetTime)
			public decimal GetTime(byte type)
			{
				return event_time_collection.GetTime(type);
			}
			#endregion


			#endregion

			#region 初期化関連メソッド

			#region *コンストラクタ(ID3v23EventFrame:1/2)
			public ID3v23EventTimeCodeFrame(string name, TimeUnit timeUnit)
				: base(name)
			{
				InitializeTimeUnits();
				this.TimeStampUnit = timeUnit;
			}
			#endregion

			#region *コンストラクタ(ID3v23EventFrame:2/2)
			//public ID3v23EventTimeCodeFrame(string name, ID3Reader reader)
			//	: base(name, reader)
			//{
			//}
			#endregion

			#region *時間の単位を初期化(InitializeTimeUnits)
			protected void InitializeTimeUnits()
			{
				event_time_units.Add(null);
				event_time_units.Add(TimeUnit.Frames);
				event_time_units.Add(TimeUnit.Milliseconds);
			}
			#endregion

			#region *[override]本体を読み込み(ReadBody)
			protected override async Task ReadBody(ID3Reader reader, int size)
			{
				InitializeTimeUnits();
				this.TimeStampUnit = (TimeUnit)event_time_units[await reader.ReadByte()];

				await event_time_collection.ReadBody(reader, size - 1);
			}
			#endregion

			#endregion

			#region 出力関連メソッド

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

			// 06/18/2007 by aldente : イベントがなければ空配列を返すように変更．
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
			#endregion

		}
		#endregion

	}
	#endregion

}
