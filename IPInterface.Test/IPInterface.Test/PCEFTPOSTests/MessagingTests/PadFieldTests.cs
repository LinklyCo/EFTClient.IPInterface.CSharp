using System;
using System.Linq;
using PCEFTPOS.EFTClient.IPInterface;
using Xunit;

namespace IPInterface.Test
{
    public class PadFieldTests
    {


        /*
         * The PadField is made up of tags that use the following format:
         *      Field       | Length |      Content
         *      ------------|--------|--------------------------------------
         *      Name        |   3    |  Name of the tag as defined by PP
         *                  |        |
         *      Data Length |   3    |  Length of the data to follow (not
         *                  |        |
         *                  |        |  including tag name and length, padded with 0s)
         *                  |        |
         *      Data        |   X    |  Name of the tag as defined by Pinpad
         */

        // the lenhdr argument ('length header') is the length of the entire message to follow,
        // (not including the lenhdr itself)

        #region Constructor tests

        [Fact]
        public void PadFieldCtr_NoLenhdr_ValidInput_ReturnsPadFieldWithCorrectTags()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";

            var padField = new PadField(input);

            Assert.Equal(3, padField.Count);

            Assert.Equal("XXX", padField[0].Name);
            Assert.Equal("a", padField[0].Data);

            Assert.Equal("YYY", padField[1].Name);
            Assert.Equal("ABC", padField[1].Data);

            Assert.Equal("ZZZ", padField[2].Name);
            Assert.Equal("45", padField[2].Data);
        }

        [Fact]
        public void PadFieldCtr_NoLenhdr_ValidInputEmptyTag_ReturnsPadFieldWithEmptyTag()
        {
            // a tag called “XXX” with a length of 0 and the data “”

            const string input = "XXX000";

            var padField = new PadField(input);

            Assert.Single(padField);

            Assert.Equal("XXX", padField[0].Name);
            Assert.Equal("", padField[0].Data);
        }

        [Fact]
        public void PadFieldCtr_Lenhdr_ValidInput_ReturnsPadFieldWithCorrectTags()
        {
            // a length header with th length of all the data to follow (24 characters total)
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "024XXX001aYYY003ABCZZZ00245";

            var padField = new PadField(input);

            Assert.Equal(3, padField.Count);

            Assert.Equal("XXX", padField[0].Name);
            Assert.Equal("a", padField[0].Data);

            Assert.Equal("YYY", padField[1].Name);
            Assert.Equal("ABC", padField[1].Data);

            Assert.Equal("ZZZ", padField[2].Name);
            Assert.Equal("45", padField[2].Data);

        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void PadFieldCtr_NullOrEmptyInput_ReturnsEmptyPadField(string input)
        {
            var padField = new PadField(input);

            Assert.Empty(padField);
        }


        [Theory]
        [InlineData("XXX001aYYY042ABCZZZ00245")] // middle tag length too long (042, longer than entire string)
        [InlineData("XXX001aYYY-42ABCZZZ00245")] // middle tag length negative (-42)
        [InlineData("XXX001aYYYNULABCZZZ00245")] // middle tag length not integer parseable ('NUL')
        public void PadFieldCtr_NoLenhdr_InvalidInput_ReturnsPadFieldWithEarlierTags(string input)
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 42 (NOTE LONGER THAN ENTIRE INPUT STRING) and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            // const string input = "XXX001aYYY42ABCZZZ00245"

            var padField = new PadField(input);

            Assert.Single(padField);

            Assert.Equal("XXX", padField[0].Name);
            Assert.Equal("a", padField[0].Data);
        }

        #endregion

        #region Add
        [Fact]
        public void Add_AppendsTagToEnd()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";

            var padField = new PadField(input);
            var padTag = new PadTag("TST", "TestData");
            var expectedCount = padField.Count + 1;

            padField.Add(padTag);

