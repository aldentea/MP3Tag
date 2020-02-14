using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Aldentea.MP3Tag.RIFF
{
	using Base;

	// 03/12/2008 by aldente
	#region IID3Chunkクラス
	public class IID3Chunk : Chunk
	{
		MP3Tag.Base.ID3v1Tag tag;
		const int id3_data_size = 128;
		public const string ChunkName = "IID3";

		// 03/12/2008 by aldente
		#region *コンストラクタ(IID3Chunk)
		public IID3Chunk(string name)
			: base(name)
		{
			if (name != ChunkName)
			{
				throw new ApplicationException("識別子が'IID3'ぢゃないよ！");
			}
		}
		#endregion

		/*
		// 03/12/2008 by aldente
		#region *コンストラクタ(IID3Chunk)
		public IID3Chunk(string name, BinaryReader reader, int data_size)
			: base(name, reader, data_size)
		{
			// ※上のコンストラクタと同じ処理を書くのは何とか回避できないか．
			if (name != ChunkName)
			{
				throw new ApplicationException("識別子が'IID3'ぢゃないよ！");
			}
		}
		#endregion
	*/

		#region abstract実装

		// 03/12/2008 by aldente
		public override int GetDataSize()
		{
			return id3_data_size;
		}

		// 03/12/2008 by aldente
		public override byte[] GetDataBytes()
		{
			return tag.GetBytes();
		}

		// 03/12/2008 by aldente
		public override async Task ReadBody(FileStream reader, int size)
		{
			tag = await MP3Tag.Base.ID3v1Tag.Read(reader);
		}

		#endregion

		// 03/12/2008 by aldente
		#region *Titleプロパティ
		public string Title
		{
			get
			{
				return tag.Title;
			}
			set
			{
				tag.Title = value;
			}
		}
		#endregion

		// 03/12/2008 by aldente
		#region *Artistプロパティ
		public string Artist
		{
			get
			{
				return tag.Artist;
			}
			set
			{
				tag.Artist = value;
			}
		}
		#endregion

		// 03/12/2008 by aldente
		#region *SabiPosプロパティ
		public decimal SabiPos
		{
			get
			{
				return tag.SabiPos;
			}
			set
			{
				tag.SabiPos = value;
			}
		}
		#endregion

		// 03/12/2008 by aldente
		#region *AlbumNameプロパティ
		public string AlbumName
		{
			get
			{
				return tag.Album_name;
			}
			set
			{
				tag.Album_name = value;
			}
		}
		#endregion

	}
	#endregion
}
