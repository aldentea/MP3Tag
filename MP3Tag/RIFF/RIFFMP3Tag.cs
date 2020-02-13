using System;
using System.Collections.Generic;
using System.IO;

namespace Aldentea.MP3Tag.RIFF
{
	using Base;

	// 03/10/2008 by aldente
	#region RIFFMP3Tagクラス
	public class RIFFMP3Tag : RIFFChunk, MP3Tag.Base.IID3Tag
	{
		public const string DataType = "RMP3";

		// 03/10/2008 by aldente
		#region *InfoChunkプロパティ
		public ListInfoChunk InfoChunk
		{
			get
			{
				return FindChunk(ListInfoChunk.DataType) as ListInfoChunk;
			}
		}
		#endregion

		// 03/14/2008 by aldente
		#region *IID3Chunkプロパティ
		public IID3Chunk IID3Chunk
		{
			get
			{
				if (InfoChunk != null)
				{
					return InfoChunk.FindChunk(IID3Chunk.ChunkName) as IID3Chunk;
				}
				else
				{
					return null;
				}
			}
		}
		#endregion

		public RIFFMP3Tag(BinaryReader reader) : base(DataType, reader)
		{
		}

		// 03/10/2008 by aldente
		public new static RIFFMP3Tag ReadFromFile(string srcFileName)
		{
			using (BinaryReader reader = new BinaryReader(new FileStream(srcFileName, FileMode.Open)))
			{
				return new RIFFMP3Tag(reader);
			}
		}

		// 03/11/2008 by aldente
		protected override Type GetListChunkType(string type_name)
		{
			switch (type_name)
			{
				case ListInfoChunk.DataType:
					return typeof(ListInfoChunk);
				default:
					return typeof(ListChunk);
			}
		}

		#region *IID3Tag実装

		// 03/11/2008 by aldente : InfoTitleChunkを用いて書き換え．
		// 03/10/2008 by aldente
		#region *Titleプロパティ
		public string Title
		{
			get
			{
				return InfoChunk.Title;
			}
			set
			{
				InfoChunk.Title = value;
				if (IID3Chunk != null)
				{
					IID3Chunk.Title = value;
				}
			}
		}
		#endregion

		// 03/11/2008 by aldente : InfoArtistChunkを用いて書き換え．
		// 03/10/2008 by aldente
		#region *Artistプロパティ
		public string Artist
		{
			get
			{
				return InfoChunk.Artist;
			}
			set
			{
				InfoChunk.Artist = value;
				if (IID3Chunk != null)
				{
					IID3Chunk.Artist = value;
				}
			}
		}
		#endregion

		// ※このあたりはまだちゃんと実装していません！

		// 02/27/2008 by aldente
		#region *SabiPosプロパティ
		public decimal SabiPos
		{
			get
			{
				return 0.0M;
			}
			set
			{
				//throw new NotImplementedException("Just a moment.");
			}
		}
		#endregion

		// 02/27/2008 by aldente
		#region *StartPosプロパティ
		public decimal StartPos
		{
			get
			{
				return 0.0M;
			}
			set
			{
				//throw new NotImplementedException("Just a moment.");
			}
		}
		#endregion

		// 02/27/2008 by aldente
		#region *StopPosプロパティ
		public decimal StopPos
		{
			get
			{
				return 0.0M;
			}
			set
			{
				//throw new NotImplementedException("Just a moment.");
			}
		}
		#endregion

		// 03/10/2008 by aldente
		public void WriteTo(string dstFilename)
		{
			WriteToFile(dstFilename);
		}

		public void Merge(MP3Tag.Base.IID3Tag another_tag)
		{
			throw new NotImplementedException("Just a moment.");
		}

		#endregion

	}
	#endregion

}
