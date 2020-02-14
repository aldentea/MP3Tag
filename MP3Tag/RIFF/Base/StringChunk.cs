using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace Aldentea.MP3Tag.RIFF.Base
{
	// 03/06/2008 by aldente
	#region StringChunkクラス
	public class StringChunk : Chunk
	{
		string data = string.Empty;

		public StringChunk(string name)
			: base(name)
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

		// とりあえずShift-JISで固定．
		Encoding my_encoding = Encoding.GetEncoding("Shift-JIS");

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

		// (1.0.0)非同期処理用メソッド。
		// 03/10/2008 by aldente
		#region *[override]本体を読み込み(ReadBody)

		public override async Task ReadBody(FileStream reader, int size)
		{
			int adjusted_size = size % 2 == 1 ? size + 1 : size;
			byte[] buf = new byte[adjusted_size];
			await reader.ReadAsync(buf, 0, adjusted_size);
			data = my_encoding.GetString(buf).TrimEnd('\0');
		}

		#endregion

		#endregion

	}
	#endregion
}
