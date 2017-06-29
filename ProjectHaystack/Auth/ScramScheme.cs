//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using ProjectHaystack.Util;

namespace ProjectHaystack.Auth
{
  using Base64 = ProjectHaystack.Util.Base64;
  using CryptoUtil = ProjectHaystack.Util.Pbkdf2;


  /// <summary>
  /// ScramScheme implements the salted challenge response authentication
  /// mechanism as defined in <a href="https://tools.ietf.org/html/rfc5802">RFC 5802</a>
  /// </summary>
  public sealed class ScramScheme : AuthScheme
  {
    public ScramScheme()
      : base("scram")
    {
    }

    public override AuthMsg OnClient(AuthClientContext cx, AuthMsg msg)
    {
      return ReferenceEquals(msg.Param("data", false), null) ? FirstMsg(cx, msg) : FinalMsg(cx, msg);
    }

    private AuthMsg FirstMsg(AuthClientContext cx, AuthMsg msg)
    {
      // construct client-first-message
      string c_nonce = GenNonce();
      string c1_bare = "n=" + cx.user + ",r=" + c_nonce;
      string c1_msg = gs2_header + c1_bare;

      // stash for final msg
      cx.stash["c_nonce"] = c_nonce;
      cx.stash["c1_bare"] = c1_bare;

      // build auth msg
      SortedDictionary<string, string> @params = new SortedDictionary<string, string>();
      @params["data"] = Base64.URI.EncodeUtf8(c1_msg);
      return new AuthMsg(name, InjectHandshakeToken(msg, @params));
    }

    private AuthMsg FinalMsg(AuthClientContext cx, AuthMsg msg)
    {
      // Decode server-first-message
      string s1_msg = Base64.URI.decodeUTF8(msg.Param("data"));
      IDictionary data = DecodeMsg(s1_msg);

      // c2-no-proof
      string cbind_input = gs2_header;
      string channel_binding = Base64.URI.EncodeUtf8(cbind_input);
      string nonce = (string) data["r"];
      string c2_no_proof = "c=" + channel_binding + ",r=" + nonce;

      // proof
      string hash = msg.Param("hash");
      string salt = (string) data["s"];
      int iterations = int.Parse((string) data["i"]);
      string c1_bare = (string) cx.stash["c1_bare"];
      string authMsg = c1_bare + "," + s1_msg + "," + c2_no_proof;

      string c2_msg = null;
      try
      {
        sbyte[] saltedPassword = Pbk(hash, cx.pass, salt, iterations);
        string clientProof = CreateClientProof(hash, saltedPassword, (sbyte[]) (Array) Encoding.UTF8.GetBytes(authMsg));
        c2_msg = c2_no_proof + ",p= " + clientProof;
      }
      catch (Exception e)
      {
        throw new AuthException("Failed to compute scram", e);
      }

      // build auth msg
      SortedDictionary<string, string> @params = new SortedDictionary<string, string>();
      @params["data"] = Base64.URI.EncodeUtf8(c2_msg);
      return new AuthMsg(name, InjectHandshakeToken(msg, @params));
    }

    /// <summary>
    /// Generate a random nonce string </summary>
    private string GenNonce()
    {
      byte[] bytes = new byte[clientNonceBytes];
      RandomNumberGenerator rng = new RNGCryptoServiceProvider();
      rng.GetNonZeroBytes(bytes);
      byte[] unsigned = bytes;
      return Base64.URI.EncodeBytes(unsigned);
    }

    /// <summary>
    /// If the msg contains a handshake token, inject it into the given params </summary>
    private static SortedDictionary<string, string> InjectHandshakeToken(AuthMsg msg,
      SortedDictionary<string, string> @params)
    {
      string tok = msg.Param("handshakeToken", false);
      if (tok != null)
      {
        @params["handshakeToken"] = tok;
      }
      return @params;
    }

    /// <summary>
    /// Decode a raw scram message </summary>
    private static IDictionary DecodeMsg(string s)
    {
      IDictionary data = new Hashtable();
      string[] toks = s.Split(',');
      for (int i = 0; i < toks.Length; ++i)
      {
        string tok = toks[i];
        int n = tok.IndexOf('=');
        if (n < 0)
        {
          continue;
        }
        string key = tok.Substring(0, n);
        string val = tok.Substring(n + 1);
        data[key] = val;
      }
      return data;
    }

    private static sbyte[] Pbk(string hash, string password, string salt, int iterations)
    {
      try
      {
        byte[] saltBytes = Base64.STANDARD.DecodeBytes(salt);
        using (var hmac = new HMACSHA256())
        {
          var mine = new Pbkdf2(hmac, System.Text.Encoding.UTF8.GetBytes(password),
            (byte[]) (Array) Base64.STANDARD.DecodeBytes(salt), iterations);
          sbyte[] signed = mine.GetBytes(32);
          byte[] signednew = (byte[]) (Array) signed;
          return signed;
        }
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    private static int KeyBits(string hash)
    {
      if ("SHA-1".Equals(hash))
      {
        return 160;
      }
      if ("SHA-256".Equals(hash))
      {
        return 256;
      }
      if ("SHA-512".Equals(hash))
      {
        return 512;
      }
      throw new System.ArgumentException("Unsupported hash function: " + hash);
    }

    private static string CreateClientProof(string hash, sbyte[] saltedPassword, sbyte[] authMsg)
    {
      try
      {
        using (var hmac = new HMACSHA256())
        {
          byte[] usSaltedPassword = (byte[]) (Array) saltedPassword;
          byte[] usAuthMsg = (byte[]) (Array) authMsg;
          var hmac2 = new HMACSHA256(usSaltedPassword);
          sbyte[] clientKey = (sbyte[]) (Array) hmac2.ComputeHash(Encoding.UTF8.GetBytes("Client Key"));
          byte[] usClientKey = (byte[]) (Array) clientKey;
          var sha1 = new SHA256Managed();
          byte[] usStoredKey = sha1.ComputeHash(usClientKey);
          sbyte[] storedKey = (sbyte[]) (Array) usStoredKey;
          var hmac3 = new HMACSHA256((byte[]) (Array) storedKey);
          sbyte[] clientSig = (sbyte[]) (Array) hmac3.ComputeHash((byte[]) (Array) authMsg);
          sbyte[] clientProof = new sbyte[clientKey.Length];
          for (int i = 0; i < clientKey.Length; i++)
          {
            clientProof[i] = (sbyte) (clientKey[i] ^ clientSig[i]);
          }
          return Base64.STANDARD.EncodeBytes((byte[]) (Array) clientProof);
        }
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    private static byte[] StrBytes(string s)
    {
      try
      {
        return Encoding.UTF8.GetBytes(s);
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    private const int clientNonceBytes = 16;
    private const string gs2_header = "n,,";
  }
}