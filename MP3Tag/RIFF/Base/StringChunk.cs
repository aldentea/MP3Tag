using System;
using System.IO;
using System.Text;

namespace Aldentea.MP3Tag.RIFF.Base
{
	// 03/06/2008 by aldente
	#region StringChunkクラス
	public class StringChunk : Chunk
	{
		string data = string.Empty;
		// とりあえずShift-JISで固定．
		Encoding my_encoding = Encoding.GetEncoding("Shift-JIS");

		public StringChunk(string name, BinaryReader reader, int data_size)
			: base(name, reader, data_size)
		{
		}

		// 03/07/2008 by aldente
		#region *Encodingプロパティ
		/// <summary>
		/// 使用する文字コードを取得／設定します．
		/// </summary>
		public Encoding Encoding
		{
			get
			{
				return my_encoding;
			}
			set
			{
				my_encoding = value;
			}
		}
		#endregion

		// 03/10/2008 by aldente
		#region *Valueプロパティ
		public string Value
		{
			get
			{
				return data;
			}
			set
			{
				data = value;
			}
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
			return my_encoding.GetByteCount(data);
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
			return my_encoding.GetBytes(data);
		}
		#endregion

		// 03/10/2008 by aldente
		#region *[override]本体を読み込み(ReadBody)
		protected override void ReadBody(BinaryReader reader, int size)
		{
			byte[] buf = new byte[size];
			reader.Read(buf, 0, size);
			data = my_encoding.GetString(buf).TrimEnd('\0');

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
