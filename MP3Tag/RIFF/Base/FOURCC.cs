using System;
using System.Collections.Generic;
using System.Text;

namespace Aldentea.MP3Tag.RIFF.Base
{
	// 03/06/2008 by aldente
	#region FOURCCクラス
	public class FOURCC
	{
		string body = string.Empty;
		static System.Text.RegularExpressions.Regex NameConvertion = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9 ]{0,4}$");

		// 03/06/2008 by aldente
		#region *Valueプロパティ
		public string Value
		{
			get
			{
				return body;
			}
			set
			{
				if (NameConvertion.IsMatch(value))
				{
					body = value.PadRight(4, ' ');
				}
				else
				{
					throw new ArgumentException("ASCII4文字以内なのだ！");
				}
			}
		}
		#endregion
		// 03/06/2008 by aldente
		#region *[override]文字列に変換(ToString)
		/// <summary>
		/// 中身を文字列として取得します．Valueプロパティで取得するのと同じです．
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Value;
		}
		#endregion

		// 03/06/2008 by aldente
		#region *バイト列として取得(GetBytes)
		/// <summary>
		/// 中の文字列を，長さ4のバイト配列として取得します．
		/// 本体が4文字に満たない場合は，右側に0x20が埋められます．
		/// </summary>
		/// <returns></returns>
		public byte[] GetBytes()
		{
			byte[] arrBody = new byte[4] { 0x20, 0x20, 0x20, 0x20 };
			Encoding.ASCII.GetBytes(body, 0, 4, arrBody, 0);
			return arrBody;
		}
		#endregion
	}
	#endregion
}
