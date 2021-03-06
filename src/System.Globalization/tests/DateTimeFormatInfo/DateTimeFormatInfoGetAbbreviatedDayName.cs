// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Xunit;

namespace System.Globalization.Tests
{
    public class DateTimeFormatInfoGetAbbreviatedDayName
    {
        public static IEnumerable<object[]> GetAbbreviatedDayName_TestData()
        {
            string[] englishAbbreviatedDayNames = new string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            yield return new object[] { DateTimeFormatInfo.InvariantInfo, englishAbbreviatedDayNames };
            yield return new object[] { new CultureInfo("en-US").DateTimeFormat, englishAbbreviatedDayNames };
            yield return new object[] { new DateTimeFormatInfo(), englishAbbreviatedDayNames };

            // ActiveIssue(2103)
            if (!PlatformDetection.IsUbuntu1510)
            {
                yield return new object[] { new CultureInfo("fr-FR").DateTimeFormat, DateTimeFormatInfoData.FrFRAbbreviatedDayNames() };
            }
        }

        [Theory]
        [MemberData(nameof(GetAbbreviatedDayName_TestData))]
        public void GetAbbreviatedDayName(DateTimeFormatInfo info, string[] expected)
        {
            DayOfWeek[] values = new DayOfWeek[]
            {
                DayOfWeek.Sunday,
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday
            };

            for (int i = 0; i < values.Length; ++i)
            {
                Assert.Equal(expected[i], info.GetAbbreviatedDayName(values[i]));
            }
        }

        [Fact]
        public void GetAbbreviatedDayName_Invalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DateTimeFormatInfo().GetAbbreviatedDayName((DayOfWeek)(-1))); // DayOfWeek is invalid
        }
    }
}
