using System;
using System.Text;
using System.IO;
using System.Collections;

namespace Aldentea.MP3Tag.Base
{

	#region StringFrameEncoderクラス
	public class StringFrameEncoder
	{
		//readonly bool use_sjis;
		ArrayList encodings = new ArrayList();

		#region *コンストラクタ(StringFrameEncoder)
		public StringFrameEncoder(bool use_sjis)
		{
			//this.use_sjis = use_sjis;
			if (use_sjis)
			{
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				encodings.Add(Encoding.GetEncoding("Shift_JIS"));
			}
			//else
			//{
			//	encodings.Add(Encoding.GetEncoding("Latin-1"));
			//}

			encodings.Add(Encoding.Unicode);
		}
		#endregion

		#region *バイト列を文字列に変換(Decode)
		public string Decode(byte[] body)
		{
			int offset = 1;
			Encoding encoding = (Encoding)encodings[body[0]];
			if (encoding == Encoding.Unicode && body[1] == 0xFF && body[2] == 0xFE)
			{
				// BOMをスキップ．
				offset = 3;
			}
			return encoding.GetString(body, offset, body.Length - offset).TrimEnd('\0');
		}
		#endregion

		// 11/25/2014 by aldentea : BOMの出力処理を追加．
		#region *文字列をバイト列に変換(Encode)
		public byte[] Encode(string value, Encoding encoding)
		{
			int index = encodings.IndexOf(encoding);
			if (index == -1)
			{
				throw new ArgumentException("その文字コードは取り扱っておりません．");
			}

			using (MemoryStream ms = new MemoryStream())
			{
				ms.WriteByte(Convert.ToByte(index));
				// この2行でBOMの出力を行う．
				byte[] bombuf = encoding.GetPreamble();
				ms.Write(bombuf, 0, bombuf.Length);

				byte[] buf = encoding.GetBytes(value + "\0"); // この時にBOMも出力される．←これは嘘．
				ms.Write(buf, 0, buf.Length);
				return ms.ToArray();
			}
		}
		#endregion

	}
	#endregion
}
