using System;
using System.Collections.Generic;
using System.Text;
using PCEFTPOS.EFTClient.IPInterface;
using Xunit;

namespace IPInterface.Test
{
    public class DirectEncodingTest
    {
        [Theory]
        [InlineData(new char[] {})]
        [InlineData(new char[] {'H', 'e', 'l', 'l', 'o'})]
        [InlineData(new char[] {'H', 'e', 'l', 'l', 'o', '\0', 't', 'h', 'e', 'r', 'e'})]
        public void GetByteCount_CharArray_ReturnsArrayLen(char[] chars)
        {
            Assert.Equal(chars.Length, DirectEncoding.DIRECT.GetByteCount(chars));
        }

        [Fact]
        public void GetByteCount_CharArray_NullInput_ThrowsNullReferenceException()
        {
            char[] chars = null;
            Action actual = () => DirectEncoding.DIRECT.GetByteCount(chars);
            Assert.Throws<NullReferenceException>(actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Hello")]
        [InlineData("Hello\0There")]
        public void GetByteCount_String_ReturnsStringLen(string str)
        {
            Assert.Equal(str.Length, DirectEncoding.DIRECT.GetByteCount(str));
        }

        [Fact]
        public void GetByteCount_String_NullInput_ThrowsNullReferenceException()
        {
            string str = null;
            Action actual = () => DirectEncoding.DIRECT.GetByteCount(str);
            Assert.Throws<NullReferenceException>(actual);
        }


        [Theory]
        [InlineData(new char[] { }, 0, 0)]
        [InlineData(new char[] { }, 0, 42)] // prove we can just set count to anything
        [InlineData(new char[] { 'H', 'e', 'l', 'l', 'o' }, 0, 5)]
        [InlineData(new char[] { 'H', 'e', 'l', 'l', 'o' }, 99, -1)] // prove we can just set count and index to anything
        [InlineData(null, 1235, -666)]
        public void GetByteCount_CountArg_ReturnsCountArg(char[] chars, int index, int count)
        {
            Assert.Equal(count, DirectEncoding.DIRECT.GetByteCount(chars, index, count));

        }

        [Theory]
        [InlineData(new char[] { }, new byte[] {})]
        [InlineData(new [] { 'H', 'e', 'l', 'l', 'o' }, new [] { (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o' })]
        [InlineData(new [] { 'H', 'i', '\0', 'y', 'o', 'u' }, new [] { (byte)'H', (byte)'i', (byte)'\0', (byte)'y', (byte)'o', (byte)'u' })]
        public void GetBytes_CharArray_ReturnsCorrespondingByteArray(char[] chars, byte[] expected)
        {
            Assert.Equal(expected, DirectEncoding.DIRECT.GetBytes(chars));
        }

        [Fact]
        public void GetBytes_CharArray_NullInput_ThrowsNullReferenceException()
        {
            char[] nullArr = null;
            Action actual = () => DirectEncoding.DIRECT.GetBytes(nullArr);
            Assert.Throws<NullReferenceException>(actual);
        }

        [Theory]
        [InlineData("", new byte[] { })]
        [InlineData("Hello", new[] { (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o' })]
        [InlineData("Hi\0you", new[] { (byte)'H', (byte)'i', (byte)'\0', (byte)'y', (byte)'o', (byte)'u' })]
        public void GetBytes_String_ReturnsCorrespondingByteArray(string str, byte[] expected)
        {
            Assert.Equal(expected, DirectEncoding.DIRECT.GetBytes(str));
        }

        [Fact]
        public void GetBytes_String_NullInput_ThrowsNullReferenceException()
        {
            string str = null;
            Action actual = () => DirectEncoding.DIRECT.GetBytes(str);
            Assert.Throws<NullReferenceException>(actual);
        }

        [Theory]
        [InlineData(new char[] { }, 0, 0, new byte[] { })]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 0, 5, new[] { (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o' })]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 4, 1, new[] { (byte)'o' })]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 4, 0, new byte[] {})]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 1, 2, new[] { (byte)'e', (byte)'l' })]
        [InlineData(new[] { 'H', 'i', '\0', 'y', 'o', 'u' }, 0, 6, new[] { (byte)'H', (byte)'i', (byte)'\0', (byte)'y', (byte)'o', (byte)'u' })]
        [InlineData(new[] { 'H', 'i', '\0', 'y', 'o', 'u' }, 2, 4, new[] { (byte)'\0', (byte)'y', (byte)'o', (byte)'u' })]
        [InlineData(new[] { 'H', 'i', '\0', 'y', 'o', 'u' }, 1, 1, new[] { (byte)'i' })]
        public void GetBytes_CharArrayCharIndex_ReturnsCorrespondingByteArray(char[] chars, int index, char count, byte[] expected)
        {
            Assert.Equal(expected, DirectEncoding.DIRECT.GetBytes(chars, index, count));
        }

        [Theory]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, -1)] // negative index
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 72)] // index > length of input
        public void GetBytes_CharArrayCharIndex_InvalidIdx_ThrowsArgumentOutOfRangeException(char[] chars, int index)
        {
            // just try and get a single character because we are testing the index
            Action actual = () => DirectEncoding.DIRECT.GetBytes(chars, index, 1);
            Assert.Throws<ArgumentOutOfRangeException>(actual);
        }

        [Theory]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 72)] // count > length of input
        public void GetBytes_CharArrayCharIndex_InvalidCount_ThrowsArgumentOutOfRangeException(char[] chars, int count)
        {
            // just start from 0 index because we are testing the count
            Action actual = () => DirectEncoding.DIRECT.GetBytes(chars, 0, count);
            Assert.Throws<ArgumentOutOfRangeException>(actual);
        }

        [Fact]
        public void GetBytes_CharArrayCharIndex_NullInput_ThrowsArgumentNullException()
        {
            // just start from 0 index because we are testing the count
            char[] chars = null;
            Action actual = () => DirectEncoding.DIRECT.GetBytes(chars, 0, 0);
            Assert.Throws<ArgumentNullException>(actual);
        }

        [Fact]
        public void GetBytes_CharArrayCharIndex_NegativeCount_ThrowsOverflowException()
        {
            // just start from 0 index because we are testing the count
            Action actual = () => DirectEncoding.DIRECT.GetBytes(new[] { 'H', 'e', 'l', 'l', 'o' }, 0, -1);
            Assert.Throws<OverflowException>(actual);
        }

        [Theory]
        [InlineData(new char[] { }, 0, 0, new byte[] { }, 0, new byte[] { })]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 0, 5, new[] { (byte)'Z', (byte)'Z', (byte)'Z', (byte)'Z', (byte)'Z' }, 0, new[] { (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o' })]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 4, 1, new[] { (byte)'Z', (byte)'Z' }, 1, new[] { (byte)'Z', (byte)'o' })]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 4, 0, new[] { (byte)'Z', (byte)'Z', (byte)'Z', (byte)'Z', (byte)'Z' }, 0, new[] { (byte)'Z', (byte)'Z', (byte)'Z', (byte)'Z', (byte)'Z' })]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 1, 2, new[] { (byte)'Z', (byte)'Z' }, 0, new[] { (byte)'e', (byte)'l' })]
        [InlineData(new[] { 'H', 'i', '\0', 'y', 'o', 'u' }, 0, 6, 
            new[] { (byte)'Z', (byte)'Z', (byte)'Z', (byte)'Z', (byte)'Z', (byte)'Z', (byte)'Z' }, 0, new[] { (byte)'H', (byte)'i', (byte)'\0', (byte)'y', (byte)'o', (byte)'u', (byte)'Z' })]
        public void GetBytes_CharArrayCharIdxByteIdx_CopiesToByteArray_ReturnsCount(char[] chars, int charIdx, char count, byte[] bytes, int byteIdx, byte[] expected)
        {
            var actualCount = DirectEncoding.DIRECT.GetBytes(chars, charIdx, count, bytes, byteIdx);
            Assert.Equal(actualCount, count);
            Assert.Equal(expected, bytes);
        }

        [Theory]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, -1)] // negative index
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 72)] // index > length of input
        public void GetBytes_CharArrayCharIdxByteIdx_InvalidCharIdx_ThrowsArgumentOutOfRangeException(char[] chars, int charIndex)
        {
            // just try and get a single character because we are testing the index
            Action actual = () => DirectEncoding.DIRECT.GetBytes(chars, charIndex, 1, new byte[chars.Length], 0);
            Assert.Throws<ArgumentOutOfRangeException>(actual);
        }

        [Theory]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 72)] // count > length of input
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, -1)] // count > length of input
        public void GetBytes_CharArrayCharIdxByteIdx_InvalidCount_ThrowsArgumentOutOfRangeException(char[] chars, int count)
        {
            // just start from 0 index because we are testing the count
            Action actual = () => DirectEncoding.DIRECT.GetBytes(chars, 0, count, new byte[chars.Length], 0);
            Assert.Throws<ArgumentOutOfRangeException>(actual);
        }


