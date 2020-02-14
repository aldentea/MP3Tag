using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Aldentea.MP3Tag
{
	using Base;
	using RIFF;

	// 01/17/2008 by aldente : static化．
	// 01/08/2008 by aldente
	#region [static]MP3TagAccessorクラス
	public static class MP3TagAccessor
	{

		// 同期実行版。いるかな？

		// 01/08/2008 by aldente : ～Accessorに移植．
		// 05/24/2007 by aldente : static化
		// 05/15/2007 by aldente
		#region *[static]ファイルからID3タグを読み込み(ReadFile)
		/// <summary>
		/// mp3ファイルからID3タグを読み込みます．
		/// タグが見つからなければnullを返します．
		/// </summary>
		/// <param name="filename">読み込むファイル名．</param>
		/// <returns>該当するバージョンのタグオブジェクト．</returns>
		public static async Task<IID3Tag> ReadFile(string fileName)
		{
			IID3Tag tag;
			//SongInfo info = new SongInfo();

			if (!File.Exists(fileName))
			{
				// ファイルがないぞ！
				return null;
			}

			// 拡張子が"rmp"の場合は，RIFFとして開く．
			if (Path.GetExtension(fileName).ToLower().EndsWith("rmp"))
			{
				return await RIFFMP3Tag.ReadFromFileAsync(fileName) as RIFFMP3Tag;
			}
			else
			{
				// ID3v2をチェック．
				tag = await ID3v2Tag.ReadFile(fileName);
				ID3v1Tag tag1 = await ID3v1Tag.ReadFile(fileName);
				if (tag != null)
				{
					if (tag1 != null)
					{
						tag.Merge(tag1);
					}
					return tag;
				}
				return tag1;
			}
		}
		#endregion


		// 11/25/2014 by aldentea : 2引数版のバグ(保存がなされていなかった)を修正．
		// 09/03/2013 by aldentea : 2引数版を用意．
		// 01/17/2008 by aldente : ～Accessorに移植．
		// 05/24/2007 by aldente : static化
		// 05/23/2007 by aldente
		#region *[static]ID3タグを書き込み(UpdateInfo)
		//public static void UpdateInfo(SongInfo info, string filename, byte charCode)
		/// <summary>
		/// ID3タグに曲情報を書き込みます．今のところID3v2.3で決め打ちです．
		/// </summary>
		/// <param name="title">曲のタイトル．</param>
		/// <param name="artist">曲のアーティスト．</param>
		/// <param name="sabipos">曲のサビ位置(秒)．</param>
		/// <param name="startpos">曲の再生開始位置(秒)．</param>
		/// <param name="stoppos">曲の停止位置(秒)．</param>
		/// <param name="filename">タグを書き込むmp3ファイル名．</param>
		/// <param name="charCode">文字コードを指定するbyte型数値．現在未使用？</param>
		public static async Task UpdateInfo(string title, string artist, decimal sabipos, decimal startpos, decimal stoppos, string filename, byte charCode)
		{
			IID3Tag tag = await ReadFile(filename);
			if (tag == null)
			{
				tag = new ID3v23Tag();
			}
			tag.Title = title;
			tag.Artist = artist;
			tag.SabiPos = sabipos;
			tag.StartPos = startpos;
			tag.StopPos = stoppos;

			await tag.WriteTo(filename);
		}



		/// <summary>
		/// 指定したタグの情報を指定したファイルに書き込みます．
		/// ※いまのところ，タイトル・アーティスト・サビ位置のみを書き込んでいます．
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="newTag"></param>
		public static async Task UpdateInfo(string fileName, IID3Tag newTag)
		{
			var original_tag = await MP3TagAccessor.ReadFile(fileName);
			if (original_tag == null)
			{
				await newTag.WriteTo(fileName);
			}
			else
			{
				// 元のタグに新しいタグの情報を書き込んで保存する．
				// 全てを更新していいのか？
				original_tag.Title = newTag.Title;
				original_tag.Artist = newTag.Artist;
				original_tag.SabiPos = newTag.SabiPos;
				//tag.StartPos = startpos;
				//tag.StopPos = stoppos;
				await original_tag.WriteTo(fileName);
			}

		}

		#endregion


		// 09/17/2014 by aldentea
		#region *[static]冒頭にあるタグのサイズを取得(GetHeaderTagSize)
		/// <summary>
		/// ファイルの冒頭にあるタグのサイズを返します．
		/// ID3v1のようにファイルの末尾にあるものはカウントしません．
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static async Task<int> GetHeaderTagSize(string fileName)
		{
			// 拡張子が"rmp"の場合は，RIFFとして開く．
			if (Path.GetExtension(fileName).ToLower().EndsWith("rmp"))
			{
				// ※こちらではHM001の問題が発生するかどうかわからないので，
				// とりあえず0を返しておく．
				return 0;
				//return RIFFMP3Tag.ReadFromFile(fileName) as RIFFMP3Tag;
			}
			else
			{
				return await ID3v2Tag.GetSize(fileName);
			}
		}
		#endregion

	}
	#endregion
}
