//Copyright (c) 2012 Josip Medved <jmedved@jmedved.com>

//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

//Source URLs: https://www.medo64.com/2012/04/pbkdf2-with-sha-256-and-others/ and https://www.medo64.com/license/

using System;
using System.Security.Cryptography;
using System.Text;

namespace ProjectHaystack.Util
{

  /// <summary>
  /// Generic PBKDF2 implementation.
  /// </summary>
  /// <example>This sample shows how to initialize class with SHA-256 HMAC.
  /// <code>
  /// using (var hmac = new HMACSHA256()) {
  ///     var df = new Pbkdf2(hmac, "password", "salt");
  ///     var bytes = df.GetBytes();
  /// }
  /// </code>
  /// </example>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pbkdf", Justification = "Spelling is correct.")]
  public class Pbkdf2
  {

    /// <summary>
    /// Creates new instance.
    /// </summary>
    /// <param name="algorithm">HMAC algorithm to Use.</param>
    /// <param name="password">The password used to derive the key.</param>
    /// <param name="salt">The key salt used to derive the key.</param>
    /// <param name="iterations">The number of iterations for the operation.</param>
    /// <exception cref="System.ArgumentNullException">Algorithm cannot be null - Password cannot be null. -or- Salt cannot be null.</exception>
    public Pbkdf2(HMAC algorithm, byte[] password, byte[] salt, Int32 iterations)
    {
      if (algorithm == null)
      {
        throw new ArgumentNullException("algorithm", "Algorithm cannot be null.");
      }
      if (salt == null)
      {
        throw new ArgumentNullException("salt", "Salt cannot be null.");
      }
      if (password == null)
      {
        throw new ArgumentNullException("password", "Password cannot be null.");
      }
      this.Algorithm = algorithm;
      this.Algorithm.Key = password;
      this.Salt = salt;
      this.IterationCount = iterations;
      this.BlockSize = this.Algorithm.HashSize / 8;
      this.BufferBytes = new byte[this.BlockSize];
    }

    /// <summary>
    /// Creates new instance.
    /// </summary>
    /// <param name="algorithm">HMAC algorithm to Use.</param>
    /// <param name="password">The password used to derive the key.</param>
    /// <param name="salt">The key salt used to derive the key.</param>
    /// <exception cref="System.ArgumentNullException">Algorithm cannot be null - Password cannot be null. -or- Salt cannot be null.</exception>
    public Pbkdf2(HMAC algorithm, Byte[] password, Byte[] salt)
      : this(algorithm, password, salt, 1000)
    {
    }

    /// <summary>
    /// Creates new instance.
    /// </summary>
    /// <param name="algorithm">HMAC algorithm to Use.</param>
    /// <param name="password">The password used to derive the key.</param>
    /// <param name="salt">The key salt used to derive the key.</param>
    /// <param name="iterations">The number of iterations for the operation.</param>
    /// <exception cref="System.ArgumentNullException">Algorithm cannot be null - Password cannot be null. -or- Salt cannot be null.</exception>
    public Pbkdf2(HMAC algorithm, String password, String salt, Int32 iterations)
      : this(algorithm, Encoding.UTF8.GetBytes(password), (byte[])(Array)Base64.STANDARD.DecodeBytes(salt), iterations)
    {
    }

    /// <summary>
    /// Creates new instance.
    /// </summary>
    /// <param name="algorithm">HMAC algorithm to Use.</param>
    /// <param name="password">The password used to derive the key.</param>
    /// <param name="salt">The key salt used to derive the key.</param>
    /// <exception cref="System.ArgumentNullException">Algorithm cannot be null - Password cannot be null. -or- Salt cannot be null.</exception>
    public Pbkdf2(HMAC algorithm, String password, String salt)
      : this(algorithm, password, salt, 1000)
    {
    }


    private readonly int BlockSize;
    private uint BlockIndex = 1;

    private byte[] BufferBytes;
    private int BufferStartIndex = 0;
    private int BufferEndIndex = 0;


    /// <summary>
    /// Gets algorithm used for generating key.
    /// </summary>
    public HMAC Algorithm { get; private set; }

    /// <summary>
    /// Gets salt bytes.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Byte array is proper return value in this case.")]
    public Byte[] Salt { get; private set; }

    /// <summary>
    /// Gets iteration count.
    /// </summary>
    public Int32 IterationCount { get; private set; }


    /// <summary>
    /// Returns a pseudo-random key from a password, salt and iteration count.
    /// </summary>
    /// <param name="count">Number of bytes to return.</param>
    /// <returns>Byte array.</returns>
    public sbyte[] GetBytes(int count)
    {
      byte[] result = new byte[count];
      int resultOffset = 0;
      int bufferCount = this.BufferEndIndex - this.BufferStartIndex;

      if (bufferCount > 0)
      { //if there is some data in buffer
        if (count < bufferCount)
        { //if there is enough data in buffer
          Buffer.BlockCopy(this.BufferBytes, this.BufferStartIndex, result, 0, count);
          this.BufferStartIndex += count;
          sbyte[] sresult = Array.ConvertAll(result, b => unchecked((sbyte)b));
          return sresult;
        }
        Buffer.BlockCopy(this.BufferBytes, this.BufferStartIndex, result, 0, bufferCount);
        this.BufferStartIndex = this.BufferEndIndex = 0;
        resultOffset += bufferCount;
      }

      while (resultOffset < count)
      {
        int needCount = count - resultOffset;
        this.BufferBytes = this.Func();
        if (needCount > this.BlockSize)
        { //we one (or more) additional passes
          Buffer.BlockCopy(this.BufferBytes, 0, result, resultOffset, this.BlockSize);
          resultOffset += this.BlockSize;
        }
        else
        {
          Buffer.BlockCopy(this.BufferBytes, 0, result, resultOffset, needCount);
          this.BufferStartIndex = needCount;
          this.BufferEndIndex = this.BlockSize;
          sbyte[] sresult = Array.ConvertAll(result, b => unchecked((sbyte)b));
          return sresult;
        }
      }
      sbyte[] sresult1 = Array.ConvertAll(result, b => unchecked((sbyte)b));
      return sresult1;
    }


    private byte[] Func()
    {
      var hash1Input = new byte[this.Salt.Length + 4];
      Buffer.BlockCopy(this.Salt, 0, hash1Input, 0, this.Salt.Length);
      Buffer.BlockCopy(GetBytesFromInt(this.BlockIndex), 0, hash1Input, this.Salt.Length, 4);
      var hash1 = this.Algorithm.ComputeHash(hash1Input);

      byte[] finalHash = hash1;
      for (int i = 2; i <= this.IterationCount; i++)
      {
        hash1 = this.Algorithm.ComputeHash(hash1, 0, hash1.Length);
        for (int j = 0; j < this.BlockSize; j++)
        {
          finalHash[j] = (byte)(finalHash[j] ^ hash1[j]);
        }
      }
      if (this.BlockIndex == uint.MaxValue)
      {
        throw new InvalidOperationException("Derived key too long.");
      }
      this.BlockIndex += 1;

      return finalHash;
    }

    private static byte[] GetBytesFromInt(uint i)
    {
      var bytes = BitConverter.GetBytes(i);
      if (BitConverter.IsLittleEndian)
      {
        return new byte[] { bytes[3], bytes[2], bytes[1], bytes[0] };
      }
      else
      {
        return bytes;
      }
    }

  }
}