        [Theory]
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, -1)] // negative byte index
        [InlineData(new[] { 'H', 'e', 'l', 'l', 'o' }, 72)] // byte index > length of byte array
        public void GetBytes_CharArrayCharIdxByteIdx_InvalidByteIdx_ThrowsArgumentOutOfRangeException(char[] chars, int byteIdx)
        {
            // just try and get a single character because we are testing the byteIndex
            Action actual = () => DirectEncoding.DIRECT.GetBytes(chars, 0, 1, new byte[chars.Length], byteIdx);
            Assert.Throws<ArgumentOutOfRangeException>(actual);
        }

        [Fact]
        public void GetBytes_CharArrayCharIdxByteIdx_NullChars_ThrowsArgumentNullException()
        {
            // just start from 0 index because we are testing the count
            char[] chars = null;

            Action actual = () => DirectEncoding.DIRECT.GetBytes(chars, 0, 0, new byte[1], 0);
            Assert.Throws<ArgumentNullException>(actual);
        }

        [Fact]
        public void GetBytes_CharArrayCharIdxByteIdx_NullBytes_ThrowsArgumentNullException()
        {
            Action actual = () => DirectEncoding.DIRECT.GetBytes(new []{ 'H'}, 0, 0, null, 0);
            Assert.Throws<ArgumentNullException>(actual);
        }

        // todo write tests for remaining public functions
    }
}
