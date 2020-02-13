using System;
using System.Collections.Generic;
using System.IO;

namespace Aldentea.MP3Tag.RIFF.Base
{
	// 03/06/2008 by aldente
	#region BinaryChunkクラス
	public class BinaryChunk : Chunk
	{
		byte[] data = new byte[0];

		// 06/11/2014 by aldentea : 2引数のコンストラクタを追加．
		// 06/10/2014 by aldentea : 1引数のコンストラクタを追加．
		#region *コンストラクタ(BinaryChunk)

		public BinaryChunk(string name, BinaryReader reader, int data_size)
			: base(name, reader, data_size)
		{
		}

		public BinaryChunk(string name)
			: base(name)
		{
		}

		public BinaryChunk(string name, byte[] data) : this(name)
		{
			this.data = data;
		}

		#endregion

		#region abstract実装

		// 03/06/2008 by aldente
		#region *[override]データサイズを取得(GetDataSize)
		/// <summary>
		/// データ部分のサイズを取得します．
		/// </summary>
		/// <returns></returns>
		public override int GetDataSize()
		{
			return data.Length;
		}
		#endregion

		// 03/06/2008 by aldente
		#region *[override]データ部分のバイト列を取得(GetDataBytes)
		/// <summary>
		/// データ部分をバイト列として取得します．
		/// </summary>
		/// <returns></returns>
		public override byte[] GetDataBytes()
		{
			return data;
		}
		#endregion

		// 03/10/2008 by aldente
		#region *[override]本体を読み込み(ReadBody)
		protected override void ReadBody(BinaryReader reader, int size)
		{
			data = new byte[size];
			reader.Read(data, 0, size);

			if (size % 2 == 1)
			{
				// パディング分だけ読み取り位置を進める．
				reader.ReadByte();
			}
		}
		#endregion

		#endregion

	}
	#endregion
}
