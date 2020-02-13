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

	// 12/25/2007 by aldente : Mergeメソッドを追加．
	#region *[abstract]ID3v2Tagクラス
	public abstract class ID3v2Tag : IID3Tag
	{
		protected static int base_header_size = 10;
		//protected static int frame_name_size;
		protected int frame_name_size;
		protected static bool use_sjis = true;

		protected int size;
		protected BitArray flags;
		protected byte[] extended_header = null;

		#region *CharCodeプロパティ
		/// <summary>
		/// 出力時の文字コードを取得／設定します．
		/// </summary>
		public byte CharCode
		{
			get
			{
				return char_code;
			}
			set
			{
				char_code = value;
			}
		}
		protected byte char_code = 0x00;
		#endregion

		#region IID3実装

		#region *Titleプロパティ
		public string Title
		{
			get
			{
				return FindValue(id_Title);
			}
			set
			{
				AddFrame(id_Title, value);
			}
		}
		#endregion

		#region *Artistプロパティ
		public string Artist
		{
			get
			{
				return FindValue(id_Artist);
			}
			set
			{
				AddFrame(id_Artist, value);
			}
		}
		#endregion

		// 05/22/2007 by aldente
		#region *SabiPosプロパティ
		public decimal SabiPos
		{
			get
			{
				decimal pos = FindEventTimeCodeFrame().SabiPos;
				if (pos == -1.0M)
				{
					return 0.0M;
				}
				else
				{
					return pos;
				}
			}
			set
			{
				FindEventTimeCodeFrame().SabiPos = value;
			}
		}
		#endregion

		// 06/18/2007 by aldente
		#region *StartPosプロパティ
		public decimal StartPos
		{
			get
			{
				decimal pos = FindEventTimeCodeFrame().StartPos;
				if (pos == -1.0M)
				{
					return 0.0M;
				}
				else
				{
					return pos;
				}
			}
			set
			{
				FindEventTimeCodeFrame().StartPos = value;
			}
		}
		#endregion

		// 06/18/2007 by aldente
		#region *StopPosプロパティ
		public decimal StopPos
		{
			get
			{
				decimal pos = FindEventTimeCodeFrame().StopPos;
				if (pos == -1.0M)
				{
					return 0.0M;
				}
				else
				{
					return pos;
				}
			}
			set
			{
				FindEventTimeCodeFrame().StopPos = value;
			}
		}
		#endregion

		#endregion

		protected string id_Title;
		protected string id_Artist;

		#region *Unsynchronisedプロパティ
		protected bool Unsynchronised
		{
			get
			{
				return flags.Get(0);
			}
			set
			{
				flags.Set(0, value);
			}
		}
		#endregion

		// ※他のフラグプロパティは継承先で定義する．

		protected ArrayList frames = new ArrayList();

		#region *[static]コンストラクタ(ID3v2Tag)
		public static void SetCharacterCode(bool enable_sjis)
		{
			use_sjis = enable_sjis;
		}
		#endregion

		protected delegate void InitializeFrameDelegater();

		#region *新規作成用コンストラクタ(ID3v2Tag)
		public ID3v2Tag()
		{
			flags = new BitArray(8);	// falseに初期設定される．
		}
		#endregion

		// 05/17/2007 by aldente : only_header引数を追加．
		#region *コンストラクタ(ID3v2Tag)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="only_header"></param>
		protected ID3v2Tag(ID3Reader reader, bool only_header, int frame_name_size)
		{
			this.frame_name_size = frame_name_size;
			ReadHeader(reader);
			if (!only_header)
			{
				ReadFrames(reader);
			}
		}
		#endregion

		// 05/16/2007 by aldente
		#region *[static]ファイルにID3v2タグが存在するか否か(Exists)
		public static bool Exists(string filename)
		{
			//if (!File.Exists(filename))
			//{
			//  throw new FileNotFoundException();
			//}

			using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open)))
			{
				return Exists(reader);
			}

		}
		#endregion

		// 05/16/2007 by aldente
		#region *[static]ファイルにID3v2タグが存在するか否か(Exists)
		/// <summary>
		/// ファイルにID3v2タグが存在するか否かをチェックします．
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		protected static bool Exists(BinaryReader reader)
		{
			// 先頭3バイトを読み込む．
			reader.BaseStream.Seek(0, SeekOrigin.Begin);
			byte[] buf = reader.ReadBytes(3);
			return (Encoding.ASCII.GetString(buf) == "ID3");
		}
		#endregion

		// 12/25/2007 by aldente
		#region *他のタグとマージ(Merge)
		public void Merge(IID3Tag another_tag)
		{
			if (Artist == string.Empty)
				Artist = another_tag.Artist;
			if (Title == string.Empty)
				Title = another_tag.Title;
			if (SabiPos == 0.0M)
				SabiPos = another_tag.SabiPos;
			if (StartPos == 0.0M)
				StartPos = another_tag.StartPos;
			if (StopPos == 0.0M)
				StopPos = another_tag.StopPos;
		}
		#endregion


		// 05/16/2007 by aldente : virtualに変更(拡張タグを読み込むため)．
		#region *[virtual]ヘッダの後半を読み込み(ReadHeader)
		protected virtual void ReadHeader(ID3Reader reader)
		{
			flags = new BitArray(reader.ReadBytes(1));
			size = reader.Read4ByteSynchsafeInteger();
		}
		#endregion

		#region *全フレーム読み込み(ReadFrames)
		protected void ReadFrames(ID3Reader reader)
		{
			while (reader.BaseStream.Position < size + base_header_size)
			{
				if (!ReadFrame(reader))
				{
					break;
				}
			}
		}
		#endregion

		#region *1つのフレームを読み込み(ReadFrame)
		protected bool ReadFrame(ID3Reader reader)
		{
			// フレーム名を読み込む．
			string name = Encoding.ASCII.GetString(reader.ReadBytes(frame_name_size));
			//string name = Encoding.ASCII.GetString(reader.ReadBytes((int)this.GetType().GetField("frame_name_size", System.Reflection.BindingFlags.FlattenHierarchy).GetValue(null)));
			// "The frame ID made out of the characters capital A-Z and 0-9."
			// なんだけど，半角空白を使う人がいるようなので，一応それにも対応しておく．
			string format = string.Format("[A-Z0-9 ]{{{0}}}", frame_name_size);
			if (!Regex.IsMatch(name, format))
			{
				// おそらくパディング領域に踏み込んだ場合．
				return false;
			}

			AddFrame(name, reader);
			return true;
		}
		#endregion

		protected abstract int AddFrame(string name, ID3Reader reader);

		// 05/17/2007 by aldente
		#region *文字列フレームを追加(AddFrame:2/3)
		/// <summary>
		/// 文字列フレームを追加します．
		/// </summary>
		/// <param name="name">フレームの識別子．</param>
		/// <param name="value">フレームの値．</param>
		/// <returns>タグが持つフレームの数．</returns>
		public int AddFrame(string name, string value)
		{
			IStringFrame frame = FindFrame(name) as IStringFrame;
			if (frame == null)
			{
				// フレームを新規作成．
				//return this.frames.Add(StringFrameClass.GetConstructor(new Type[] { typeof(string), typeof(string) }).Invoke(new string[] { name, value }));
				frame = GenerateStringFrame(name);
				this.frames.Add(frame);
			}

			frame.Value = value;

			return frames.Count;

		}
		#endregion

		// 05/17/2007 by aldente
		#region *バイナリフレームを追加(AddFrame:3/3)
		/// <summary>
		/// バイナリフレームを追加します．
		/// </summary>
		/// <param name="name">フレームの識別子．</param>
		/// <param name="content">フレームの値．</param>
		/// <returns>タグが持つフレームの数．</returns>
		public int UpdateFrame(string name, byte[] content)
		{
			IBinaryFrame frame = FindFrame(name) as IBinaryFrame;
			if (frame == null)
			{
				// フレームを新規作成．
				//return this.frames.Add(BinaryFrameClass.GetConstructor(new Type[] { typeof(string), typeof(byte[]) }).Invoke(new object[] { name, content }));
				frame = GenerateBinaryFrame(name);
				this.frames.Add(frame);
			}

			frame.Content = content;

			return frames.Count;

		}
		#endregion

		// 05/22/2007 by aldente
		#region *イベントタイムコードフレームを追加(FindEventTimeCodeFrame)
		/// <summary>
		/// イベントタイムコードフレームを呼び出します．
		/// 現在のフレームになければ，新規に作成します．
		/// </summary>
		/// <returns>イベントタイムコードフレームオブジェクト．</returns>
		protected IEventTimeCodeFrame FindEventTimeCodeFrame()
		{
			IEventTimeCodeFrame ret_frame;
			for (int i = 0; i < frames.Count; i++)
			{
				ret_frame = frames[i] as IEventTimeCodeFrame;
				if (ret_frame != null)
				{
					return ret_frame;
				}
			}
			// なければ新規作成．
			//ret_frame = (IEventTimeCodeFrame)EventTimeCodeFrameClass.GetConstructor(new Type[] { }).Invoke(new object[] { });
			ret_frame = GenerateEventTimeCodeFrame();
			frames.Add(ret_frame);
			return ret_frame;
		}
		#endregion

		// 09/17/2014 by aldentea
		#region *[static]ファイルからタグ全体のサイズを取得(GetSize)
		public static int GetSize(string filename)
		{
			using (ID3Reader reader = new ID3Reader(File.Open(filename, FileMode.Open)))
			{
				if (!Exists(reader))
				{
					return 0;
				}
				return Generate(reader, true).size;
			}
		}
		#endregion

		// 05/16/2007 by aldente : 未対応バージョンにはApplicationExceptionを発生するように変更．
		#region *[static]ファイルからID3v2タグを読み込む(ReadFile)
		/// <summary>
		/// 指定したファイルのID3v2タグを読み込み，ID3v2Tagオブジェクトを返します．
		/// ID3v2タグが見つからなければnullを返します．
		/// </summary>
		/// <param name="filename">ID3v2を読み込むファイルの名前．</param>
		/// <returns>ID3v2Tagオブジェクト．タグが見つからなければnull．</returns>
		public static ID3v2Tag ReadFile(string filename)
		{
			using (ID3Reader reader = new ID3Reader(File.Open(filename, FileMode.Open)))
			{
				if (!Exists(reader))
				{
					return null;
				}
				return Generate(reader, false);
			}
		}
		#endregion

		// 05/17/2007 by aldente
		#region *タグオブジェクトを生成(Generate)
		/// <summary>
		/// ヘッダのバージョン番号を読み取り，タグオブジェクトを生成します．
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="only_header"></param>
		/// <returns></returns>
		private static ID3v2Tag Generate(ID3Reader reader, bool only_header)
		{
			// readerは"ID3"まで読み取ったものとする．
			byte[] version = reader.ReadBytes(2);
			switch (version[0])
			{
				case 0x02:
					return new ID3v22Tag(reader, only_header);
				case 0x03:
					return new ID3v23Tag(reader, only_header);
				case 0x04:
					throw new ApplicationException("残念ながらまだ未対応ですm(_ _)m");
				//tag = new ID3v23Tag();
				//break;
				default:
					throw new ApplicationException("見たことないバージョンでチュね～");
			}
		}
		#endregion

		#region *指定したフレームの値を取得(FindValue)
		/// <summary>
		/// 指定した名前のテキストフレームを探して，その値を取得します．
		/// </summary>
		/// <param name="name">テキストフレーム識別子．</param>
		/// <returns>フレームの値．見つからなければ空文字列．</returns>
		protected string FindValue(string name)
		{
			if (name[0] != 'T')
			{
				throw new ArgumentException("無効なnameが与えられたにょ．");
			}
			foreach (object frame in frames)
			{
				if (((ID3v2Frame)frame).Name == name)
				{
					return ((IStringFrame)frame).Value;
				}
			}
			return string.Empty;

		}
		#endregion

		// 05/17/2007 by aldente
		#region *指定した名前のフレームを取得(FindFrame)
		/// <summary>
		/// 指定した識別子を持つフレームを取得します．
		/// 見つからなければnullを返します．
		/// ※同名のフレームが複数ある場合はどうなるの？
		/// </summary>
		/// <param name="name">フレームの識別子．</param>
		/// <returns>該当するフレームオブジェクト．</returns>
		protected ID3v2Frame FindFrame(string name)
		{
			for (int i = 0; i < frames.Count; i++)
			{
				ID3v2Frame frame = (ID3v2Frame)frames[i];
				if (frame.Name == name)
				{
					return frame;
				}
			}
			return null;
		}
		#endregion

		// 05/16/2007 by aldente
		#region *タグ全体のサイズを取得(GetTotalSize)
		/// <summary>
		/// ヘッダも含めたタグ全体のサイズを取得します．
		/// </summary>
		/// <returns>タグ全体のサイズ．</returns>
		public int GetTotalSize()
		{
			// タグヘッダから全体のサイズを算出．
			return base_header_size + ((extended_header == null) ? 0 : extended_header.Length) + size;
		}
		#endregion

		#region *タグ全体をバイト列に変換(GetBytes)
		/// <summary>
		/// タグ全体をバイト列としてエンコードしたものを取得します．
		/// </summary>
		/// <returns></returns>
		public byte[] GetBytes(byte code, int padding_size)
		{
			byte[] buf;
			using (MemoryStream ms = new MemoryStream())
			{
				// 仮ヘッダ取得(とりあえず仮のタグサイズを記録しておく)．
				buf = GetHeader(0);
				int header_size = buf.Length;
				ms.Write(buf, 0, header_size);

				// フレーム取得
				for (int i = 0; i < frames.Count; i++)
				{
					// ※とりあえずshift-jisで決め打ち．
					buf = ((ID3v2Frame)frames[i]).GetBytes();
					if (buf != null)
					{
						ms.Write(buf, 0, buf.Length);
					}
				}

				// パディング処理
				int final_size = (((int)ms.Length - 1) / padding_size + 1) * padding_size;
				buf = new byte[final_size];

				// フレーム出力
				ms.ToArray().CopyTo(buf, 0);
				// 真ヘッダ出力
				GetHeader(final_size - header_size).CopyTo(buf, 0);
			}
			return buf;
		}
		#endregion

		// 02/04/2013 by aldentea : 一時ファイルの名前をAPIから取得。
		// 05/17/2007 by aldente
		#region *ファイルに書き込む(WriteTo)
		/// <summary>
		/// 指定したファイルにタグを書き込みます．
		/// 既存のタグは上書きされます．
		/// </summary>
		/// <param name="dstFilename">書き込み先のファイル名．</param>
		public void WriteTo(string dstFilename)
		{
			if (!File.Exists(dstFilename))
			{
				// どうしてくれよう？
			}

			bool v1_exists = ID3v1Tag.Exists(dstFilename);

			//string tempFilename = "namunamu.mp3";
			string tempFilename = Path.GetTempFileName();

			using (ID3Reader reader = new ID3Reader(new FileStream(dstFilename, FileMode.Open)))
			{
				bool exists = Exists(reader);
				int old_tag_size = exists ? Generate(reader, true).GetTotalSize() : 0;

				// 11/10/2008 by aldente
				// FileModeをCreateNewからCreateに変更．
				using (BinaryWriter writer = new BinaryWriter(new FileStream(tempFilename, FileMode.Create)))
				{
					ID3v1Tag v1Tag;
					// タグを書き込む．
					writer.Write(this.GetBytes(this.char_code, 0x100));
					// 本体を書き込む．
					reader.BaseStream.Seek(old_tag_size, SeekOrigin.Begin);
					if (v1_exists)
					{
						writer.Write(reader.ReadBytes((int)reader.BaseStream.Length - old_tag_size - 128));
						v1Tag = new ID3v1Tag(reader, false);
					}
					else
					{
						writer.Write(reader.ReadBytes((int)reader.BaseStream.Length - old_tag_size));
						v1Tag = new ID3v1Tag();
					}
					// ID3v1タグを書き込む．
					v1Tag.Title = this.Title;
					v1Tag.Artist = this.Artist;
					byte[] buf = v1Tag.GetBytes();
					writer.Write(buf, 0, buf.Length);

				}
			}
			File.Delete(dstFilename);
			File.Move(tempFilename, dstFilename);
		}
		#endregion

		// 05/16/2007 by aldente
		#region *synchsafe整数をInt32に変換(ConvertSynchsafeIntToInt32)
		/// <summary>
		/// 整数をsunchsafe整数に変換します．
		/// </summary>
		/// <param name="synchsafe">sunchsafeにしたい整数．</param>
		/// <returns>変換後の整数値．</returns>
		private int ConvertInt32ToSynchsafeInt(int src)
		{
			int ret = 0;
			for (int i = 0; i < 4; i++)
			{
				ret += (src & (0x7F << (7 * i))) << i;
			}
			return ret;
		}
		#endregion

		// 05/16/2007 by aldente
		#region *ヘッダを取得(GetHeader)
		/// <summary>
		/// ヘッダをbyte列として取得します．
		/// </summary>
		/// <returns></returns>
		protected byte[] GetHeader(int main_size)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				ms.Write(Encoding.ASCII.GetBytes("ID3"), 0, 3);
				ms.Write(GetVersion(), 0, 2);
				ms.WriteByte(GetFlags());
				ms.Write(new IPAddress(IPAddress.HostToNetworkOrder(ConvertInt32ToSynchsafeInt(main_size))).GetAddressBytes(), 0, 4);

				// ※拡張ヘッダは当分無視？

				return ms.ToArray();
			}
		}
		#endregion

		protected abstract byte[] GetVersion();

		// 05/16/2007 by aldente
		#region *フラグを取得(GetFlags)
		/// <summary>
		/// フラグをbyte型として取得します．
		/// </summary>
		/// <returns></returns>
		protected byte GetFlags()
		{
			byte ret = 0x00;
			for (int i = 0; i < 8; i++)
			{
				if (flags[i])
				{
					ret += (byte)(0x80 >> i);
				}
			}
			return ret;
		}
		#endregion

		protected abstract IStringFrame GenerateStringFrame(string name);
		protected abstract IBinaryFrame GenerateBinaryFrame(string name);
		protected abstract IEventTimeCodeFrame GenerateEventTimeCodeFrame();

		#region [abstract]ID3v2Frameクラス
		protected abstract class ID3v2Frame
		{
			protected readonly string name;

			#region *Nameプロパティ
			public string Name
			{
				get
				{
					return name;
				}
			}
			#endregion

			#region *コンストラクタ(ID3v2Frame)
			protected ID3v2Frame(string name)
			{
				//InitializeFrameHeader();
				this.name = name;
			}
			#endregion

			protected delegate void GetBodyDelegater(out byte[] body);
			protected abstract void ReadBody(ID3Reader reader, int size);
			public abstract byte[] GetBytes();

		}
		#endregion

	}
	#endregion


}
