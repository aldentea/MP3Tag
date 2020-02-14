using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Aldentea.MP3Tag.Base
{

	#region ID3Readerクラス
	public class ID3Reader : FileStream
	{
		#region *コンストラクタ(ID3Reader)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		public ID3Reader(string path)
			: base(path, FileMode.Open, FileAccess.Read)
		{
		}
		#endregion

		// 09/03/2013 by aldentea
		// ↓Read3ByteIntegerのコメントになっていたが，Read4ByteSynchsafeIntegerのコメントだと思うので移動．
		// 05/28/2007 by aldente : バグを修正(8を7に修正)．
		#region *4バイトSynchsafe整数を読み込み(Read4ByteSynchsafeInteger)
		/// <summary>
		/// 4バイトのSynchsafe整数を読み込みます．
		/// </summary>
		/// <returns></returns>
		public async Task<int> Read4ByteSynchsafeInteger()
		{
			int digits = 4;
			byte[] buf = new byte[digits];
			await this.ReadAsync(buf, 0, digits);

			int value = 0;
			for (int n = 0; n < digits; n++)
			{
				value += buf[digits - 1 - n] << (7 * n);
			}
			return value;
		}
		#endregion

		#region *3バイト整数を読み込み(Read3ByteInteger)
		/// <summary>
		/// 3バイトのビッグエンディアン整数を読み込みます．
		/// </summary>
		/// <returns>読み込んだ整数値．</returns>
		public async Task<int> Read3ByteInteger()
		{
			int digits = 3;
			byte[] buf = new byte[digits];
			await this.ReadAsync(buf, 0, digits);

			int value = 0;
			for (int n = 0; n < digits; n++)
			{
				value += buf[digits - 1 - n] << (8 * n);
			}
			return value;
		}
		#endregion

		public new async Task<byte> ReadByte()
		{
			return (await ReadBytes(1))[0];
		}

		/// <summary>
		/// 指定されたバイト数を読み込み、配列として返します。
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public async Task<byte[]> ReadBytes(int size)
		{
			byte[] buf = new byte[size];
			await ReadAsync(buf, 0, size);
			return buf;
		}

		/// <summary>
		/// 4バイトを読み取り、リトルエンディアン形式の32ビット整数に変換します。
		/// </summary>
		/// <returns></returns>
		public async Task<int> ReadInt32(bool bigEndian = false)
		{
			byte[] bytes = await ReadBytes(4);
			int le = BitConverter.ToInt32(bytes, 0);
			return bigEndian ? System.Net.IPAddress.HostToNetworkOrder(le) : le;
		}

	}
	#endregion

}
