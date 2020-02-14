using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using System.Threading.Tasks;
using System.Linq;

namespace Aldentea.MP3Tag.Base
{
	// 12/25/2007 by aldente : Mergeメソッドを追加．
	// 05/15/2007 by aldente : ID3v1.1に対応．
	// 04/02/2007 by aldente
	#region ID3v1Tagクラス
	public class ID3v1Tag : IID3Tag
	{
		//private readonly BinaryReader reader;
		protected string artist = string.Empty;
		protected string title = string.Empty;
		protected string album_name = string.Empty;
		protected int year = 0;
		protected string comment = string.Empty;
		protected byte track_no = 0;
		protected byte genre_no = 0x00;

		#region IID3Tag実装

		#region *Artistプロパティ
		public string Artist
		{
			get
			{
				return artist;
			}
			set
			{
				artist = this.TruncateString(value, 30);
			}
		}
		#endregion

		#region *Titleプロパティ
		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				title = TruncateString(value, 30);
			}
		}
		#endregion

		// 12/25/2007 by aldente : qneのサビ位置形式に対応．
		// ID3v2に合わせて実装だけはしておく．
		/// <summary>
		/// サビ位置を秒単位のdecimalで取得／設定します．
		/// qneでの仕様に合わせて，コメントの先頭部に"%\d+%"(秒数を10進表示したものを'%'で挟む)記録するものとします．
		/// 長めのcommentが記録されているときにサビ位置を設定すると，コメントが切り詰められてしまうので注意してください．
		/// </summary>
		#region *SabiPosプロパティ
		public decimal SabiPos
		{
			get
			{
				// ※"先頭"を表すメタ文字ってなかったっけ(Rubyで言う'\A'みたいなの)？
				// よくわからんので"行頭"で代用．
				Match m = Regex.Match(comment, @"^%(\d+)%");
				if (m.Success)
				{
					return Convert.ToDecimal(m.Groups[1].Value);
				}
				else
				{
					return 0.0M;
				}
			}
			set
			{
				//	// 何もしない．
				int sabi_pos = Decimal.ToInt32(value);
				if (sabi_pos > 0.0M)
				{
					string old_comment = Comment;
					Comment = this.TruncateString(string.Format(@"%{0:\d}%{1}", sabi_pos, old_comment), 30);
				}
			}
		}
		#endregion

		// ID3v2に合わせて実装だけはしておく．
		#region *StartPosプロパティ
		public decimal StartPos
		{
			get
			{
				return 0.0M;
			}
			set
			{
				// 何もしない．
			}
		}
		#endregion

		// ID3v2に合わせて実装だけはしておく．
		#region *StopPosプロパティ
		public decimal StopPos
		{
			get
			{
				return 0.0M;
			}
			set
			{
				// 何もしない．
			}
		}
		#endregion

		#endregion

		#region プロパティ

		#region *Album_nameプロパティ
		public string Album_name
		{
			get
			{
				return album_name;
			}
			set
			{
				album_name = TruncateString(value, 30);
			}
		}
		#endregion

		#region *Yearプロパティ
		public int Year
		{
			get
			{
				return year;
			}
			set
			{
				year = value;
			}
		}
		#endregion

		#region *Commentプロパティ
		public string Comment
		{
			get
			{
				return comment;
			}
			set
			{
				comment = TruncateString(value, 30);
			}
		}
		#endregion

		#region *Track_noプロパティ
		public byte Track_no
		{
			get
			{
				return track_no;
			}
			set
			{
				track_no = value;
			}
		}
		#endregion

		#region *Genre_noプロパティ
		public byte Genre_no
		{
			get
			{
				return genre_no;
			}
			set
			{
				genre_no = value;
			}
		}
		#endregion

		#endregion

		// 05/15/2007 by aldente
		#region *コンストラクタ(ID3v1Tag)
		public ID3v1Tag()
		{
		}
		#endregion

		//#region *コンストラクタ(ID3v1Tag)
		//public ID3v1Tag(BinaryReader reader, bool alreadyReadIdentifier)
		//{
		//	Read(reader, alreadyReadIdentifier);
		//}
		//#endregion

		public async Task Initialize(FileStream reader, bool alreadyReadIdentifier)
		{
			await Read(reader, alreadyReadIdentifier);
		}

		// 05/15/2007 by aldente
		#region *[static]ファイルにID3v1タグが存在するか否か(Exists)
		public static async Task<bool> Exists(string filename)
		{
			//if (!File.Exists(filename))
			//{
			//  throw new FileNotFoundException();
			//}

			using (FileStream reader = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				return await Exists(reader);
			}

		}
		#endregion

		// 05/15/2007 by aldente
		#region *[static]ファイルにID3v1タグが存在するか否か(Exists)
		// 05/15/2007 by aldente
		/// <summary>
		/// ファイルにID3v1タグが存在するか否かをチェックします．
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		protected static async Task<bool> Exists(FileStream reader)
		{
			// 末尾128バイトを読み込む．
			FileInfo info = new FileInfo(reader.Name);
			long size = info.Length;
			if (size < 128)
			{
				return false;
			}
			reader.Seek(size - 128, SeekOrigin.Begin);
			byte[] buf = new byte[3];
			await reader.ReadAsync(buf, 0, 3);
			return (Encoding.ASCII.GetString(buf) == "TAG");
		}
		#endregion

		// 12/25/2007 by aldente
		#region *他のタグとマージ(Merge)
		public void Merge(IID3Tag another_tag)
		{
			if (artist == string.Empty)
				artist = another_tag.Artist;
			if (title == string.Empty)
				title = another_tag.Title;
			if (SabiPos == 0.0M)
				SabiPos = another_tag.SabiPos;
			// 他のプロパティについては保持できないので処理を行わない．
		}
		#endregion

		#region 読み込み関連メソッド

		// 05/15/2007 by aldente
		#region *[static]ファイルからID3v1タグを読み込み(ReadFile)
		/// <summary>
		/// 指定したファイルのID3v1タグを読み込み，ID3v1Tagオブジェクトを返します．
		/// ID3v1タグが見つからなければnullを返します．
		/// </summary>
		/// <param name="filename">ID3v1を読み込むファイルの名前．</param>
		/// <returns>ID3v1Tagオブジェクト．タグが見つからなければnull．</returns>
		public static async Task<ID3v1Tag> ReadFile(string filename)
		{
			using (var reader = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				return await Read(reader);
			}
		}
		#endregion


		#region *[static]ID3v1タグを読み込み(Read)
		public static async Task<ID3v1Tag> Read(FileStream reader)
		{
			if (await Exists(reader))
			{
				var tag = new ID3v1Tag();
				await tag.Initialize(reader, true);
				return tag;
			}
			else
			{
				return null;
			}
		}
		#endregion


		// 04/04/2007 by aldente
		#region *読み込み(Read)
		protected async Task Read(FileStream reader, bool alreadyReadIdentifier)
		{
			if (!alreadyReadIdentifier)
			{
				// 最初の3バイト("TAG")をスキップする．
				reader.Seek(3, SeekOrigin.Current);
			}

			byte[] tag = new byte[125];
			await reader.ReadAsync(tag, 0, 125);
			// title
			title = GetString(tag.Take(30).ToArray());
			// artist
			artist = GetString(tag.Skip(30).Take(30).ToArray());
			// album_name
			album_name = GetString(tag.Skip(60).Take(30).ToArray());
			// year
			var buf = tag.Skip(90).Take(4).ToArray();
			try
			{
				year = Convert.ToInt32(GetString(buf));
			}
			catch (FormatException)
			{
				year = 0;
			}
			// comment
			var comment_buf = tag.Skip(94).Take(30).ToArray();
			if (comment_buf[28] == 0x00 && comment_buf[29] != 0x00)
			{
				// ID3v1.1
				track_no = comment_buf[29];
			}
			comment = GetString(comment_buf);
			genre_no = tag[124];
		}
		#endregion

		// 04/04/2007 by aldente
		#region *バイト列から文字列を取得(GetString)
		protected string GetString(byte[] buf)
		{
			// バイト列から文字列を取り出す．0x00以降は読まない．
			Encoding sjisEncoding = Encoding.GetEncoding("shift-jis");

			int n = 0;
			foreach (byte b in buf)
			{
				if (b == 0x00)
				{
					break;
				}
				n++;
			}

			if (n == 0)
			{
				return string.Empty;
			}

			byte[] newbuf = new byte[n];
			for (int i = 0; i < n; i++)
			{
				newbuf[i] = buf[i];
			}

			return sjisEncoding.GetString(newbuf);
		}
		#endregion

		#endregion

		#region 書き込み関連メソッド

		// 02/04/2013 by aldentea : 一時ファイルの名前をAPIから取得。
		// 05/15/2007 by aldente
		#region *ファイルに書き込む(WriteTo)
		/// <summary>
		/// 指定したファイルにタグを書き込みます．
		/// 既存のタグは上書きされます．
		/// </summary>
		/// <param name="dstFilename">書き込み先のファイル名．</param>
		public async Task WriteTo(string dstFilename)
		{
			if (!File.Exists(dstFilename))
			{
				// どうしてくれよう？
			}

			string tempFilename = Path.GetTempFileName();
			using (var reader = new FileStream(dstFilename, FileMode.Open, FileAccess.Read))
			{
				bool exists = await Exists(reader);

				using (var writer = new FileStream(tempFilename, FileMode.CreateNew, FileAccess.Write))
				{
					reader.Seek(0, SeekOrigin.Begin);
					var contents_size = Convert.ToInt32(reader.Length - (exists ? 128 : 0));
					var contents = new byte[contents_size];
					await reader.ReadAsync(contents, 0, contents_size);
					await writer.WriteAsync(contents, 0, contents_size);
					await writer.WriteAsync(this.GetBytes(), 0, 128);	// 長さは決め打ちで問題ないと信じる。
				}
			}
			File.Delete(dstFilename);
			File.Move(tempFilename, dstFilename);	// このあたり大丈夫かな…Moveに失敗するような状況であればDeleteも失敗すると信じているが。
		}
		#endregion

		// 05/15/2007 by aldente
		#region *バイト列にエンコード(GetBytes)
		/// <summary>
		/// タグの内容をバイト列に出力します．
		/// </summary>
		/// <returns>出力されたバイト列．</returns>
		public byte[] GetBytes()
		{
			// とりあえずsjisで決め打ち．
			Encoding sjisEncoding = Encoding.GetEncoding("shift-jis");

			byte[] content = new byte[128];
			//MemoryStream ms = new MemoryStream(128);

			// "TAG"(識別子)
			Encoding.ASCII.GetBytes("TAG").CopyTo(content, 0);
			//ms.Write(Encoding.ASCII.GetBytes("TAG"), 0, 3);
			// title
			//int n = title.Length;
			//byte[] buf = sjisEncoding.GetByteCount(title);
			sjisEncoding.GetBytes(title).CopyTo(content, 3);

			// artist
			sjisEncoding.GetBytes(artist).CopyTo(content, 33);

			// album_name
			sjisEncoding.GetBytes(album_name).CopyTo(content, 63);

			// year
			if (year > 0)
			{
				Encoding.ASCII.GetBytes(year.ToString()).CopyTo(content, 93);
			}

			// comment
			sjisEncoding.GetBytes(comment).CopyTo(content, 97);

			// track_no
			if (track_no != 0)
			{
				content[125] = 0x00;
				content[126] = track_no;
			}

			// genre_no
			content[127] = genre_no;

			return content;
		}
		#endregion

		// 12/25/2007 by aldente : バイト数を引数で指定するように変更．
		// 05/15/2007 by aldente
		#region *文字列を切り詰める(TruncateString)
		/// <summary>
		/// 文字列を30バイト以内に切り詰めます．
		/// </summary>
		/// <param name="value"></param>
		protected string TruncateString(string value, int max_size)
		{
			Encoding sjisEncoding = Encoding.GetEncoding("shift-jis");

			for (int i = 0; i < value.Length; i++)
			{
				if (sjisEncoding.GetByteCount(value.Substring(0, i)) > max_size)
				{
					return value.Substring(0, i - 1);
				}
			}
			return value;

		}
		#endregion

		#endregion

		// 06/17/2008 by aldente
		#region *Lyrics3Tagクラス
		public class Lyrics3Tag
		{
			// TODO : プロパティ化する．値はfieldsと直接やりとり．
			//protected Lyric lyric = new Lyric();

			// 06/19/2008 by aldente
			#region *Artistプロパティ
			public string Artist
			{
				get
				{
					return fields["EAR"];
				}
				set
				{
					fields["EAR"] = value;
				}
			}
			#endregion

			// 06/19/2008 by aldente
			#region *Titleプロパティ
			public string Title
			{
				get
				{
					return fields["ETT"];
				}
				set
				{
					fields["ETT"] = value;
				}
			}
			#endregion

			// 06/19/2008 by aldente
			#region *AlbumNameプロパティ
			public string AlbumName
			{
				get
				{
					return fields["EAL"];
				}
				set
				{
					fields["EAL"] = value;
				}
			}
			#endregion


			protected Dictionary<string, string> fields = new Dictionary<string, string>();

			#region *コンストラクタ(Lyrics3Tag:1/2)
			public Lyrics3Tag()
			{
			}
			#endregion

			#region *コンストラクタ(Lyrics3Tag:2/2)
			public Lyrics3Tag(BinaryReader reader)
			{

				int size = Exists(reader);
				if (size > 0)
				{
					ReadLyrics3(reader, size);
				}
			}
			#endregion

			// 06/17/2008 by aldente
			#region *[static]ファイルにID3v1タグが存在するか否か(Exists)
			/// <summary>
			/// ファイルにLyrics3タグが存在するか否かをチェックします．
			/// 存在すればそのサイズを，さもなければ0を返します．
			/// </summary>
			/// <param name="reader"></param>
			/// <returns></returns>
			protected static int Exists(BinaryReader reader)
			{
				// 末尾128バイトを読み込む．
				FileInfo info = new FileInfo(((FileStream)reader.BaseStream).Name);
				long size = info.Length;
				if (size < 128 + 9 + 6)
				{
					reader.BaseStream.Seek(size - 128 - 9 - 6, SeekOrigin.Begin);
					byte[] buf = reader.ReadBytes(9 + 6);
					if (Encoding.ASCII.GetString(buf, 6, 9) == "LYRICS200")
					{
						return Convert.ToInt32(Encoding.ASCII.GetString(buf, 0, 6));
					}
				}
				return 0;
			}
			#endregion

			// 06/19/2008 by aldente
			private void ReadLyrics3(BinaryReader reader, int size)
			{
				// ファイルポインタはLyrics3タグの末尾にある．
				reader.BaseStream.Seek(-size - 6 - 9, SeekOrigin.Current);

				byte[] buf = reader.ReadBytes(11);
				if (Encoding.ASCII.GetString(buf) == "LYRICSBEGIN")
				{
					size -= 11;
					// フィールドレコードを読み取る．
					while (size > 0)
					{
						// フィールドIDを読み込む
						string f_name = reader.ReadBytes(3).ToString();
						// データサイズを読み込む
						int data_size = Convert.ToInt32(reader.ReadBytes(5).ToString());
						// データを読み込む
						string data = reader.ReadBytes(data_size).ToString();
						fields[f_name] = data;
						size -= (3 + 5 + size);
					}
				}
			}

		}
		#endregion

	}
	#endregion
}
