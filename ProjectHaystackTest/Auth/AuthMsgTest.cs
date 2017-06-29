//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectHaystack.Auth.Tests
{
  [TestClass()]
  public class AuthMsgTest
  {
    [TestMethod()]
    public void EncodeTest()
    {
      //basic identity
      Assert.AreEqual(AuthMsg.FromStr("foo"), AuthMsg.FromStr("foo"));
      Assert.AreEqual(AuthMsg.FromStr("a x=y"), AuthMsg.FromStr("a x=y"));
      Assert.AreEqual(AuthMsg.FromStr("a i=j, x=y"), AuthMsg.FromStr("a i = j, x = y"));
      Assert.AreEqual(AuthMsg.FromStr("a i=j, x=y"), AuthMsg.FromStr("a i=j, x=y"));
      Assert.AreNotEqual(AuthMsg.FromStr("foo"), AuthMsg.FromStr("bar"));
      Assert.AreNotEqual(AuthMsg.FromStr("foo"), AuthMsg.FromStr("foo k=v"));

      //basics on fromStr
      AuthMsg q = AuthMsg.FromStr("foo alpha=beta, gamma=delta");
      Assert.AreEqual(q.scheme, "foo");
      Assert.AreEqual(q.Param("alpha") , "beta");
      Assert.AreEqual(q.Param("Alpha") , "beta");
      Assert.AreEqual(q.Param("ALPHA") , "beta");
      Assert.AreEqual(q.Param("Gamma") , "delta");

      //fromStr parsing
      SortedDictionary<string, string> parameters = new SortedDictionary<string, string>();
      parameters.Add("alpha", "beta");
      Assert.AreEqual(AuthMsg.FromStr("foo alpha \t = \t beta"), new AuthMsg("foo", parameters));

      parameters.Clear();
      parameters.Add("A", "b");
      parameters.Add("C", "d");
      parameters.Add("E", "f");
      parameters.Add("G", "h");
      Assert.AreEqual(AuthMsg.FromStr("foo a=b, c = d, e=f, g=h"), new AuthMsg("foo", parameters));

      parameters.Clear();
      parameters.Add("G", "h");
      parameters.Add("E", "f");
      parameters.Add("C", "d");
      parameters.Add("A", "b");
      Assert.AreEqual(AuthMsg.FromStr("foo a=b, c = d, e=f, g=h"), new AuthMsg("foo", parameters));

      parameters.Clear();
      parameters.Add("G", "h");
      parameters.Add("E", "f");
      parameters.Add("C", "d");
      parameters.Add("A", "b");
      Assert.AreEqual(AuthMsg.FromStr("foo g=h, c = d, e=f,  a = b").ToString(), "foo a=b, c=d, e=f, g=h");
    }

    [TestMethod()]
    [ExpectedException(typeof(FormatException))]
    public void BadFromStrTest()
    {
      AuthMsg.FromStr("hmac salt=a=b hash=sha-1");
      AuthMsg.FromStr("hmac salt=abc hash=sha-1 bad/key=val");
      AuthMsg.FromStr("(bad)");
      AuthMsg.FromStr("ok key=val not good");
      AuthMsg.FromStr("ok key not good=val");
      AuthMsg.FromStr("ok key not good=val");
      AuthMsg.FromStr("hmac foo");
      AuthMsg.FromStr("hmac foo=bar xxx");
    }

    [TestMethod()]
    public void SplitListTest()
    {
      VerifySplitList("a,b", new String[] { "a", "b" });
      VerifySplitList("a \t,  b", new String[] { "a", "b" });
      VerifySplitList("a, b, c", new String[] { "a", "b", "c" });
      VerifySplitList("a b=c", new String[] { "a b=c" });
      VerifySplitList("a b=c, d=e", new String[] { "a b=c,d=e" });
      VerifySplitList("a b=c, d=e \t,\t f=g", new String[] { "a b=c,d=e,f=g" });
      VerifySplitList("a b=c, d=e, f g=h", new String[] { "a b=c,d=e", "f g=h" });
      VerifySplitList("a b=c, d=e, f, g h=i,j=k", new String[] { "a b=c,d=e", "f", "g h=i,j=k" });
    }

    private void VerifySplitList(String s, String[] expected)
    {
      String[] split = AuthMsg.SplitList(s);
      Assert.AreEqual(split.Length, expected.Length);
      for(int i=0; i< split.Length; i++)
      {
        Assert.AreEqual(split[i], expected[i]);
      }
      AuthMsg[] msgs = AuthMsg.ListFromStr(s);
      Assert.AreEqual(msgs.Length, expected.Length);
    }
    
  }
}