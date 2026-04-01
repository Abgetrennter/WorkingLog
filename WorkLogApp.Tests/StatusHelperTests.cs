using System;
using System.Linq;
using WorkLogApp.Core.Enums;
using WorkLogApp.Core.Helpers;
using Xunit;

namespace WorkLogApp.Tests
{
    /// <summary>
    /// StatusHelper 单元测试
    /// </summary>
    public class StatusHelperTests
    {
        [Theory]
        [InlineData(StatusEnum.Todo, "待办")]
        [InlineData(StatusEnum.Doing, "进行中")]
        [InlineData(StatusEnum.Done, "已完成")]
        [InlineData(StatusEnum.Blocked, "已阻塞")]
        [InlineData(StatusEnum.Cancelled, "已取消")]
        public void ToChinese_ShouldReturnCorrectChinese(StatusEnum status, string expected)
        {
            // Act
            var result = status.ToChinese();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("待办", StatusEnum.Todo)]
        [InlineData("进行中", StatusEnum.Doing)]
        [InlineData("已完成", StatusEnum.Done)]
        [InlineData("已阻塞", StatusEnum.Blocked)]
        [InlineData("已取消", StatusEnum.Cancelled)]
        public void Parse_ChineseInput_ShouldReturnCorrectStatus(string input, StatusEnum expected)
        {
            // Act
            var result = StatusHelper.Parse(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Todo", StatusEnum.Todo)]
        [InlineData("Doing", StatusEnum.Doing)]
        [InlineData("Done", StatusEnum.Done)]
        [InlineData("Blocked", StatusEnum.Blocked)]
        [InlineData("Cancelled", StatusEnum.Cancelled)]
        public void Parse_EnglishInput_ShouldReturnCorrectStatus(string input, StatusEnum expected)
        {
            // Act
            var result = StatusHelper.Parse(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("0", StatusEnum.Todo)]
        [InlineData("1", StatusEnum.Doing)]
        [InlineData("2", StatusEnum.Done)]
        [InlineData("3", StatusEnum.Blocked)]
        [InlineData("4", StatusEnum.Cancelled)]
        public void Parse_NumericInput_ShouldReturnCorrectStatus(string input, StatusEnum expected)
        {
            // Act
            var result = StatusHelper.Parse(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("TODO")]  // 大写
        [InlineData("todo")]  // 小写
        [InlineData("ToDO")]  // 混合大小写
        public void Parse_CaseInsensitive_ShouldReturnCorrectStatus(string input)
        {
            // Act
            var result = StatusHelper.Parse(input);

            // Assert
            Assert.Equal(StatusEnum.Todo, result);
        }

        [Theory]
        [InlineData("  待办  ", StatusEnum.Todo)]
        [InlineData("\t进行中\t", StatusEnum.Doing)]
        public void Parse_WithWhitespace_ShouldTrimAndReturnCorrectStatus(string input, StatusEnum expected)
        {
            // Act
            var result = StatusHelper.Parse(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Parse_InvalidInput_ShouldReturnDefault()
        {
            // Act
            var result = StatusHelper.Parse("InvalidStatus");

            // Assert
            Assert.Equal(StatusEnum.Todo, result); // 默认值
        }

        [Fact]
        public void Parse_NullInput_ShouldReturnDefault()
        {
            // Act
            var result = StatusHelper.Parse(null);

            // Assert
            Assert.Equal(StatusEnum.Todo, result); // 默认值
        }

        [Fact]
        public void Parse_EmptyInput_ShouldReturnDefault()
        {
            // Act
            var result = StatusHelper.Parse("");

            // Assert
            Assert.Equal(StatusEnum.Todo, result); // 默认值
        }

        [Fact]
        public void GetList_ShouldReturnAllStatuses()
        {
            // Act
            var result = StatusHelper.GetList();

            // Assert
            Assert.Equal(5, result.Count);
            Assert.Contains(result, x => x.Key == StatusEnum.Todo && x.Value == "待办");
            Assert.Contains(result, x => x.Key == StatusEnum.Doing && x.Value == "进行中");
            Assert.Contains(result, x => x.Key == StatusEnum.Done && x.Value == "已完成");
            Assert.Contains(result, x => x.Key == StatusEnum.Blocked && x.Value == "已阻塞");
            Assert.Contains(result, x => x.Key == StatusEnum.Cancelled && x.Value == "已取消");
        }

        [Theory]
        [InlineData(StatusEnum.Todo, true)]
        [InlineData(StatusEnum.Doing, true)]
        [InlineData(StatusEnum.Done, false)]
        [InlineData(StatusEnum.Blocked, true)]
        [InlineData(StatusEnum.Cancelled, false)]
        public void IsIncomplete_ShouldReturnCorrectResult(StatusEnum status, bool expected)
        {
            // Act
            var result = StatusHelper.IsIncomplete(status);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
