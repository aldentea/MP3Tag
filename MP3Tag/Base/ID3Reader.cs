using System;
using System.Collections.Generic;
using System.IO;

namespace Aldentea.MP3Tag.Base
{

	#region ID3Readerクラス
	public class ID3Reader : BinaryReader
	{
		#region *コンストラクタ(ID3Reader)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		public ID3Reader(Stream input)
			: base(input)
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
		public int Read4ByteSynchsafeInteger()
		{
			int digits = 4;
			byte[] buf = ReadBytes(digits);
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
		public int Read3ByteInteger()
		{
			int digits = 3;
			byte[] buf = ReadBytes(digits);
			int value = 0;
			for (int n = 0; n < digits; n++)
			{
				value += buf[digits - 1 - n] << (8 * n);
			}
			return value;
		}
		#endregion

	}
	#endregion

}
