using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Aldentea.MP3Tag.RIFF.Base
{
	// 03/07/2008 by aldente
	#region [abstract]Chunkクラス
	public abstract class Chunk
	{
		protected FOURCC name = new FOURCC();

		// 03/07/2008 by aldente
		#region *[virtual]Nameプロパティ
		/// <summary>
		/// チャンクの名前を取得／設定します．
		/// </summary>
		public virtual string Name
		{
			get
			{
				return name.Value;
			}
			set
			{
				name.Value = value;
			}
		}
		#endregion

		// 03/10/2008 by aldente
		#region *コンストラクタ(Chunk:1/2)
		/// <summary>
		/// 
		/// </summary>
		protected Chunk(string name)
		{
			this.name.Value = name;
		}
		#endregion

		// 03/10/2008 by aldente
		#region *コンストラクタ(Chunk:2/2)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		//public Chunk(string name, BinaryReader reader, int data_size) : this(name)
		//{
		//	ReadBody(reader, data_size);
		//}
		#endregion

		public async Task Initialize(FileStream reader, int dataSize)
		{
			await ReadBody(reader, dataSize);
		}

		// 03/07/2008 by aldente
		#region *[abstract]データサイズを取得(GetDataSize)
		/// <summary>
		/// データ部分のサイズを取得します．
		/// </summary>
		/// <returns></returns>
		public abstract int GetDataSize();
		#endregion

		// 03/07/2008 by aldente
		#region *チャンク全体のサイズを取得(GetSize)
		/// <summary>
		/// チャンク全体のサイズを取得します．
		/// </summary>
		/// <returns></returns>
		public int GetSize()
		{
			int data_size = GetDataSize();
			return data_size + (((data_size & 1) == 1) ? 9 : 8);
		}
		#endregion


		// 03/07/2008 by aldente
		#region *[abstract]データ部分のバイト列を取得(GetDataBytes)
		/// <summary>
		/// データ部分をバイト列として取得します．
		/// </summary>
		/// <returns></returns>
		public abstract byte[] GetDataBytes();
		#endregion

		// 03/07/2008 by aldente
		#region *チャンク全体をバイト列として取得(GetBytes)
		/// <summary>
		/// チャンク全体をバイト型の配列として取得します．
		/// </summary>
		/// <returns></returns>
		public byte[] GetBytes()
		{
			byte[] buf = new byte[GetSize()];
			name.GetBytes().CopyTo(buf, 0);
			GetDataSizeBytes().CopyTo(buf, 4);
			GetDataBytes().CopyTo(buf, 8);
			return buf;
		}
		#endregion

		// 03/07/2008 by aldente
		#region *データサイズの値をバイト列に変換(GetDataSizeBytes)
		/// <summary>
		/// データ部分のサイズを取得し，リトルエンディアンのバイト列に変換します．
		/// </summary>
		/// <returns></returns>
		private byte[] GetDataSizeBytes()
		{
			int size = GetDataSize();
			return Int32ToBytes(size);
		}
		#endregion

		// 09/03/2013 by aldentea : unsafeじゃない実装にしました．
		// 03/07/2008 by aldente
		#region *[static]32ビット整数値をバイト列に変換(Int32ToBytes)
		/// <summary>
		/// 32ビット整数値を，リトルエンディアンのバイト列に変換します．
		/// </summary>
		/// <param name="src">変換元の整数値．</param>
		/// <returns></returns>
		private static byte[] Int32ToBytes(Int32 src)
		{
			byte[] le_size = BitConverter.GetBytes(src);
			if (BitConverter.IsLittleEndian)
			{
				return le_size;
			}
			else
			{
				// ビッグエンディアン環境！
				Array.Reverse(le_size); // Array.Reverseは破壊的メソッド！
				return le_size;
			}
			//unsafe
			//{
			//  byte* p = (byte*)&src;
			//  for (int i = 0; i < 4; i++)
			//  {
			//    le_size[i] = *p++;
			//  }
			//}
			//return le_size;
		}
		#endregion

		/*
				// 03/11/2008 by aldente : obsolete?
				// 03/10/2008 by aldente
				#region *チャンクを読み込み(Read)
				public void Read(BinaryReader reader)
				{
					// サイズを読み込む．
					//int size = ReadInt32(reader);
					// 本体を読み込む．
					//ReadBody(reader, size);
				}
				#endregion
		*/

		public virtual async Task ReadBody(FileStream reader, int size)
		{
			throw new NotImplementedException("Chunkを継承するクラスは、ReadBodyAsyncメソッドをオーバーライドして下さい。");
		}

		// 03/10/2008 by aldente
		#region *チャンクを書き込み(Write)
		public async Task Write(Stream writer)
		{
			// 識別子を書き込む．
			await writer.WriteAsync(name.GetBytes(), 0, 4);
			// サイズを書き込む．
			await writer.WriteAsync(GetDataSizeBytes(), 0, 4);
			// 中身を書き込む．
			int size = GetDataSize();
			await writer.WriteAsync(GetDataBytes(), 0, size);
			if (size % 2 == 1)
			{
				// パディングを行う．
				await writer.WriteAsync(new byte[] { 0x00 });
			}
		}
		#endregion

	}
	#endregion
}
