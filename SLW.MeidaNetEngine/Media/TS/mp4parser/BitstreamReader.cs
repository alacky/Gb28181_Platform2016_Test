/*
Copyright (c) 2011 Stanislav Vitvitskiy

Permission is hereby granted, free of charge, to any person obtaining a copy of this
software and associated documentation files (the "Software"), to deal in the Software
without restriction, including without limitation the rights to use, copy, modify,
merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System.IO;
namespace com.googlecode.mp4parser.h264.read
{


    /// <summary>
    /// A dummy implementation of H264 RBSP reading
    /// 
    /// @author Stanislav Vitvitskiy
    /// </summary>
    public class BitstreamReader
	{
		private Stream @is;
		private int curByte;
		private int nextByte;
		internal int nBit;
		protected internal static int bitsRead;

		protected internal CharCache debugBits = new CharCache(50);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public BitstreamReader(java.io.Stream is) throws java.io.IOException
		public BitstreamReader(Stream @is)
		{
			this.@is = @is;
			curByte = @is.ReadByte();
			nextByte = @is.ReadByte();
		}

		/*
		  * (non-Javadoc)
		  *
		  * @see ua.org.jplayer.javcodec.h264.RBSPStream#read1Bit()
		  */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read1Bit() throws java.io.IOException
		public virtual int read1Bit()
		{
			if (nBit == 8)
			{
				advance();
				if (curByte == -1)
				{
					return -1;
				}
			}
			int res = (curByte >> (7 - nBit)) & 1;
			nBit++;

			debugBits.append(res == 0 ? '0' : '1');
			++bitsRead;

			return res;
		}

		/*
		  * (non-Javadoc)
		  *
		  * @see ua.org.jplayer.javcodec.h264.RBSPStream#readNBit(int)
		  */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public long readNBit(int n) throws java.io.IOException
		public virtual long readNBit(int n)
		{
			if (n > 64)
			{
				throw new System.ArgumentException("Can not readByte more then 64 bit");
			}

			long val = 0;

			for (int i = 0; i < n; i++)
			{
				val <<= 1;
				val |= read1Bit();
			}

			return val;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void advance() throws java.io.IOException
		private void advance()
		{
			curByte = nextByte;
			nextByte = @is.ReadByte();
			nBit = 0;
		}

		/*
		  * (non-Javadoc)
		  *
		  * @see ua.org.jplayer.javcodec.h264.RBSPStream#readByte()
		  */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readByte() throws java.io.IOException
		public virtual int readByte()
		{
			if (nBit > 0)
			{
				advance();
			}

			int res = curByte;

			advance();

			return res;
		}

		/*
		  * (non-Javadoc)
		  *
		  * @see ua.org.jplayer.javcodec.h264.RBSPStream#moreRBSPData()
		  */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean moreRBSPData() throws java.io.IOException
		public virtual bool moreRBSPData()
		{
			if (nBit == 8)
			{
				advance();
			}
			int tail = 1 << (8 - nBit - 1);
			int mask = ((tail << 1) - 1);
			bool hasTail = (curByte & mask) == tail;

			return !(curByte == -1 || (nextByte == -1 && hasTail));
		}

		public virtual long BitPosition
		{
			get
			{
				return (bitsRead * 8 + (nBit % 8));
			}
		}

		/*
		  * (non-Javadoc)
		  *
		  * @see ua.org.jplayer.javcodec.h264.RBSPStream#readRemainingByte()
		  */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public long readRemainingByte() throws java.io.IOException
		public virtual long readRemainingByte()
		{
			return readNBit(8 - nBit);
		}

		/*
		  * (non-Javadoc)
		  *
		  * @see ua.org.jplayer.javcodec.h264.RBSPStream#next_bits(int)
		  */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int peakNextBits(int n) throws java.io.IOException
		public virtual int peakNextBits(int n)
		{
			if (n > 8)
			{
				throw new System.ArgumentException("N should be less then 8");
			}
			if (nBit == 8)
			{
				advance();
				if (curByte == -1)
				{
					return -1;
				}
			}
			int[] bits = new int[16 - nBit];

			int cnt = 0;
			for (int i = nBit; i < 8; i++)
			{
				bits[cnt++] = (curByte >> (7 - i)) & 0x1;
			}

			for (int i = 0; i < 8; i++)
			{
				bits[cnt++] = (nextByte >> (7 - i)) & 0x1;
			}

			int result = 0;
			for (int i = 0; i < n; i++)
			{
				result <<= 1;
				result |= bits[i];
			}

			return result;
		}

		/*
		  * (non-Javadoc)
		  *
		  * @see ua.org.jplayer.javcodec.h264.RBSPStream#byte_aligned()
		  */
		public virtual bool ByteAligned
		{
			get
			{
				return (nBit % 8) == 0;
			}
		}

		/*
		  * (non-Javadoc)
		  *
		  * @see ua.org.jplayer.javcodec.h264.RBSPStream#close()
		  */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
		public virtual void close()
		{
		}

		public virtual int CurBit
		{
			get
			{
				return nBit;
			}
		}
	}
}