            Assert.Equal(expectedCount, padField.Count);
            Assert.Equal(padTag, padField.Last());
        }

        #endregion

        #region Clear
        [Fact]
        public void Clear_EmptiesTags()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";

            var padField = new PadField(input);
            padField.Clear();

            Assert.Empty(padField);
        }
        #endregion

        #region GetAsString
        [Fact]
        public void GetAsString_Lenhdr_ReturnsInputStrWithLenhdr()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";
            string lenHdr = input.Length.ToString().PadLeft(3, '0');
            string expected = lenHdr + input;

            var padField = new PadField(input);
            var actual = padField.GetAsString(true);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetAsString_NoLenhdr_ReturnsInputStr()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string expected = "XXX001aYYY003ABCZZZ00245";

            var padField = new PadField(expected);
            var actual = padField.GetAsString(false);

            Assert.Equal(expected, actual);
        }


        #endregion

        #region GetTag

        [Theory]
        [InlineData("XXX001aYYY003ABCZZZ00245", "XXX", "a", 3)] // first present tag
        [InlineData("XXX001aYYY003ABCZZZ00245", "YYY", "ABC", 3)] // second present tag
        [InlineData("XXX001aYYY003ABCZZZ00245", "ZZZ", "45", 3)] // Third present tag
        [InlineData("XXX001aYYY003ABCZZZ00245", "NOO", "", 4)] // Not present tag
        [InlineData("XXX001aYYY003ABCZZZ00245", null, "", 4)] // Null tag (not present, checking if throws)
        public void GetTag_ReturnsTagIfPresentOrAddsTagWithEmptyDataAndReturnsThat(string pad, string tagName, string expectedTagData, int expectedCount)
        {
            var padField = new PadField(pad);
            var actualTag = padField.GetTag(tagName);

            Assert.Equal(expectedTagData, actualTag.Data);
            Assert.Equal(tagName, actualTag.Name);
            Assert.Equal(padField.Count, expectedCount);
        }

        #endregion


        #region GetTagInPad
        [Theory]
        [InlineData("XXX001aYYY003ABCZZZ00245", "XXX", "a")] // first present tag
        [InlineData("XXX001aYYY003ABCZZZ00245", "YYY", "ABC")] // second present tag
        [InlineData("XXX001aYYY003ABCZZZ00245", "ZZZ", "45")] // Third present tag
        [InlineData("XXX001aYYY003ABCZZZ00245", "NOO", "")] // Not present tag
        [InlineData("XXX001aYYY003ABCZZZ00245", null, "")] // Null tag (not present, checking if throws
        public void GetTagInPad_ReturnsConstructedTagIfPresentOrTagWithEmptyData(string pad, string tagName, string expectedTagData)
        {
            var actual = PadField.GetTagInPad(pad, tagName);

            Assert.Equal(expectedTagData, actual);
        }

        #endregion

        #region FindTag

        [Fact]
        public void FindTag_TagPresent_ReturnsCorrectTagIdx()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";

            var padField = new PadField(input);

            Assert.Equal(0, padField.FindTag("XXX"));
            Assert.Equal(1, padField.FindTag("YYY"));
            Assert.Equal(2, padField.FindTag("ZZZ"));
        }

        [Fact]
        public void FindTag_TagNotPresent_ReturnsMinusOne()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “42”
            const string input = "XXX001aYYY003ABCZZZ00242";

            var padField = new PadField(input);

            Assert.Equal(-1, padField.FindTag("NOO"));
        }

        [Fact]
        public void FindTag_MultipleMatchingTags_ReturnsFirst()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “YYY” with a length of 2 and the data “42”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCYYY00242ZZZ00245";

            var padField = new PadField(input);

            Assert.Equal(1, padField.FindTag("YYY"));
        }

        #endregion

        #region FindTags

        [Fact]
        public void FindTags_MultipleMatchingTags_ReturnsAll()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “YYY” with a length of 2 and the data “42”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCYYY00242ZZZ00245";

            var padField = new PadField(input);
            var tags = padField.FindTags("YYY");


            Assert.Collection(tags, tag1 => Assert.Equal("ABC", tag1.Data),
                tag2 => Assert.Equal("42", tag2.Data));
        }

        [Fact]
        public void FindTags_NoMatchingTags_ReturnsAll()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “YYY” with a length of 2 and the data “42”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCYYY00242ZZZ00245";

            var padField = new PadField(input);
            var tags = padField.FindTags("NOO");


            Assert.Empty(tags);
        }

        #endregion

        #region HasTag

        [Theory]
        [InlineData("XXX", true)]
        [InlineData("NOO", false)]
        [InlineData(null, false)]
        public void HasTag_ReturnsIfNamedTagPresent(string tagName, bool expected)
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";
            var padField = new PadField(input);

            var actual = padField.HasTag(tagName);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region IndexOf

        [Theory]
        [InlineData("XXX", "a",  0)]
        [InlineData("YYY", "ABC", 1)]
        [InlineData("NOO", "TST", -1)]
        [InlineData("XXX", "TST", -1)] // tag name present but not data
        public void IndexOf_ReturnsIdxOfTagIfPresentOtherwiseMinusOne(string tagName, string tagData, int expected)
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";
            var padField = new PadField(input);
            var tag = new PadTag(tagName, tagData);


            var actual = padField.IndexOf(tag);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Insert
        [Fact]
        public void Insert_InsertsTagAtIndex()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";

            var padField = new PadField(input);
            var padTag = new PadTag("TST", "TestData");
            var expectedCount = padField.Count + 1;

            padField.Insert(0, padTag);

            Assert.Equal(expectedCount, padField.Count);
            Assert.Equal(padTag, padField.First());
        }

        [Fact]
        public void Insert_InvalidIndex_ThrowsArgumentOutOfRangeException()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";

            var padField = new PadField(input);
            var padTag = new PadTag("TST", "TestData");

            Action negativeIdx = () => padField.Insert(-1, padTag);
            Action tooLargeIdx = () => padField.Insert(padField.Count+10, padTag);

            Assert.Throws<ArgumentOutOfRangeException>(negativeIdx);
            Assert.Throws<ArgumentOutOfRangeException>(tooLargeIdx);
        }

        #endregion

        #region Contains

        [Fact]
        public void ContainsString_TagPresent_ReturnsTrue()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";

            var padField = new PadField(input);


            Assert.True(padField.Contains("XXX001a"));
            Assert.True(padField.Contains("YYY003ABC"));
            Assert.True(padField.Contains("ZZZ00245"));
        }

        [Fact]
        public void ContainsPadTag_TagPresent_ReturnsTrue()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";
            var xxx = new PadTag("XXX", "a");
            var yyy = new PadTag("YYY", "ABC");
            var zzz = new PadTag("ZZZ", "45");


            var padField = new PadField(input);


            Assert.Contains(xxx, padField);
            Assert.Contains(yyy, padField);
            Assert.Contains(zzz, padField);
        }

        [Fact]
        public void ContainsString_TagNotPresent_ReturnsFalse()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";

            var padField = new PadField(input);


            // empty tag
            Assert.False(padField.Contains("NOO"));

            // correct tag name but different content
            Assert.False(padField.Contains("XXX001b"));
        }

        [Fact]
        public void ContainsPadTag_TagNotPresent_ReturnsFalse()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";

            // present tag with different data
            var xxx = new PadTag("XXX", "DIFFERENT DATA");

            // missing tag
            var noo = new PadTag("NOO", "OOO");

            var padField = new PadField(input);


            Assert.DoesNotContain(xxx, padField);
            Assert.DoesNotContain(noo, padField);
        }

        #endregion

        #region Remove

        [Fact]
        public void Remove_TagPresent_RemovesTag_ReturnsTrue()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";
            var padField = new PadField(input);
            var toRemove = new PadTag("YYY", "ABC");

            var expectedCount = padField.Count - 1;
            var actual = padField.Remove(toRemove);

            Assert.True(actual);
            Assert.Equal(expectedCount, padField.Count);
            Assert.DoesNotContain(padField, tag => tag.Name == "YYY" && tag.Data == "ABC");
        }

        [Fact]
        public void RemoveTag_TagNotPreset_DoesNothing_ReturnsFalse()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";
            var padFieldTest = new PadField(input);
            var padFieldControl = new PadField(input);
            var toRemove = new PadTag("NOO", "");

            var actual = padFieldTest.Remove(toRemove);

            Assert.False(actual);
            Assert.Equal(padFieldControl.Count, padFieldTest.Count);
            for (int i = 0; i < padFieldControl.Count; i++)
            {
                Assert.Equal(padFieldControl[i], padFieldTest[i]);
            }
        }

        [Fact]
        public void RemoveTag_TagWithSameNameDiffData_DoesNothing_ReturnsFalse()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";
            var padFieldTest = new PadField(input);
            var padFieldControl = new PadField(input);
            var toRemove = new PadTag("YYY", "NOO");

            var actual = padFieldTest.Remove(toRemove);

            Assert.False(actual);
            Assert.Equal(padFieldControl.Count, padFieldTest.Count);
            for (int i = 0; i < padFieldControl.Count; i++)
            {
                Assert.Equal(padFieldControl[i], padFieldTest[i]);
            }
        }
        #endregion

        #region RemoveTag

        [Fact]
        public void RemoveTag_TagPresent_RemovesTag()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";
            var padField = new PadField(input);

            var expectedCount = padField.Count - 1;
            padField.RemoveTag("YYY");

            Assert.Equal(expectedCount, padField.Count);
            Assert.DoesNotContain(padField, tag => tag.Name == "YYY" && tag.Data == "ABC");
        }

        [Fact]
        public void RemoveTag_TagNotPresent_DoesNothing()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";
            var padFieldTest = new PadField(input);
            var padFieldControl = new PadField(input);

            padFieldTest.RemoveTag("NOO");


            Assert.Equal(padFieldControl.Count, padFieldTest.Count);
            for (int i = 0; i < padFieldControl.Count; i++)
            {
                Assert.Equal(padFieldControl[i], padFieldTest[i]);
            }
        }


        [Fact]
        public void RemoveTag_MultipleMatchingTagsPresent_RemovesFirstButLeavesOthers()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “YYY” with a length of 2 and the data “42”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCYYY00242ZZZ00245";

            var padField = new PadField(input);

            var expectedCount = padField.Count - 1;
            padField.RemoveTag("YYY");

            Assert.Equal(expectedCount, padField.Count);
            Assert.DoesNotContain(padField, tag => tag.Name == "YYY" && tag.Data == "ABC");
            Assert.Contains(padField, tag => tag.Name == "YYY" && tag.Data == "42");
        }

        #endregion


        #region SetTag
        [Fact]
        public void SetTag_TagNotPresent_AddsNewTagToEnd()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";
            const string testFieldData = "Test";
            var padField = new PadField(input);

            var beforeSet = padField.Count;
            padField.SetTag("TTT", testFieldData);


            Assert.Equal(beforeSet+1,padField.Count);
            Assert.Equal(testFieldData, padField[beforeSet].Data);
        }

        [Fact]
        public void SetTag_SingleMatchingTagPresent_UpdatesTagData()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aYYY003ABCZZZ00245";
            const string testFieldData = "Test";
            var padField = new PadField(input);

            var beforeSet = padField.Count;
            padField.SetTag("XXX", testFieldData);


            Assert.Equal(beforeSet, padField.Count);
            Assert.Equal(testFieldData, padField[0].Data);
        }

        [Fact]
        public void SetTag_TwoMatchingTagsPresent_UpdatesFirstTagData()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // a second tag called “XXX” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string input = "XXX001aXXX003ABCZZZ00245";
            const string testFieldData = "Test";
            var padField = new PadField(input);

            var beforeSet = padField.Count;
            padField.SetTag("XXX", testFieldData);


            Assert.Equal(beforeSet, padField.Count);
            Assert.Equal(testFieldData, padField[0].Data);
            Assert.Equal("ABC", padField[1].Data);
        }

        #endregion


        #region ToString

        [Fact]
        public void ToString_ReturnsInputStr()
        {
            // a tag called “XXX” with a length of 1 and the data “a”
            // tag called “YYY” with a length of 3 and the data “ABC”
            // tag called “ZZZ” with a length of 2 and the data “45”
            const string expected = "XXX001aYYY003ABCZZZ00245";

            var padField = new PadField(expected);
            var actual = padField.ToString();

            Assert.Equal(expected, actual);
        }

        #endregion


        #region UpdateTagInPad
        [Theory]
        // set first present tag, no lenhdr in original (note is in output)
        [InlineData("XXX001aYYY003ABCZZZ00245", "XXX", "b", "024XXX001bYYY003ABCZZZ00245")]

        // second present tag, lenhedr in source, new total length
        [InlineData("024XXX001aYYY003ABCZZZ00245", "YYY", "DEFG", "025XXX001aYYY004DEFGZZZ00245")]

        // tag not present, no lenhdr in original
        [InlineData("XXX001aYYY003ABCZZZ00245", "SVN", "git", "033XXX001aYYY003ABCZZZ00245SVN003git")]
        public void UpdateTagInPad_ReturnsUpdatedPadFieldStringWithLenhdr(string pad, string tagName, string data, string expected)
        {
            var actual = PadField.UpdateTagInPad(pad, tagName, data);

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
