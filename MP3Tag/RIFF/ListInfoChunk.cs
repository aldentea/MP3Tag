using System;
using System.Collections.Generic;
using System.IO;

namespace Aldentea.MP3Tag.RIFF
{
	using Base;

	// 03/10/2008 by aldente
	#region ListInfoChunkクラス
	public class ListInfoChunk : ListChunk
	{
		const string title_chunk_name = "INAM";
		const string artist_chunk_name = "IART";
		public const string DataType = "INFO";

		// 03/10/2008 by aldente
		#region *TitleChunkプロパティ
		protected StringChunk TitleChunk
		{
			get
			{
				return FindChunk(title_chunk_name) as StringChunk;
			}
		}
		#endregion

		// 03/10/2008 by aldente
		#region *ArtistChunkプロパティ
		protected StringChunk ArtistChunk
		{
			get
			{
				return FindChunk(artist_chunk_name) as StringChunk;
			}
		}
		#endregion

		// 03/10/2008 by aldente
		#region *Titleプロパティ
		public string Title
		{
			get
			{
				return TitleChunk.Value;
			}
			set
			{
				TitleChunk.Value = value;
			}
		}
		#endregion

		// 03/10/2008 by aldente
		#region *Artistプロパティ
		public string Artist
		{
			get
			{
				return ArtistChunk.Value;
			}
			set
			{
				ArtistChunk.Value = value;
			}
		}
		#endregion

		// 03/11/2008 by aldente
		public ListInfoChunk()
			: base(DataType)
		{
		}

		// 03/11/2008 by aldente
		public ListInfoChunk(string data_type)
			: base(data_type)
		{
		}

		// 03/11/2008 by aldente
		public ListInfoChunk(BinaryReader reader, int data_size)
			: base(DataType, reader, data_size)
		{
		}

		// 03/11/2008 by aldente
		public ListInfoChunk(string data_type, BinaryReader reader, int data_size)
			: base(data_type, reader, data_size)
		{
		}


		// 03/10/2008 by aldente
		protected override Type GetChunkType(string id)
		{
			switch (id)
			{
				case title_chunk_name:
				case artist_chunk_name:
					return typeof(StringChunk);
				case IID3Chunk.ChunkName:
					return typeof(IID3Chunk);
				default:
					return typeof(BinaryChunk);
			}
		}

	}

	#endregion

}
