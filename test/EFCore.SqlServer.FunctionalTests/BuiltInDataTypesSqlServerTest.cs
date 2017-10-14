// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable UnusedParameter.Local
// ReSharper disable PossibleInvalidOperationException
namespace Microsoft.EntityFrameworkCore
{
    [SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
    public class BuiltInDataTypesSqlServerTest : BuiltInDataTypesTestBase<BuiltInDataTypesSqlServerTest.BuiltInDataTypesSqlServerFixture>
    {
        public BuiltInDataTypesSqlServerTest(BuiltInDataTypesSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [Fact]
        public virtual void Can_perform_query_with_ansi_strings()
        {
            Can_perform_query_with_ansi_strings_test(supportsAnsi: true);
        }

        [Fact]
        public void Sql_translation_uses_type_mapper_when_constant()
        {
            using (var context = CreateContext())
            {
                var results
                    = context.Set<MappedNullableDataTypes>()
                        .Where(e => e.TimeSpanAsTime == new TimeSpan(0, 1, 2))
                        .Select(e => e.Int)
                        .ToList();

                Assert.Equal(0, results.Count);
                Assert.Equal(
                    @"SELECT [e].[Int]
FROM [MappedNullableDataTypes] AS [e]
WHERE [e].[TimeSpanAsTime] = '00:01:02'",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Sql_translation_uses_type_mapper_when_parameter()
        {
            using (var context = CreateContext())
            {
                var timeSpan = new TimeSpan(2, 1, 0);

                var results
                    = context.Set<MappedNullableDataTypes>()
                        .Where(e => e.TimeSpanAsTime == timeSpan)
                        .Select(e => e.Int)
                        .ToList();

                Assert.Equal(0, results.Count);
                Assert.Equal(
                    @"@__timeSpan_0='02:01:00'

SELECT [e].[Int]
FROM [MappedNullableDataTypes] AS [e]
WHERE [e].[TimeSpanAsTime] = @__timeSpan_0",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public virtual void Can_query_using_any_mapped_data_type()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Int = 999,
                        LongAsBigint = 78L,
                        ShortAsSmallint = 79,
                        ByteAsTinyint = 80,
                        UintAsInt = uint.MaxValue,
                        UlongAsBigint = ulong.MaxValue,
                        UShortAsSmallint = ushort.MaxValue,
                        SbyteAsTinyint = sbyte.MinValue,
                        BoolAsBit = true,
                        DecimalAsMoney = 81.1m,
                        DecimalAsSmallmoney = 82.2m,
                        DoubleAsFloat = 83.3,
                        FloatAsReal = 84.4f,
                        DoubleAsDoublePrecision = 85.5,
                        DateTimeAsDate = new DateTime(1605, 1, 2, 10, 11, 12),
                        DateTimeOffsetAsDatetimeoffset = new DateTimeOffset(new DateTime(), TimeSpan.Zero),
                        DateTimeAsDatetime2 = new DateTime(),
                        DateTimeAsSmalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                        DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
                        TimeSpanAsTime = new TimeSpan(0, 11, 15, 12, 2),
                        StringAsVarcharMax = "C",
                        StringAsCharVaryingMax = "Your",
                        StringAsCharacterVaryingMax = "strong",
                        StringAsNvarcharMax = "don't",
                        StringAsNationalCharVaryingMax = "help",
                        StringAsNationalCharacterVaryingMax = "anyone!",
                        StringAsText = "Gumball Rules!",
                        StringAsNtext = "Gumball Rules OK!",
                        BytesAsVarbinaryMax = new byte[] { 89, 90, 91, 92 },
                        BytesAsBinaryVaryingMax = new byte[] { 93, 94, 95, 96 },
                        BytesAsImage = new byte[] { 97, 98, 99, 100 },
                        Decimal = 101.7m,
                        DecimalAsDec = 102.8m,
                        DecimalAsNumeric = 103.9m,
                        GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
                        UintAsBigint = uint.MaxValue,
                        UlongAsDecimal200 = ulong.MaxValue,
                        UShortAsInt = ushort.MaxValue,
                        SByteAsSmallint = sbyte.MinValue,
                        CharAsVarcharMax = 'A',
                        CharAsAsCharVaryingMax = 'B',
                        CharAsCharacterVaryingMax = 'C',
                        CharAsNvarcharMax = 'D',
                        CharAsNationalCharVaryingMax = 'E',
                        CharAsNationalCharacterVaryingMax = 'F',
                        CharAsText = 'G',
                        CharAsNtext = 'H',
                        CharAsInt = 'I'
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999);

                long? param1 = 78L;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.LongAsBigint == param1));

                short? param2 = 79;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.ShortAsSmallint == param2));

                byte? param3 = 80;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.ByteAsTinyint == param3));

                bool? param4 = true;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.BoolAsBit == param4));

                decimal? param5 = 81.1m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DecimalAsMoney == param5));

                decimal? param6 = 82.2m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DecimalAsSmallmoney == param6));

                double? param7a = 83.3;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DoubleAsFloat == param7a));

                float? param7b = 84.4f;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.FloatAsReal == param7b));

                double? param7c = 85.5;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DoubleAsDoublePrecision == param7c));

                DateTime? param8 = new DateTime(1605, 1, 2);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsDate == param8));

                DateTimeOffset? param9 = new DateTimeOffset(new DateTime(), TimeSpan.Zero);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeOffsetAsDatetimeoffset == param9));

                DateTime? param10 = new DateTime();
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsDatetime2 == param10));

                DateTime? param11 = new DateTime(2019, 1, 2, 14, 11, 12);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsDatetime == param11));

                DateTime? param12 = new DateTime(2018, 1, 2, 13, 11, 0);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsSmalldatetime == param12));

                TimeSpan? param13 = new TimeSpan(0, 11, 15, 12, 2);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.TimeSpanAsTime == param13));

                var param19 = "C";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsVarcharMax == param19));

                var param20 = "Your";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsCharVaryingMax == param20));

                var param21 = "strong";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsCharacterVaryingMax == param21));

                var param27 = "don't";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsNvarcharMax == param27));

                var param28 = "help";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsNationalCharVaryingMax == param28));

                var param29 = "anyone!";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsNationalCharacterVaryingMax == param29));

                var param35 = new byte[] { 89, 90, 91, 92 };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.BytesAsVarbinaryMax == param35));

                var param36 = new byte[] { 93, 94, 95, 96 };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.BytesAsBinaryVaryingMax == param36));

                decimal? param38 = 102m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Decimal == param38));

                decimal? param39 = 103m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DecimalAsDec == param39));

                decimal? param40 = 104m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DecimalAsNumeric == param40));

                uint? param41 = uint.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UintAsInt == param41));

                ulong? param42 = ulong.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UlongAsBigint == param42));

                ushort? param43 = ushort.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UShortAsSmallint == param43));

                sbyte? param44 = sbyte.MinValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.SbyteAsTinyint == param44));

                uint? param45 = uint.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UintAsBigint == param45));

                ulong? param46 = ulong.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UlongAsDecimal200 == param46));

                ushort? param47 = ushort.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UShortAsInt == param47));

                sbyte? param48 = sbyte.MinValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.SByteAsSmallint == param48));

                Guid? param49 = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47");
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.GuidAsUniqueidentifier == param49));

                char? param50 = 'A';
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsVarcharMax == param50));

                char? param51 = 'B';
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsAsCharVaryingMax == param51));

                char? param52 = 'C';
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsCharacterVaryingMax == param52));

                char? param53 = 'D';
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsNvarcharMax == param53));

                char? param54 = 'E';
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsNationalCharVaryingMax == param54));

                char? param55 = 'F';
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsNationalCharacterVaryingMax == param55));

                char? param58 = 'I';
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsInt == param58));
            }
        }

        [Fact]
        public virtual void Can_query_using_any_mapped_data_types_with_nulls()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Int = 911
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911);

                long? param1 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.LongAsBigint == param1));

                short? param2 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.ShortAsSmallint == param2));
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && (long?)(int?)e.ShortAsSmallint == param2));

                byte? param3 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.ByteAsTinyint == param3));

                bool? param4 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BoolAsBit == param4));

                decimal? param5 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsMoney == param5));

                decimal? param6 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsSmallmoney == param6));

                double? param7a = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DoubleAsFloat == param7a));

                float? param7b = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.FloatAsReal == param7b));

                double? param7c = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DoubleAsDoublePrecision == param7c));

                DateTime? param8 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsDate == param8));

                DateTimeOffset? param9 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeOffsetAsDatetimeoffset == param9));

                DateTime? param10 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsDatetime2 == param10));

                DateTime? param11 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsDatetime == param11));

                DateTime? param12 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsSmalldatetime == param12));

                TimeSpan? param13 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.TimeSpanAsTime == param13));

                string param19 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsVarcharMax == param19));

                string param20 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsCharVaryingMax == param20));

                string param21 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsCharacterVaryingMax == param21));

                string param27 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsNvarcharMax == param27));

                string param28 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsNationalCharVaryingMax == param28));

                string param29 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsNationalCharacterVaryingMax == param29));

                string param30 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsText == param30));

                string param31 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsNtext == param31));

                byte[] param35 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BytesAsVarbinaryMax == param35));

                byte[] param36 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BytesAsBinaryVaryingMax == param36));

                byte[] param37 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BytesAsImage == param37));

                decimal? param38 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Decimal == param38));

                decimal? param39 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsDec == param39));

                decimal? param40 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsNumeric == param40));

                uint? param41 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UintAsInt == param41));

                ulong? param42 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UlongAsBigint == param42));

                ushort? param43 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UShortAsSmallint == param43));

                sbyte? param44 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.SbyteAsTinyint == param44));

                uint? param45 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UintAsBigint == param45));

                ulong? param46 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UlongAsDecimal200 == param46));

                ushort? param47 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UShortAsInt == param47));

                sbyte? param48 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.SByteAsSmallint == param48));

                Guid? param49 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.GuidAsUniqueidentifier == param49));

                char? param50 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsVarcharMax == param50));

                char? param51 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsAsCharVaryingMax == param51));

                char? param52 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsCharacterVaryingMax == param52));

                char? param53 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsNvarcharMax == param53));

                char? param54 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsNationalCharVaryingMax == param54));

                char? param55 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsNationalCharacterVaryingMax == param55));

                char? param56 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsText == param56));

                char? param57 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsNtext == param57));

                char? param58 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsInt == param58));
            }
        }

        [Fact]
        public virtual void Can_query_using_any_nullable_data_type_as_literal()
        {
            Can_query_using_any_nullable_data_type_as_literal_helper(strictEquality: true);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types()
        {
            var entity = CreateMappedDataTypes(77);
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(entity);

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='77'
@p1='True'
@p2='80' (Size = 1)
@p3='0x5D5E5F60' (Nullable = false) (Size = 8000)
@p4='0x61626364' (Nullable = false) (Size = 8000)
@p5='0x595A5B5C' (Nullable = false) (Size = 8000)
@p6='B' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p7='C' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p8='73'
@p9='E' (Nullable = false) (Size = 1)
@p10='F' (Nullable = false) (Size = 1)
@p11='H' (Nullable = false) (Size = 1)
@p12='D' (Nullable = false) (Size = 1)
@p13='G' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p14='A' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p15='2015-01-02T10:11:12' (DbType = Date)
@p16='2019-01-02T14:11:12' (DbType = DateTime)
@p17='2017-01-02T12:11:12'
@p18='2018-01-02T13:11:12' (DbType = DateTime)
@p19='2016-01-02T11:11:12.0000000+00:00'
@p20='101.1'
@p21='102.2'
@p22='81.1'
@p23='103.3'
@p24='82.2'
@p25='85.5'
@p26='83.3'
@p27='84.4'
@p28='a8f9f951-145f-4545-ac60-b92ff57ada47'
@p29='78'
@p30='-128'
@p31='128' (Size = 1)
@p32='79'
@p33='Your' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p34='strong' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p35='help' (Nullable = false) (Size = 4000)
@p36='anyone!' (Nullable = false) (Size = 4000)
@p37='Gumball Rules OK!' (Nullable = false) (Size = 4000)
@p38='" + entity.StringAsNvarcharMax + @"' (Nullable = false) (Size = -1)
@p39='Gumball Rules!' (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p40='" + entity.StringAsVarcharMax + @"' (Nullable = false) (Size = -1) (DbType = AnsiString)
@p41='11:15:12'
@p42='65535'
@p43='-1'
@p44='4294967295'
@p45='-1'
@p46='-1'
@p47='18446744073709551615' (Precision = 20)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 77), 77);
            }
        }

        private string DumpParameters()
            => Fixture.TestSqlLoggerFactory.Parameters.Single().Replace(", ", EOL);

        private static void AssertMappedDataTypes(MappedDataTypes entity, int id)
        {
            var expected = CreateMappedDataTypes(id);
            Assert.Equal(id, entity.Int);
            Assert.Equal(78, entity.LongAsBigInt);
            Assert.Equal(79, entity.ShortAsSmallint);
            Assert.Equal(80, entity.ByteAsTinyint);
            Assert.Equal(uint.MaxValue, entity.UintAsInt);
            Assert.Equal(ulong.MaxValue, entity.UlongAsBigint);
            Assert.Equal(ushort.MaxValue, entity.UShortAsSmallint);
            Assert.Equal(sbyte.MinValue, entity.SByteAsTinyint);
            Assert.True(entity.BoolAsBit);
            Assert.Equal(81.1m, entity.DecimalAsMoney);
            Assert.Equal(82.2m, entity.DecimalAsSmallmoney);
            Assert.Equal(83.3, entity.DoubleAsFloat);
            Assert.Equal(84.4f, entity.FloatAsReal);
            Assert.Equal(85.5, entity.DoubleAsDoublePrecision);
            Assert.Equal(new DateTime(2015, 1, 2), entity.DateTimeAsDate);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.DateTimeOffsetAsDatetimeoffset);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.DateTimeAsDatetime2);
            Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.DateTimeAsSmalldatetime);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.DateTimeAsDatetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
            Assert.Equal(expected.StringAsVarcharMax, entity.StringAsVarcharMax);
            Assert.Equal("Your", entity.StringAsAsCharVaryingMax);
            Assert.Equal("strong", entity.StringAsCharacterVaryingMax);
            Assert.Equal(expected.StringAsNvarcharMax, entity.StringAsNvarcharMax);
            Assert.Equal("help", entity.StringAsNationalCharVaryingMax);
            Assert.Equal("anyone!", entity.StringAsNationalCharacterVaryingMax);
            Assert.Equal("Gumball Rules!", entity.StringAsText);
            Assert.Equal("Gumball Rules OK!", entity.StringAsNtext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.BytesAsVarbinaryMax);
            Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.BytesAsBinaryVaryingMax);
            Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.BytesAsImage);
            Assert.Equal(101m, entity.Decimal);
            Assert.Equal(102m, entity.DecimalAsDec);
            Assert.Equal(103m, entity.DecimalAsNumeric);
            Assert.Equal(new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"), entity.GuidAsUniqueidentifier);
            Assert.Equal(uint.MaxValue, entity.UintAsBigint);
            Assert.Equal(ulong.MaxValue, entity.UlongAsDecimal200);
            Assert.Equal(ushort.MaxValue, entity.UShortAsInt);
            Assert.Equal(sbyte.MinValue, entity.SByteAsSmallint);
            Assert.Equal('A', entity.CharAsVarcharMax);
            Assert.Equal('B', entity.CharAsAsCharVaryingMax);
            Assert.Equal('C', entity.CharAsCharacterVaryingMax);
            Assert.Equal('D', entity.CharAsNvarcharMax);
            Assert.Equal('E', entity.CharAsNationalCharVaryingMax);
            Assert.Equal('F', entity.CharAsNationalCharacterVaryingMax);
            Assert.Equal('G', entity.CharAsText);
            Assert.Equal('H', entity.CharAsNtext);
            Assert.Equal('I', entity.CharAsInt);
        }

        private static MappedDataTypes CreateMappedDataTypes(int id)
            => new MappedDataTypes
            {
                Int = id,
                LongAsBigInt = 78L,
                ShortAsSmallint = 79,
                ByteAsTinyint = 80,
                UintAsInt = uint.MaxValue,
                UlongAsBigint = ulong.MaxValue,
                UShortAsSmallint = ushort.MaxValue,
                SByteAsTinyint = sbyte.MinValue,
                BoolAsBit = true,
                DecimalAsMoney = 81.1m,
                DecimalAsSmallmoney = 82.2m,
                DoubleAsFloat = 83.3,
                FloatAsReal = 84.4f,
                DoubleAsDoublePrecision = 85.5,
                DateTimeAsDate = new DateTime(2015, 1, 2, 10, 11, 12),
                DateTimeOffsetAsDatetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                DateTimeAsDatetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                DateTimeAsSmalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
                TimeSpanAsTime = new TimeSpan(11, 15, 12),
                StringAsVarcharMax = string.Concat(Enumerable.Repeat("C", 8001)),
                StringAsAsCharVaryingMax = "Your",
                StringAsCharacterVaryingMax = "strong",
                StringAsNvarcharMax = string.Concat(Enumerable.Repeat("D", 4001)),
                StringAsNationalCharVaryingMax = "help",
                StringAsNationalCharacterVaryingMax = "anyone!",
                StringAsText = "Gumball Rules!",
                StringAsNtext = "Gumball Rules OK!",
                BytesAsVarbinaryMax = new byte[] { 89, 90, 91, 92 },
                BytesAsBinaryVaryingMax = new byte[] { 93, 94, 95, 96 },
                BytesAsImage = new byte[] { 97, 98, 99, 100 },
                Decimal = 101.1m,
                DecimalAsDec = 102.2m,
                DecimalAsNumeric = 103.3m,
                GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
                UintAsBigint = uint.MaxValue,
                UlongAsDecimal200 = ulong.MaxValue,
                UShortAsInt = ushort.MaxValue,
                SByteAsSmallint = sbyte.MinValue,
                CharAsVarcharMax = 'A',
                CharAsAsCharVaryingMax = 'B',
                CharAsCharacterVaryingMax = 'C',
                CharAsNvarcharMax = 'D',
                CharAsNationalCharVaryingMax = 'E',
                CharAsNationalCharacterVaryingMax = 'F',
                CharAsText = 'G',
                CharAsNtext = 'H',
                CharAsInt = 'I'
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='77'
@p1='True' (Nullable = true)
@p2='80' (Nullable = true) (Size = 1)
@p3='0x5D5E5F60' (Size = 8000)
@p4='0x61626364' (Size = 8000)
@p5='0x595A5B5C' (Size = 8000)
@p6='B' (Size = 1) (DbType = AnsiString)
@p7='C' (Size = 1) (DbType = AnsiString)
@p8='73' (Nullable = true)
@p9='E' (Size = 1)
@p10='F' (Size = 1)
@p11='H' (Size = 1)
@p12='D' (Size = 1)
@p13='G' (Size = 1) (DbType = AnsiString)
@p14='A' (Size = 1) (DbType = AnsiString)
@p15='2015-01-02T10:11:12' (Nullable = true) (DbType = Date)
@p16='2019-01-02T14:11:12' (Nullable = true) (DbType = DateTime)
@p17='2017-01-02T12:11:12' (Nullable = true)
@p18='2018-01-02T13:11:12' (Nullable = true) (DbType = DateTime)
@p19='2016-01-02T11:11:12.0000000+00:00' (Nullable = true)
@p20='101.1' (Nullable = true)
@p21='102.2' (Nullable = true)
@p22='81.1' (Nullable = true)
@p23='103.3' (Nullable = true)
@p24='82.2' (Nullable = true)
@p25='85.5' (Nullable = true)
@p26='83.3' (Nullable = true)
@p27='84.4' (Nullable = true)
@p28='a8f9f951-145f-4545-ac60-b92ff57ada47' (Nullable = true)
@p29='78' (Nullable = true)
@p30='-128' (Nullable = true)
@p31='128' (Nullable = true) (Size = 1)
@p32='79' (Nullable = true)
@p33='Your' (Size = 8000) (DbType = AnsiString)
@p34='strong' (Size = 8000) (DbType = AnsiString)
@p35='help' (Size = 4000)
@p36='anyone!' (Size = 4000)
@p37='Gumball Rules OK!' (Size = 4000)
@p38='don't' (Size = 4000)
@p39='Gumball Rules!' (Size = 8000) (DbType = AnsiString)
@p40='C' (Size = 8000) (DbType = AnsiString)
@p41='11:15:12' (Nullable = true)
@p42='65535' (Nullable = true)
@p43='-1' (Nullable = true)
@p44='4294967295' (Nullable = true)
@p45='-1' (Nullable = true)
@p46='-1' (Nullable = true)
@p47='18446744073709551615' (Nullable = true) (Precision = 20)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedNullableDataTypes(MappedNullableDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(78, entity.LongAsBigint);
            Assert.Equal(79, entity.ShortAsSmallint.Value);
            Assert.Equal(80, entity.ByteAsTinyint.Value);
            Assert.Equal(uint.MaxValue, entity.UintAsInt);
            Assert.Equal(ulong.MaxValue, entity.UlongAsBigint);
            Assert.Equal(ushort.MaxValue, entity.UShortAsSmallint);
            Assert.Equal(sbyte.MinValue, entity.SbyteAsTinyint);
            Assert.True(entity.BoolAsBit);
            Assert.Equal(81.1m, entity.DecimalAsMoney);
            Assert.Equal(82.2m, entity.DecimalAsSmallmoney);
            Assert.Equal(83.3, entity.DoubleAsFloat);
            Assert.Equal(84.4f, entity.FloatAsReal);
            Assert.Equal(85.5, entity.DoubleAsDoublePrecision);
            Assert.Equal(new DateTime(2015, 1, 2), entity.DateTimeAsDate);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.DateTimeOffsetAsDatetimeoffset);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.DateTimeAsDatetime2);
            Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.DateTimeAsSmalldatetime);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.DateTimeAsDatetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
            Assert.Equal("C", entity.StringAsVarcharMax);
            Assert.Equal("Your", entity.StringAsCharVaryingMax);
            Assert.Equal("strong", entity.StringAsCharacterVaryingMax);
            Assert.Equal("don't", entity.StringAsNvarcharMax);
            Assert.Equal("help", entity.StringAsNationalCharVaryingMax);
            Assert.Equal("anyone!", entity.StringAsNationalCharacterVaryingMax);
            Assert.Equal("Gumball Rules!", entity.StringAsText);
            Assert.Equal("Gumball Rules OK!", entity.StringAsNtext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.BytesAsVarbinaryMax);
            Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.BytesAsBinaryVaryingMax);
            Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.BytesAsImage);
            Assert.Equal(101m, entity.Decimal);
            Assert.Equal(102m, entity.DecimalAsDec);
            Assert.Equal(103m, entity.DecimalAsNumeric);
            Assert.Equal(new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"), entity.GuidAsUniqueidentifier);
            Assert.Equal(uint.MaxValue, entity.UintAsBigint);
            Assert.Equal(ulong.MaxValue, entity.UlongAsDecimal200);
            Assert.Equal(ushort.MaxValue, entity.UShortAsInt);
            Assert.Equal(sbyte.MinValue, entity.SByteAsSmallint);
            Assert.Equal('A', entity.CharAsVarcharMax);
            Assert.Equal('B', entity.CharAsAsCharVaryingMax);
            Assert.Equal('C', entity.CharAsCharacterVaryingMax);
            Assert.Equal('D', entity.CharAsNvarcharMax);
            Assert.Equal('E', entity.CharAsNationalCharVaryingMax);
            Assert.Equal('F', entity.CharAsNationalCharacterVaryingMax);
            Assert.Equal('G', entity.CharAsText);
            Assert.Equal('H', entity.CharAsNtext);
            Assert.Equal('I', entity.CharAsInt);
        }

        private static MappedNullableDataTypes CreateMappedNullableDataTypes(int id)
            => new MappedNullableDataTypes
            {
                Int = id,
                LongAsBigint = 78L,
                ShortAsSmallint = 79,
                ByteAsTinyint = 80,
                UintAsInt = uint.MaxValue,
                UlongAsBigint = ulong.MaxValue,
                UShortAsSmallint = ushort.MaxValue,
                SbyteAsTinyint = sbyte.MinValue,
                BoolAsBit = true,
                DecimalAsMoney = 81.1m,
                DecimalAsSmallmoney = 82.2m,
                DoubleAsFloat = 83.3,
                FloatAsReal = 84.4f,
                DoubleAsDoublePrecision = 85.5,
                DateTimeAsDate = new DateTime(2015, 1, 2, 10, 11, 12),
                DateTimeOffsetAsDatetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                DateTimeAsDatetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                DateTimeAsSmalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
                TimeSpanAsTime = new TimeSpan(11, 15, 12),
                StringAsVarcharMax = "C",
                StringAsCharVaryingMax = "Your",
                StringAsCharacterVaryingMax = "strong",
                StringAsNvarcharMax = "don't",
                StringAsNationalCharVaryingMax = "help",
                StringAsNationalCharacterVaryingMax = "anyone!",
                StringAsText = "Gumball Rules!",
                StringAsNtext = "Gumball Rules OK!",
                BytesAsVarbinaryMax = new byte[] { 89, 90, 91, 92 },
                BytesAsBinaryVaryingMax = new byte[] { 93, 94, 95, 96 },
                BytesAsImage = new byte[] { 97, 98, 99, 100 },
                Decimal = 101.1m,
                DecimalAsDec = 102.2m,
                DecimalAsNumeric = 103.3m,
                GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
                UintAsBigint = uint.MaxValue,
                UlongAsDecimal200 = ulong.MaxValue,
                UShortAsInt = ushort.MaxValue,
                SByteAsSmallint = sbyte.MinValue,
                CharAsVarcharMax = 'A',
                CharAsAsCharVaryingMax = 'B',
                CharAsCharacterVaryingMax = 'C',
                CharAsNvarcharMax = 'D',
                CharAsNationalCharVaryingMax = 'E',
                CharAsNationalCharacterVaryingMax = 'F',
                CharAsText = 'G',
                CharAsNtext = 'H',
                CharAsInt = 'I'
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='78'
@p1='' (DbType = String)
@p2='' (DbType = Byte)
@p3='' (Size = 8000) (DbType = Binary)
@p4='' (Size = 8000) (DbType = Binary)
@p5='' (Size = 8000) (DbType = Binary)
@p6=''
@p7=''
@p8='' (DbType = Int32)
@p9='' (DbType = String)
@p10='' (DbType = String)
@p11='' (DbType = String)
@p12='' (DbType = String)
@p13=''
@p14=''
@p15='' (DbType = Date)
@p16='' (DbType = DateTime)
@p17='' (DbType = DateTime2)
@p18='' (DbType = DateTime)
@p19='' (DbType = DateTimeOffset)
@p20='' (DbType = String)
@p21='' (DbType = String)
@p22='' (DbType = String)
@p23='' (DbType = String)
@p24='' (DbType = String)
@p25='' (DbType = String)
@p26='' (DbType = String)
@p27='' (DbType = String)
@p28='' (DbType = Guid)
@p29='' (DbType = Int64)
@p30='' (DbType = Int16)
@p31='' (DbType = Byte)
@p32='' (DbType = Int16)
@p33='' (Size = 8000)
@p34='' (Size = 8000)
@p35='' (Size = 4000) (DbType = String)
@p36='' (Size = 4000) (DbType = String)
@p37='' (Size = 4000) (DbType = String)
@p38='' (Size = 4000) (DbType = String)
@p39='' (Size = 8000)
@p40='' (Size = 8000)
@p41='' (DbType = String)
@p42='' (DbType = Int32)
@p43='' (DbType = Int16)
@p44='' (DbType = Int64)
@p45='' (DbType = Int32)
@p46='' (DbType = Int64)
@p47='' (DbType = Decimal)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 78), 78);
            }
        }

        private static void AssertNullMappedNullableDataTypes(MappedNullableDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Null(entity.LongAsBigint);
            Assert.Null(entity.ShortAsSmallint);
            Assert.Null(entity.ByteAsTinyint);
            Assert.Null(entity.UintAsInt);
            Assert.Null(entity.UlongAsBigint);
            Assert.Null(entity.UShortAsSmallint);
            Assert.Null(entity.SbyteAsTinyint);
            Assert.Null(entity.BoolAsBit);
            Assert.Null(entity.DecimalAsMoney);
            Assert.Null(entity.DecimalAsSmallmoney);
            Assert.Null(entity.DoubleAsFloat);
            Assert.Null(entity.FloatAsReal);
            Assert.Null(entity.DoubleAsDoublePrecision);
            Assert.Null(entity.DateTimeAsDate);
            Assert.Null(entity.DateTimeOffsetAsDatetimeoffset);
            Assert.Null(entity.DateTimeAsDatetime2);
            Assert.Null(entity.DateTimeAsSmalldatetime);
            Assert.Null(entity.DateTimeAsDatetime);
            Assert.Null(entity.TimeSpanAsTime);
            Assert.Null(entity.StringAsVarcharMax);
            Assert.Null(entity.StringAsCharVaryingMax);
            Assert.Null(entity.StringAsCharacterVaryingMax);
            Assert.Null(entity.StringAsNvarcharMax);
            Assert.Null(entity.StringAsNationalCharVaryingMax);
            Assert.Null(entity.StringAsNationalCharacterVaryingMax);
            Assert.Null(entity.StringAsText);
            Assert.Null(entity.StringAsNtext);
            Assert.Null(entity.BytesAsVarbinaryMax);
            Assert.Null(entity.BytesAsBinaryVaryingMax);
            Assert.Null(entity.BytesAsImage);
            Assert.Null(entity.Decimal);
            Assert.Null(entity.DecimalAsDec);
            Assert.Null(entity.DecimalAsNumeric);
            Assert.Null(entity.GuidAsUniqueidentifier);
            Assert.Null(entity.UintAsBigint);
            Assert.Null(entity.UlongAsDecimal200);
            Assert.Null(entity.UShortAsInt);
            Assert.Null(entity.SByteAsSmallint);
            Assert.Null(entity.CharAsVarcharMax);
            Assert.Null(entity.CharAsAsCharVaryingMax);
            Assert.Null(entity.CharAsCharacterVaryingMax);
            Assert.Null(entity.CharAsNvarcharMax);
            Assert.Null(entity.CharAsNationalCharVaryingMax);
            Assert.Null(entity.CharAsNationalCharacterVaryingMax);
            Assert.Null(entity.CharAsText);
            Assert.Null(entity.CharAsNtext);
            Assert.Null(entity.CharAsInt);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='77'
@p1='0x0A0B0C' (Size = 3)
@p2='0x0C0D0E' (Size = 3)
@p3='0x0B0C0D' (Size = 3)
@p4='B' (Size = 1) (DbType = AnsiString)
@p5='C' (Size = 1) (DbType = AnsiString)
@p6='E' (Size = 1)
@p7='F' (Size = 1)
@p8='D' (Size = 1)
@p9='A' (Size = 1) (DbType = AnsiString)
@p10='Wor' (Size = 3) (DbType = AnsiString)
@p11='Thr' (Size = 3) (DbType = AnsiString)
@p12='Lon' (Size = 3) (DbType = AnsiString)
@p13='Let' (Size = 3) (DbType = AnsiString)
@p14='The' (Size = 3)
@p15='Squ' (Size = 3)
@p16='Col' (Size = 3)
@p17='Won' (Size = 3)
@p18='Int' (Size = 3)
@p19='Tha' (Size = 3) (DbType = AnsiString)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 77), 77);
            }
        }

        private static void AssertMappedSizedDataTypes(MappedSizedDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Equal("Wor", entity.StringAsChar3);
            Assert.Equal("Lon", entity.StringAsCharacter3);
            Assert.Equal("Tha", entity.StringAsVarchar3);
            Assert.Equal("Thr", entity.StringAsCharVarying3);
            Assert.Equal("Let", entity.StringAsCharacterVarying3);
            Assert.Equal("Won", entity.StringAsNchar3);
            Assert.Equal("Squ", entity.StringAsNationalCharacter3);
            Assert.Equal("Int", entity.StringAsNvarchar3);
            Assert.Equal("The", entity.StringAsNationalCharVarying3);
            Assert.Equal("Col", entity.StringAsNationalCharacterVarying3);
            Assert.Equal(new byte[] { 10, 11, 12 }, entity.BytesAsBinary3);
            Assert.Equal(new byte[] { 11, 12, 13 }, entity.BytesAsVarbinary3);
            Assert.Equal(new byte[] { 12, 13, 14 }, entity.BytesAsBinaryVarying3);
            Assert.Equal('A', entity.CharAsVarchar3);
            Assert.Equal('B', entity.CharAsAsCharVarying3);
            Assert.Equal('C', entity.CharAsCharacterVarying3);
            Assert.Equal('D', entity.CharAsNvarchar3);
            Assert.Equal('E', entity.CharAsNationalCharVarying3);
            Assert.Equal('F', entity.CharAsNationalCharacterVarying3);
        }

        private static MappedSizedDataTypes CreateMappedSizedDataTypes(int id)
            => new MappedSizedDataTypes
            {
                Id = id,
                StringAsChar3 = "Wor",
                StringAsCharacter3 = "Lon",
                StringAsVarchar3 = "Tha",
                StringAsCharVarying3 = "Thr",
                StringAsCharacterVarying3 = "Let",
                StringAsNchar3 = "Won",
                StringAsNationalCharacter3 = "Squ",
                StringAsNvarchar3 = "Int",
                StringAsNationalCharVarying3 = "The",
                StringAsNationalCharacterVarying3 = "Col",
                BytesAsBinary3 = new byte[] { 10, 11, 12 },
                BytesAsVarbinary3 = new byte[] { 11, 12, 13 },
                BytesAsBinaryVarying3 = new byte[] { 12, 13, 14 },
                CharAsVarchar3 = 'A',
                CharAsAsCharVarying3 = 'B',
                CharAsCharacterVarying3 = 'C',
                CharAsNvarchar3 = 'D',
                CharAsNationalCharVarying3 = 'E',
                CharAsNationalCharacterVarying3 = 'F'
            };

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='78'
@p1='' (Size = 3) (DbType = Binary)
@p2='' (Size = 3) (DbType = Binary)
@p3='' (Size = 3) (DbType = Binary)
@p4=''
@p5=''
@p6='' (DbType = String)
@p7='' (DbType = String)
@p8='' (DbType = String)
@p9=''
@p10='' (Size = 3)
@p11='' (Size = 3)
@p12='' (Size = 3)
@p13='' (Size = 3)
@p14='' (Size = 3) (DbType = String)
@p15='' (Size = 3) (DbType = String)
@p16='' (Size = 3) (DbType = String)
@p17='' (Size = 3) (DbType = String)
@p18='' (Size = 3) (DbType = String)
@p19='' (Size = 3)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 78), 78);
            }
        }

        private static void AssertNullMappedSizedDataTypes(MappedSizedDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Null(entity.StringAsChar3);
            Assert.Null(entity.StringAsCharacter3);
            Assert.Null(entity.StringAsVarchar3);
            Assert.Null(entity.StringAsCharVarying3);
            Assert.Null(entity.StringAsCharacterVarying3);
            Assert.Null(entity.StringAsNchar3);
            Assert.Null(entity.StringAsNationalCharacter3);
            Assert.Null(entity.StringAsNvarchar3);
            Assert.Null(entity.StringAsNationalCharVarying3);
            Assert.Null(entity.StringAsNationalCharacterVarying3);
            Assert.Null(entity.BytesAsBinary3);
            Assert.Null(entity.BytesAsVarbinary3);
            Assert.Null(entity.BytesAsBinaryVarying3);
            Assert.Null(entity.CharAsVarchar3);
            Assert.Null(entity.CharAsAsCharVarying3);
            Assert.Null(entity.CharAsCharacterVarying3);
            Assert.Null(entity.CharAsNvarchar3);
            Assert.Null(entity.CharAsNationalCharVarying3);
            Assert.Null(entity.CharAsNationalCharacterVarying3);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='77'
@p1='2017-01-02T12:11:12'
@p2='2016-01-02T11:11:12.0000000+00:00'
@p3='102.2'
@p4='101.1'
@p5='103.3'
@p6='85.5'
@p7='83.3'",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 77), 77);
            }
        }

        private static void AssertMappedScaledDataTypes(MappedScaledDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Equal(83.3f, entity.FloatAsFloat3);
            Assert.Equal(85.5f, entity.FloatAsDoublePrecision3);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.DateTimeOffsetAsDatetimeoffset3);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.DateTimeAsDatetime23);
            Assert.Equal(101m, entity.DecimalAsDecimal3);
            Assert.Equal(102m, entity.DecimalAsDec3);
            Assert.Equal(103m, entity.DecimalAsNumeric3);
        }

        private static MappedScaledDataTypes CreateMappedScaledDataTypes(int id)
            => new MappedScaledDataTypes
            {
                Id = id,
                FloatAsFloat3 = 83.3f,
                FloatAsDoublePrecision3 = 85.5f,
                DateTimeOffsetAsDatetimeoffset3 = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                DateTimeAsDatetime23 = new DateTime(2017, 1, 2, 12, 11, 12),
                DecimalAsDecimal3 = 101.1m,
                DecimalAsDec3 = 102.2m,
                DecimalAsNumeric3 = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='77'
@p1='102.2'
@p2='101.1'
@p3='103.3'",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedPrecisionAndScaledDataTypes(context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 77), 77);
            }
        }

        private static void AssertMappedPrecisionAndScaledDataTypes(MappedPrecisionAndScaledDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Equal(101.1m, entity.DecimalAsDecimal52);
            Assert.Equal(102.2m, entity.DecimalAsDec52);
            Assert.Equal(103.3m, entity.DecimalAsNumeric52);
        }

        private static MappedPrecisionAndScaledDataTypes CreateMappedPrecisionAndScaledDataTypes(int id)
            => new MappedPrecisionAndScaledDataTypes
            {
                Id = id,
                DecimalAsDecimal52 = 101.1m,
                DecimalAsDec52 = 102.2m,
                DecimalAsNumeric52 = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='True'
@p1='80' (Size = 1)
@p2='0x5D5E5F60' (Size = 8000)
@p3='0x61626364' (Size = 8000)
@p4='0x595A5B5C' (Size = 8000)
@p5='B' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p6='C' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p7='73'
@p8='E' (Nullable = false) (Size = 1)
@p9='F' (Nullable = false) (Size = 1)
@p10='H' (Nullable = false) (Size = 1)
@p11='D' (Nullable = false) (Size = 1)
@p12='G' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p13='A' (Nullable = false) (Size = 1) (DbType = AnsiString)
@p14='2015-01-02T10:11:12' (DbType = Date)
@p15='2019-01-02T14:11:12' (DbType = DateTime)
@p16='2017-01-02T12:11:12'
@p17='2018-01-02T13:11:12' (DbType = DateTime)
@p18='2016-01-02T11:11:12.0000000+00:00'
@p19='101.1'
@p20='102.2'
@p21='81.1'
@p22='103.3'
@p23='82.2'
@p24='85.5'
@p25='83.3'
@p26='84.4'
@p27='a8f9f951-145f-4545-ac60-b92ff57ada47'
@p28='77'
@p29='78'
@p30='-128'
@p31='128' (Size = 1)
@p32='79'
@p33='Your' (Size = 8000) (DbType = AnsiString)
@p34='strong' (Size = 8000) (DbType = AnsiString)
@p35='help' (Size = 4000)
@p36='anyone!' (Size = 4000)
@p37='Gumball Rules OK!' (Size = 4000)
@p38='don't' (Size = 4000)
@p39='Gumball Rules!' (Size = 8000) (DbType = AnsiString)
@p40='C' (Size = 8000) (DbType = AnsiString)
@p41='11:15:12'
@p42='65535'
@p43='-1'
@p44='4294967295'
@p45='-1'
@p46='-1'
@p47='18446744073709551615' (Precision = 20)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedDataTypesWithIdentity(MappedDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(78, entity.LongAsBigint);
            Assert.Equal(79, entity.ShortAsSmallint);
            Assert.Equal(80, entity.ByteAsTinyint);
            Assert.Equal(uint.MaxValue, entity.UintAsInt);
            Assert.Equal(ulong.MaxValue, entity.UlongAsBigint);
            Assert.Equal(ushort.MaxValue, entity.UShortAsSmallint);
            Assert.Equal(sbyte.MinValue, entity.SbyteAsTinyint);
            Assert.True(entity.BoolAsBit);
            Assert.Equal(81.1m, entity.DecimalAsMoney);
            Assert.Equal(82.2m, entity.DecimalAsSmallmoney);
            Assert.Equal(83.3, entity.DoubleAsFloat);
            Assert.Equal(84.4f, entity.FloatAsReal);
            Assert.Equal(85.5, entity.DoubleAsDoublePrecision);
            Assert.Equal(new DateTime(2015, 1, 2), entity.DateTimeAsDate);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.DateTimeOffsetAsDatetimeoffset);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.DateTimeAsDatetime2);
            Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.DateTimeAsSmalldatetime);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.DateTimeAsDatetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
            Assert.Equal("C", entity.StringAsVarcharMax);
            Assert.Equal("Your", entity.StringAsCharVaryingMax);
            Assert.Equal("strong", entity.StringAsCharacterVaryingMax);
            Assert.Equal("don't", entity.StringAsNvarcharMax);
            Assert.Equal("help", entity.StringAsNationalCharVaryingMax);
            Assert.Equal("anyone!", entity.StringAsNationalCharacterVaryingMax);
            Assert.Equal("Gumball Rules!", entity.StringAsText);
            Assert.Equal("Gumball Rules OK!", entity.StringAsNtext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.BytesAsVarbinaryMax);
            Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.BytesAsBinaryVaryingMax);
            Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.BytesAsImage);
            Assert.Equal(101m, entity.Decimal);
            Assert.Equal(102m, entity.DecimalAsDec);
            Assert.Equal(103m, entity.DecimalAsNumeric);
            Assert.Equal(new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"), entity.GuidAsUniqueidentifier);
            Assert.Equal(uint.MaxValue, entity.UintAsBigint);
            Assert.Equal(ulong.MaxValue, entity.UlongAsDecimal200);
            Assert.Equal(ushort.MaxValue, entity.UShortAsInt);
            Assert.Equal(sbyte.MinValue, entity.SByteAsSmallint);
            Assert.Equal('A', entity.CharAsVarcharMax);
            Assert.Equal('B', entity.CharAsAsCharVaryingMax);
            Assert.Equal('C', entity.CharAsCharacterVaryingMax);
            Assert.Equal('D', entity.CharAsNvarcharMax);
            Assert.Equal('E', entity.CharAsNationalCharVaryingMax);
            Assert.Equal('F', entity.CharAsNationalCharacterVaryingMax);
            Assert.Equal('G', entity.CharAsText);
            Assert.Equal('H', entity.CharAsNtext);
            Assert.Equal('I', entity.CharAsInt);
        }

        private static MappedDataTypesWithIdentity CreateMappedDataTypesWithIdentity(int id)
            => new MappedDataTypesWithIdentity
            {
                Int = id,
                LongAsBigint = 78L,
                ShortAsSmallint = 79,
                ByteAsTinyint = 80,
                UintAsInt = uint.MaxValue,
                UlongAsBigint = ulong.MaxValue,
                UShortAsSmallint = ushort.MaxValue,
                SbyteAsTinyint = sbyte.MinValue,
                BoolAsBit = true,
                DecimalAsMoney = 81.1m,
                DecimalAsSmallmoney = 82.2m,
                DoubleAsFloat = 83.3,
                FloatAsReal = 84.4f,
                DoubleAsDoublePrecision = 85.5,
                DateTimeAsDate = new DateTime(2015, 1, 2, 10, 11, 12),
                DateTimeOffsetAsDatetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                DateTimeAsDatetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                DateTimeAsSmalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
                TimeSpanAsTime = new TimeSpan(11, 15, 12),
                StringAsVarcharMax = "C",
                StringAsCharVaryingMax = "Your",
                StringAsCharacterVaryingMax = "strong",
                StringAsNvarcharMax = "don't",
                StringAsNationalCharVaryingMax = "help",
                StringAsNationalCharacterVaryingMax = "anyone!",
                StringAsText = "Gumball Rules!",
                StringAsNtext = "Gumball Rules OK!",
                BytesAsVarbinaryMax = new byte[] { 89, 90, 91, 92 },
                BytesAsBinaryVaryingMax = new byte[] { 93, 94, 95, 96 },
                BytesAsImage = new byte[] { 97, 98, 99, 100 },
                Decimal = 101.1m,
                DecimalAsDec = 102.2m,
                DecimalAsNumeric = 103.3m,
                GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
                UintAsBigint = uint.MaxValue,
                UlongAsDecimal200 = ulong.MaxValue,
                UShortAsInt = ushort.MaxValue,
                SByteAsSmallint = sbyte.MinValue,
                CharAsVarcharMax = 'A',
                CharAsAsCharVaryingMax = 'B',
                CharAsCharacterVaryingMax = 'C',
                CharAsNvarcharMax = 'D',
                CharAsNationalCharVaryingMax = 'E',
                CharAsNationalCharacterVaryingMax = 'F',
                CharAsText = 'G',
                CharAsNtext = 'H',
                CharAsInt = 'I'
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='True' (Nullable = true)
@p1='80' (Nullable = true) (Size = 1)
@p2='0x61626364' (Size = 8000)
@p3='0x595A5B5C' (Size = 8000)
@p4='0x5D5E5F60' (Size = 8000)
@p5='B' (Size = 1) (DbType = AnsiString)
@p6='C' (Size = 1) (DbType = AnsiString)
@p7='73' (Nullable = true)
@p8='E' (Size = 1)
@p9='F' (Size = 1)
@p10='H' (Size = 1)
@p11='D' (Size = 1)
@p12='G' (Size = 1) (DbType = AnsiString)
@p13='A' (Size = 1) (DbType = AnsiString)
@p14='2015-01-02T10:11:12' (Nullable = true) (DbType = Date)
@p15='2019-01-02T14:11:12' (Nullable = true) (DbType = DateTime)
@p16='2017-01-02T12:11:12' (Nullable = true)
@p17='2018-01-02T13:11:12' (Nullable = true) (DbType = DateTime)
@p18='2016-01-02T11:11:12.0000000+00:00' (Nullable = true)
@p19='101.1' (Nullable = true)
@p20='102.2' (Nullable = true)
@p21='81.1' (Nullable = true)
@p22='103.3' (Nullable = true)
@p23='82.2' (Nullable = true)
@p24='85.5' (Nullable = true)
@p25='83.3' (Nullable = true)
@p26='84.4' (Nullable = true)
@p27='a8f9f951-145f-4545-ac60-b92ff57ada47' (Nullable = true)
@p28='77' (Nullable = true)
@p29='78' (Nullable = true)
@p30='-128' (Nullable = true)
@p31='128' (Nullable = true) (Size = 1)
@p32='79' (Nullable = true)
@p33='Your' (Size = 8000) (DbType = AnsiString)
@p34='strong' (Size = 8000) (DbType = AnsiString)
@p35='help' (Size = 4000)
@p36='anyone!' (Size = 4000)
@p37='Gumball Rules OK!' (Size = 4000)
@p38='don't' (Size = 4000)
@p39='Gumball Rules!' (Size = 8000) (DbType = AnsiString)
@p40='C' (Size = 8000) (DbType = AnsiString)
@p41='11:15:12' (Nullable = true)
@p42='65535' (Nullable = true)
@p43='4294967295' (Nullable = true)
@p44='-1' (Nullable = true)
@p45='-1' (Nullable = true)
@p46='18446744073709551615' (Nullable = true) (Precision = 20)
@p47='-1' (Nullable = true)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedNullableDataTypesWithIdentity(MappedNullableDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(78, entity.LongAsBigint);
            Assert.Equal(79, entity.ShortAsSmallint.Value);
            Assert.Equal(80, entity.ByteAsTinyint.Value);
            Assert.Equal(uint.MaxValue, entity.UintAsInt);
            Assert.Equal(ulong.MaxValue, entity.UlongAsBigint);
            Assert.Equal(ushort.MaxValue, entity.UshortAsSmallint);
            Assert.Equal(sbyte.MinValue, entity.SbyteAsTinyint);
            Assert.True(entity.BoolAsBit);
            Assert.Equal(81.1m, entity.DecimalAsMoney);
            Assert.Equal(82.2m, entity.DecimalAsSmallmoney);
            Assert.Equal(83.3, entity.DoubleAsFloat);
            Assert.Equal(84.4f, entity.FloatAsReal);
            Assert.Equal(85.5, entity.DoubkleAsDoublePrecision);
            Assert.Equal(new DateTime(2015, 1, 2), entity.DateTimeAsDate);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.DateTimeOffsetAsDatetimeoffset);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.DateTimeAsDatetime2);
            Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.DateTimeAsSmalldatetime);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.DateTimeAsDatetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
            Assert.Equal("C", entity.StringAsVarcharMax);
            Assert.Equal("Your", entity.StringAsCharVaryingMax);
            Assert.Equal("strong", entity.StringAsCharacterVaryingMax);
            Assert.Equal("don't", entity.StringAsNvarcharMax);
            Assert.Equal("help", entity.StringAsNationalCharVaryingMax);
            Assert.Equal("anyone!", entity.StringAsNationalCharacterVaryingMax);
            Assert.Equal("Gumball Rules!", entity.StringAsText);
            Assert.Equal("Gumball Rules OK!", entity.StringAsNtext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.BytesAsVarbinaryMax);
            Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.BytesAsVaryingMax);
            Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.BytesAsImage);
            Assert.Equal(101m, entity.Decimal);
            Assert.Equal(102m, entity.DecimalAsDec);
            Assert.Equal(103m, entity.DecimalAsNumeric);
            Assert.Equal(new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"), entity.GuidAsUniqueidentifier);
            Assert.Equal(uint.MaxValue, entity.UintAsBigint);
            Assert.Equal(ulong.MaxValue, entity.UlongAsDecimal200);
            Assert.Equal(ushort.MaxValue, entity.UShortAsInt);
            Assert.Equal(sbyte.MinValue, entity.SByteAsSmallint);
            Assert.Equal('A', entity.CharAsVarcharMax);
            Assert.Equal('B', entity.CharAsAsCharVaryingMax);
            Assert.Equal('C', entity.CharAsCharacterVaryingMax);
            Assert.Equal('D', entity.CharAsNvarcharMax);
            Assert.Equal('E', entity.CharAsNationalCharVaryingMax);
            Assert.Equal('F', entity.CharAsNationalCharacterVaryingMax);
            Assert.Equal('G', entity.CharAsText);
            Assert.Equal('H', entity.CharAsNtext);
            Assert.Equal('I', entity.CharAsInt);
        }

        private static MappedNullableDataTypesWithIdentity CreateMappedNullableDataTypesWithIdentity(int id)
            => new MappedNullableDataTypesWithIdentity
            {
                Int = id,
                LongAsBigint = 78L,
                ShortAsSmallint = 79,
                ByteAsTinyint = 80,
                UintAsInt = uint.MaxValue,
                UlongAsBigint = ulong.MaxValue,
                UshortAsSmallint = ushort.MaxValue,
                SbyteAsTinyint = sbyte.MinValue,
                BoolAsBit = true,
                DecimalAsMoney = 81.1m,
                DecimalAsSmallmoney = 82.2m,
                DoubleAsFloat = 83.3,
                FloatAsReal = 84.4f,
                DoubkleAsDoublePrecision = 85.5,
                DateTimeAsDate = new DateTime(2015, 1, 2, 10, 11, 12),
                DateTimeOffsetAsDatetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                DateTimeAsDatetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                DateTimeAsSmalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
                TimeSpanAsTime = new TimeSpan(11, 15, 12),
                StringAsVarcharMax = "C",
                StringAsCharVaryingMax = "Your",
                StringAsCharacterVaryingMax = "strong",
                StringAsNvarcharMax = "don't",
                StringAsNationalCharVaryingMax = "help",
                StringAsNationalCharacterVaryingMax = "anyone!",
                StringAsText = "Gumball Rules!",
                StringAsNtext = "Gumball Rules OK!",
                BytesAsVarbinaryMax = new byte[] { 89, 90, 91, 92 },
                BytesAsVaryingMax = new byte[] { 93, 94, 95, 96 },
                BytesAsImage = new byte[] { 97, 98, 99, 100 },
                Decimal = 101.1m,
                DecimalAsDec = 102.2m,
                DecimalAsNumeric = 103.3m,
                GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
                UintAsBigint = uint.MaxValue,
                UlongAsDecimal200 = ulong.MaxValue,
                UShortAsInt = ushort.MaxValue,
                SByteAsSmallint = sbyte.MinValue,
                CharAsVarcharMax = 'A',
                CharAsAsCharVaryingMax = 'B',
                CharAsCharacterVaryingMax = 'C',
                CharAsNvarcharMax = 'D',
                CharAsNationalCharVaryingMax = 'E',
                CharAsNationalCharacterVaryingMax = 'F',
                CharAsText = 'G',
                CharAsNtext = 'H',
                CharAsInt = 'I'
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='' (DbType = String)
@p1='' (DbType = Byte)
@p2='' (Size = 8000) (DbType = Binary)
@p3='' (Size = 8000) (DbType = Binary)
@p4='' (Size = 8000) (DbType = Binary)
@p5=''
@p6=''
@p7='' (DbType = Int32)
@p8='' (DbType = String)
@p9='' (DbType = String)
@p10='' (DbType = String)
@p11='' (DbType = String)
@p12=''
@p13=''
@p14='' (DbType = Date)
@p15='' (DbType = DateTime)
@p16='' (DbType = DateTime2)
@p17='' (DbType = DateTime)
@p18='' (DbType = DateTimeOffset)
@p19='' (DbType = String)
@p20='' (DbType = String)
@p21='' (DbType = String)
@p22='' (DbType = String)
@p23='' (DbType = String)
@p24='' (DbType = String)
@p25='' (DbType = String)
@p26='' (DbType = String)
@p27='' (DbType = Guid)
@p28='78' (Nullable = true)
@p29='' (DbType = Int64)
@p30='' (DbType = Int16)
@p31='' (DbType = Byte)
@p32='' (DbType = Int16)
@p33='' (Size = 8000)
@p34='' (Size = 8000)
@p35='' (Size = 4000) (DbType = String)
@p36='' (Size = 4000) (DbType = String)
@p37='' (Size = 4000) (DbType = String)
@p38='' (Size = 4000) (DbType = String)
@p39='' (Size = 8000)
@p40='' (Size = 8000)
@p41='' (DbType = String)
@p42='' (DbType = Int32)
@p43='' (DbType = Int64)
@p44='' (DbType = Int32)
@p45='' (DbType = Int64)
@p46='' (DbType = Decimal)
@p47='' (DbType = Int16)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 78), 78);
            }
        }

        private static void AssertNullMappedNullableDataTypesWithIdentity(
            MappedNullableDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Null(entity.LongAsBigint);
            Assert.Null(entity.ShortAsSmallint);
            Assert.Null(entity.ByteAsTinyint);
            Assert.Null(entity.UintAsInt);
            Assert.Null(entity.UlongAsBigint);
            Assert.Null(entity.UshortAsSmallint);
            Assert.Null(entity.SbyteAsTinyint);
            Assert.Null(entity.BoolAsBit);
            Assert.Null(entity.DecimalAsMoney);
            Assert.Null(entity.DecimalAsSmallmoney);
            Assert.Null(entity.DoubleAsFloat);
            Assert.Null(entity.FloatAsReal);
            Assert.Null(entity.DoubkleAsDoublePrecision);
            Assert.Null(entity.DateTimeAsDate);
            Assert.Null(entity.DateTimeOffsetAsDatetimeoffset);
            Assert.Null(entity.DateTimeAsDatetime2);
            Assert.Null(entity.DateTimeAsSmalldatetime);
            Assert.Null(entity.DateTimeAsDatetime);
            Assert.Null(entity.TimeSpanAsTime);
            Assert.Null(entity.StringAsVarcharMax);
            Assert.Null(entity.StringAsCharVaryingMax);
            Assert.Null(entity.StringAsCharacterVaryingMax);
            Assert.Null(entity.StringAsNvarcharMax);
            Assert.Null(entity.StringAsNationalCharVaryingMax);
            Assert.Null(entity.StringAsNationalCharacterVaryingMax);
            Assert.Null(entity.StringAsText);
            Assert.Null(entity.StringAsNtext);
            Assert.Null(entity.BytesAsVarbinaryMax);
            Assert.Null(entity.BytesAsVaryingMax);
            Assert.Null(entity.BytesAsImage);
            Assert.Null(entity.Decimal);
            Assert.Null(entity.DecimalAsDec);
            Assert.Null(entity.DecimalAsNumeric);
            Assert.Null(entity.GuidAsUniqueidentifier);
            Assert.Null(entity.UintAsBigint);
            Assert.Null(entity.UlongAsDecimal200);
            Assert.Null(entity.UShortAsInt);
            Assert.Null(entity.SByteAsSmallint);
            Assert.Null(entity.CharAsVarcharMax);
            Assert.Null(entity.CharAsAsCharVaryingMax);
            Assert.Null(entity.CharAsCharacterVaryingMax);
            Assert.Null(entity.CharAsNvarcharMax);
            Assert.Null(entity.CharAsNationalCharVaryingMax);
            Assert.Null(entity.CharAsNationalCharacterVaryingMax);
            Assert.Null(entity.CharAsText);
            Assert.Null(entity.CharAsNtext);
            Assert.Null(entity.CharAsInt);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='0x0A0B0C' (Size = 3)
@p1='0x0C0D0E' (Size = 3)
@p2='0x0B0C0D' (Size = 3)
@p3='B' (Size = 1) (DbType = AnsiString)
@p4='C' (Size = 1) (DbType = AnsiString)
@p5='E' (Size = 1)
@p6='F' (Size = 1)
@p7='D' (Size = 1)
@p8='A' (Size = 1) (DbType = AnsiString)
@p9='77'
@p10='Wor' (Size = 3) (DbType = AnsiString)
@p11='Thr' (Size = 3) (DbType = AnsiString)
@p12='Lon' (Size = 3) (DbType = AnsiString)
@p13='Let' (Size = 3) (DbType = AnsiString)
@p14='The' (Size = 3)
@p15='Squ' (Size = 3)
@p16='Col' (Size = 3)
@p17='Won' (Size = 3)
@p18='Int' (Size = 3)
@p19='Tha' (Size = 3) (DbType = AnsiString)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedSizedDataTypesWithIdentity(MappedSizedDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal("Wor", entity.StringAsChar3);
            Assert.Equal("Lon", entity.StringAsCharacter3);
            Assert.Equal("Tha", entity.StringAsVarchar3);
            Assert.Equal("Thr", entity.StringAsCharVarying3);
            Assert.Equal("Let", entity.StringAsCharacterVarying3);
            Assert.Equal("Won", entity.StringAsNchar3);
            Assert.Equal("Squ", entity.StringAsNationalCharacter3);
            Assert.Equal("Int", entity.StringAsNvarchar3);
            Assert.Equal("The", entity.StringAsNationalCharVarying3);
            Assert.Equal("Col", entity.StringAsNationalCharacterVarying3);
            Assert.Equal(new byte[] { 10, 11, 12 }, entity.BytesAsBinary3);
            Assert.Equal(new byte[] { 11, 12, 13 }, entity.BytesAsVarbinary3);
            Assert.Equal(new byte[] { 12, 13, 14 }, entity.BytesAsBinaryVarying3);
            Assert.Equal('A', entity.CharAsVarchar3);
            Assert.Equal('B', entity.CharAsAsCharVarying3);
            Assert.Equal('C', entity.CharAsCharacterVarying3);
            Assert.Equal('D', entity.CharAsNvarchar3);
            Assert.Equal('E', entity.CharAsNationalCharVarying3);
            Assert.Equal('F', entity.CharAsNationalCharacterVarying3);
        }

        private static MappedSizedDataTypesWithIdentity CreateMappedSizedDataTypesWithIdentity(int id)
            => new MappedSizedDataTypesWithIdentity
            {
                Int = id,
                StringAsChar3 = "Wor",
                StringAsCharacter3 = "Lon",
                StringAsVarchar3 = "Tha",
                StringAsCharVarying3 = "Thr",
                StringAsCharacterVarying3 = "Let",
                StringAsNchar3 = "Won",
                StringAsNationalCharacter3 = "Squ",
                StringAsNvarchar3 = "Int",
                StringAsNationalCharVarying3 = "The",
                StringAsNationalCharacterVarying3 = "Col",
                BytesAsBinary3 = new byte[] { 10, 11, 12 },
                BytesAsVarbinary3 = new byte[] { 11, 12, 13 },
                BytesAsBinaryVarying3 = new byte[] { 12, 13, 14 },
                CharAsVarchar3 = 'A',
                CharAsAsCharVarying3 = 'B',
                CharAsCharacterVarying3 = 'C',
                CharAsNvarchar3 = 'D',
                CharAsNationalCharVarying3 = 'E',
                CharAsNationalCharacterVarying3 = 'F'
            };

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='' (Size = 3) (DbType = Binary)
@p1='' (Size = 3) (DbType = Binary)
@p2='' (Size = 3) (DbType = Binary)
@p3=''
@p4=''
@p5='' (DbType = String)
@p6='' (DbType = String)
@p7='' (DbType = String)
@p8=''
@p9='78'
@p10='' (Size = 3)
@p11='' (Size = 3)
@p12='' (Size = 3)
@p13='' (Size = 3)
@p14='' (Size = 3) (DbType = String)
@p15='' (Size = 3) (DbType = String)
@p16='' (Size = 3) (DbType = String)
@p17='' (Size = 3) (DbType = String)
@p18='' (Size = 3) (DbType = String)
@p19='' (Size = 3)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 78), 78);
            }
        }

        private static void AssertNullMappedSizedDataTypesWithIdentity(MappedSizedDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Null(entity.StringAsChar3);
            Assert.Null(entity.StringAsCharacter3);
            Assert.Null(entity.StringAsVarchar3);
            Assert.Null(entity.StringAsCharVarying3);
            Assert.Null(entity.StringAsCharacterVarying3);
            Assert.Null(entity.StringAsNchar3);
            Assert.Null(entity.StringAsNationalCharacter3);
            Assert.Null(entity.StringAsNvarchar3);
            Assert.Null(entity.StringAsNationalCharVarying3);
            Assert.Null(entity.StringAsNationalCharacterVarying3);
            Assert.Null(entity.BytesAsBinary3);
            Assert.Null(entity.BytesAsVarbinary3);
            Assert.Null(entity.BytesAsBinaryVarying3);
            Assert.Null(entity.CharAsVarchar3);
            Assert.Null(entity.CharAsAsCharVarying3);
            Assert.Null(entity.CharAsCharacterVarying3);
            Assert.Null(entity.CharAsNvarchar3);
            Assert.Null(entity.CharAsNationalCharVarying3);
            Assert.Null(entity.CharAsNationalCharacterVarying3);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='2017-01-02T12:11:12'
@p1='2016-01-02T11:11:12.0000000+00:00'
@p2='102.2'
@p3='101.1'
@p4='103.3'
@p5='85.5'
@p6='83.3'
@p7='77'",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedScaledDataTypesWithIdentity(MappedScaledDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(83.3f, entity.FloatAsFloat3);
            Assert.Equal(85.5f, entity.FloatAsDoublePrecision3);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.DateTimeOffsetAsDatetimeoffset3);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.DateTimeAsDatetime23);
            Assert.Equal(101m, entity.DecimalAsDecimal3);
            Assert.Equal(102m, entity.DecimalAsDec3);
            Assert.Equal(103m, entity.DecimalAsNumeric3);
        }

        private static MappedScaledDataTypesWithIdentity CreateMappedScaledDataTypesWithIdentity(int id)
            => new MappedScaledDataTypesWithIdentity
            {
                Int = id,
                FloatAsFloat3 = 83.3f,
                FloatAsDoublePrecision3 = 85.5f,
                DateTimeOffsetAsDatetimeoffset3 = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                DateTimeAsDatetime23 = new DateTime(2017, 1, 2, 12, 11, 12),
                DecimalAsDecimal3 = 101.1m,
                DecimalAsDec3 = 102.2m,
                DecimalAsNumeric3 = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(
                    CreateMappedPrecisionAndScaledDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='102.2'
@p1='101.1'
@p2='103.3'
@p3='77'",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedPrecisionAndScaledDataTypesWithIdentity(
                    context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedPrecisionAndScaledDataTypesWithIdentity(MappedPrecisionAndScaledDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(101.1m, entity.DecimalAsDecimal52);
            Assert.Equal(102.2m, entity.DecimalAsDec52);
            Assert.Equal(103.3m, entity.DecimalAsNumeric52);
        }

        private static MappedPrecisionAndScaledDataTypesWithIdentity CreateMappedPrecisionAndScaledDataTypesWithIdentity(int id)
            => new MappedPrecisionAndScaledDataTypesWithIdentity
            {
                Int = id,
                DecimalAsDecimal52 = 101.1m,
                DecimalAsDec52 = 102.2m,
                DecimalAsNumeric52 = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(177));
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(178));
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 177), 177);
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 178), 178);
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(177));
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(178));
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 177), 177);
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 178), 178);
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 278 });
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 279 });
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 280 });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 278), 278);
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 279), 279);
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 280), 280);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(177));
                context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(178));
                context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 177), 177);
                AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 178), 178);
                AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 278 });
                context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 279 });
                context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 280 });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 278), 278);
                AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 279), 279);
                AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 280), 280);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(177));
                context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(178));
                context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 177), 177);
                AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 178), 178);
                AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(177));
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(178));
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedPrecisionAndScaledDataTypes(context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 177), 177);
                AssertMappedPrecisionAndScaledDataTypes(context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 178), 178);
                AssertMappedPrecisionAndScaledDataTypes(context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(177));
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(178));
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
                AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
                AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(177));
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(178));
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
                AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
                AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 278 });
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 279 });
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 280 });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 278), 278);
                AssertNullMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 279), 279);
                AssertNullMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 280), 280);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(177));
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(178));
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
                AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
                AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 278 });
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 279 });
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 280 });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 278), 278);
                AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 279), 279);
                AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 280), 280);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(177));
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(178));
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(CreateMappedPrecisionAndScaledDataTypesWithIdentity(177));
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(CreateMappedPrecisionAndScaledDataTypesWithIdentity(178));
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(CreateMappedPrecisionAndScaledDataTypesWithIdentity(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedPrecisionAndScaledDataTypesWithIdentity(
                    context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
                AssertMappedPrecisionAndScaledDataTypesWithIdentity(
                    context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
                AssertMappedPrecisionAndScaledDataTypesWithIdentity(
                    context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
            }
        }

        [ConditionalFact]
        public virtual void Columns_have_expected_data_types()
        {
            const string query
                = @"SELECT
                        TABLE_NAME,
                        COLUMN_NAME,
                        DATA_TYPE,
                        IS_NULLABLE,
                        CHARACTER_MAXIMUM_LENGTH,
                        NUMERIC_PRECISION,
                        NUMERIC_SCALE,
                        DATETIME_PRECISION
                    FROM INFORMATION_SCHEMA.COLUMNS";

            var columns = new List<ColumnInfo>();

            using (var context = CreateContext())
            {
                var connection = context.Database.GetDbConnection();

                var command = connection.CreateCommand();
                command.CommandText = query;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var columnInfo = new ColumnInfo
                        {
                            TableName = reader.GetString(0),
                            ColumnName = reader.GetString(1),
                            DataType = reader.GetString(2),
                            IsNullable = reader.IsDBNull(3) ? null : (bool?)(reader.GetString(3) == "YES"),
                            MaxLength = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4),
                            NumericPrecision = reader.IsDBNull(5) ? null : (int?)reader.GetByte(5),
                            NumericScale = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            DateTimePrecision = reader.IsDBNull(7) ? null : (int?)reader.GetInt16(7)
                        };

                        columns.Add(columnInfo);
                    }
                }
            }

            var builder = new StringBuilder();

            foreach (var column in columns.OrderBy(e => e.TableName).ThenBy(e => e.ColumnName))
            {
                builder.Append(column.TableName);
                builder.Append(".");
                builder.Append(column.ColumnName);
                builder.Append(" ---> [");

                if (column.IsNullable == true)
                {
                    builder.Append("nullable ");
                }

                builder.Append(column.DataType);
                builder.Append("]");

                if (column.MaxLength.HasValue)
                {
                    builder.Append(" [MaxLength = ");
                    builder.Append(column.MaxLength);
                    builder.Append("]");
                }

                if (column.NumericPrecision.HasValue)
                {
                    builder.Append(" [Precision = ");
                    builder.Append(column.NumericPrecision);
                }

                if (column.DateTimePrecision.HasValue)
                {
                    builder.Append(" [Precision = ");
                    builder.Append(column.DateTimePrecision);
                }

                if (column.NumericScale.HasValue)
                {
                    builder.Append(" Scale = ");
                    builder.Append(column.NumericScale);
                }

                if (column.NumericPrecision.HasValue
                    || column.DateTimePrecision.HasValue
                    || column.NumericScale.HasValue)
                {
                    builder.Append("]");
                }

                builder.AppendLine();
            }

            var actual = builder.ToString();

            const string expected = @"BinaryForeignKeyDataType.BinaryKeyDataTypeId ---> [nullable varbinary] [MaxLength = 900]
BinaryForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
BinaryKeyDataType.Id ---> [varbinary] [MaxLength = 900]
BuiltInDataTypes.Enum16 ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypes.Enum32 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.Enum64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum8 ---> [tinyint] [Precision = 3 Scale = 0]
BuiltInDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.PartitionId ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestBoolean ---> [bit]
BuiltInDataTypes.TestByte ---> [tinyint] [Precision = 3 Scale = 0]
BuiltInDataTypes.TestCharacter ---> [nchar] [MaxLength = 1]
BuiltInDataTypes.TestDateTime ---> [datetime2] [Precision = 7]
BuiltInDataTypes.TestDateTimeOffset ---> [datetimeoffset] [Precision = 7]
BuiltInDataTypes.TestDecimal ---> [decimal] [Precision = 18 Scale = 2]
BuiltInDataTypes.TestDouble ---> [float] [Precision = 53]
BuiltInDataTypes.TestInt16 ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypes.TestInt32 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestSignedByte ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypes.TestSingle ---> [real] [Precision = 24]
BuiltInDataTypes.TestTimeSpan ---> [time] [Precision = 7]
BuiltInDataTypes.TestUnsignedInt16 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestUnsignedInt32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestUnsignedInt64 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.Enum16 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypes.Enum32 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.Enum64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum8 ---> [nullable tinyint] [Precision = 3 Scale = 0]
BuiltInNullableDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.PartitionId ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestByteArray ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableBoolean ---> [nullable bit]
BuiltInNullableDataTypes.TestNullableByte ---> [nullable tinyint] [Precision = 3 Scale = 0]
BuiltInNullableDataTypes.TestNullableCharacter ---> [nullable nchar] [MaxLength = 1]
BuiltInNullableDataTypes.TestNullableDateTime ---> [nullable datetime2] [Precision = 7]
BuiltInNullableDataTypes.TestNullableDateTimeOffset ---> [nullable datetimeoffset] [Precision = 7]
BuiltInNullableDataTypes.TestNullableDecimal ---> [nullable decimal] [Precision = 18 Scale = 2]
BuiltInNullableDataTypes.TestNullableDouble ---> [nullable float] [Precision = 53]
BuiltInNullableDataTypes.TestNullableInt16 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt32 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableSignedByte ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypes.TestNullableSingle ---> [nullable real] [Precision = 24]
BuiltInNullableDataTypes.TestNullableTimeSpan ---> [nullable time] [Precision = 7]
BuiltInNullableDataTypes.TestNullableUnsignedInt16 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt64 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.TestString ---> [nullable nvarchar] [MaxLength = -1]
MappedDataTypes.BoolAsBit ---> [bit]
MappedDataTypes.ByteAsTinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedDataTypes.BytesAsBinaryVaryingMax ---> [varbinary] [MaxLength = -1]
MappedDataTypes.BytesAsImage ---> [image] [MaxLength = 2147483647]
MappedDataTypes.BytesAsVarbinaryMax ---> [varbinary] [MaxLength = -1]
MappedDataTypes.CharAsAsCharVaryingMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.CharAsCharacterVaryingMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.CharAsInt ---> [int] [Precision = 10 Scale = 0]
MappedDataTypes.CharAsNationalCharacterVaryingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.CharAsNationalCharVaryingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.CharAsNtext ---> [ntext] [MaxLength = 1073741823]
MappedDataTypes.CharAsNvarcharMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.CharAsText ---> [text] [MaxLength = 2147483647]
MappedDataTypes.CharAsVarcharMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.DateTimeAsDate ---> [date] [Precision = 0]
MappedDataTypes.DateTimeAsDatetime ---> [datetime] [Precision = 3]
MappedDataTypes.DateTimeAsDatetime2 ---> [datetime2] [Precision = 7]
MappedDataTypes.DateTimeAsSmalldatetime ---> [smalldatetime] [Precision = 0]
MappedDataTypes.DateTimeOffsetAsDatetimeoffset ---> [datetimeoffset] [Precision = 7]
MappedDataTypes.Decimal ---> [decimal] [Precision = 18 Scale = 0]
MappedDataTypes.DecimalAsDec ---> [decimal] [Precision = 18 Scale = 0]
MappedDataTypes.DecimalAsMoney ---> [money] [Precision = 19 Scale = 4]
MappedDataTypes.DecimalAsNumeric ---> [numeric] [Precision = 18 Scale = 0]
MappedDataTypes.DecimalAsSmallmoney ---> [smallmoney] [Precision = 10 Scale = 4]
MappedDataTypes.DoubleAsDoublePrecision ---> [float] [Precision = 53]
MappedDataTypes.DoubleAsFloat ---> [float] [Precision = 53]
MappedDataTypes.FloatAsReal ---> [real] [Precision = 24]
MappedDataTypes.GuidAsUniqueidentifier ---> [uniqueidentifier]
MappedDataTypes.Int ---> [int] [Precision = 10 Scale = 0]
MappedDataTypes.LongAsBigInt ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypes.SByteAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypes.SByteAsTinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedDataTypes.ShortAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypes.StringAsAsCharVaryingMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.StringAsCharacterVaryingMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.StringAsNationalCharacterVaryingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.StringAsNationalCharVaryingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.StringAsNtext ---> [ntext] [MaxLength = 1073741823]
MappedDataTypes.StringAsNvarcharMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.StringAsText ---> [text] [MaxLength = 2147483647]
MappedDataTypes.StringAsVarcharMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.TimeSpanAsTime ---> [time] [Precision = 7]
MappedDataTypes.UintAsBigint ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypes.UintAsInt ---> [int] [Precision = 10 Scale = 0]
MappedDataTypes.UlongAsBigint ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypes.UlongAsDecimal200 ---> [decimal] [Precision = 20 Scale = 0]
MappedDataTypes.UShortAsInt ---> [int] [Precision = 10 Scale = 0]
MappedDataTypes.UShortAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypesWithIdentity.BoolAsBit ---> [bit]
MappedDataTypesWithIdentity.ByteAsTinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedDataTypesWithIdentity.BytesAsBinaryVaryingMax ---> [nullable varbinary] [MaxLength = -1]
MappedDataTypesWithIdentity.BytesAsImage ---> [nullable image] [MaxLength = 2147483647]
MappedDataTypesWithIdentity.BytesAsVarbinaryMax ---> [nullable varbinary] [MaxLength = -1]
MappedDataTypesWithIdentity.CharAsAsCharVaryingMax ---> [varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.CharAsCharacterVaryingMax ---> [varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.CharAsInt ---> [int] [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.CharAsNationalCharacterVaryingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.CharAsNationalCharVaryingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.CharAsNtext ---> [ntext] [MaxLength = 1073741823]
MappedDataTypesWithIdentity.CharAsNvarcharMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.CharAsText ---> [text] [MaxLength = 2147483647]
MappedDataTypesWithIdentity.CharAsVarcharMax ---> [varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.DateTimeAsDate ---> [date] [Precision = 0]
MappedDataTypesWithIdentity.DateTimeAsDatetime ---> [datetime] [Precision = 3]
MappedDataTypesWithIdentity.DateTimeAsDatetime2 ---> [datetime2] [Precision = 7]
MappedDataTypesWithIdentity.DateTimeAsSmalldatetime ---> [smalldatetime] [Precision = 0]
MappedDataTypesWithIdentity.DateTimeOffsetAsDatetimeoffset ---> [datetimeoffset] [Precision = 7]
MappedDataTypesWithIdentity.Decimal ---> [decimal] [Precision = 18 Scale = 0]
MappedDataTypesWithIdentity.DecimalAsDec ---> [decimal] [Precision = 18 Scale = 0]
MappedDataTypesWithIdentity.DecimalAsMoney ---> [money] [Precision = 19 Scale = 4]
MappedDataTypesWithIdentity.DecimalAsNumeric ---> [numeric] [Precision = 18 Scale = 0]
MappedDataTypesWithIdentity.DecimalAsSmallmoney ---> [smallmoney] [Precision = 10 Scale = 4]
MappedDataTypesWithIdentity.DoubleAsDoublePrecision ---> [float] [Precision = 53]
MappedDataTypesWithIdentity.DoubleAsFloat ---> [float] [Precision = 53]
MappedDataTypesWithIdentity.FloatAsReal ---> [real] [Precision = 24]
MappedDataTypesWithIdentity.GuidAsUniqueidentifier ---> [uniqueidentifier]
MappedDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.Int ---> [int] [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.LongAsBigint ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypesWithIdentity.SByteAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypesWithIdentity.SbyteAsTinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedDataTypesWithIdentity.ShortAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypesWithIdentity.StringAsCharacterVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsCharVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsNationalCharacterVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsNationalCharVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsNtext ---> [nullable ntext] [MaxLength = 1073741823]
MappedDataTypesWithIdentity.StringAsNvarcharMax ---> [nullable nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.StringAsText ---> [nullable text] [MaxLength = 2147483647]
MappedDataTypesWithIdentity.StringAsVarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.TimeSpanAsTime ---> [time] [Precision = 7]
MappedDataTypesWithIdentity.UintAsBigint ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypesWithIdentity.UintAsInt ---> [int] [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.UlongAsBigint ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypesWithIdentity.UlongAsDecimal200 ---> [decimal] [Precision = 20 Scale = 0]
MappedDataTypesWithIdentity.UShortAsInt ---> [int] [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.UShortAsSmallint ---> [smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypes.BoolAsBit ---> [nullable bit]
MappedNullableDataTypes.ByteAsTinyint ---> [nullable tinyint] [Precision = 3 Scale = 0]
MappedNullableDataTypes.BytesAsBinaryVaryingMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypes.BytesAsImage ---> [nullable image] [MaxLength = 2147483647]
MappedNullableDataTypes.BytesAsVarbinaryMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypes.CharAsAsCharVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.CharAsCharacterVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.CharAsInt ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypes.CharAsNationalCharacterVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.CharAsNationalCharVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.CharAsNtext ---> [nullable ntext] [MaxLength = 1073741823]
MappedNullableDataTypes.CharAsNvarcharMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.CharAsText ---> [nullable text] [MaxLength = 2147483647]
MappedNullableDataTypes.CharAsVarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.DateTimeAsDate ---> [nullable date] [Precision = 0]
MappedNullableDataTypes.DateTimeAsDatetime ---> [nullable datetime] [Precision = 3]
MappedNullableDataTypes.DateTimeAsDatetime2 ---> [nullable datetime2] [Precision = 7]
MappedNullableDataTypes.DateTimeAsSmalldatetime ---> [nullable smalldatetime] [Precision = 0]
MappedNullableDataTypes.DateTimeOffsetAsDatetimeoffset ---> [nullable datetimeoffset] [Precision = 7]
MappedNullableDataTypes.Decimal ---> [nullable decimal] [Precision = 18 Scale = 0]
MappedNullableDataTypes.DecimalAsDec ---> [nullable decimal] [Precision = 18 Scale = 0]
MappedNullableDataTypes.DecimalAsMoney ---> [nullable money] [Precision = 19 Scale = 4]
MappedNullableDataTypes.DecimalAsNumeric ---> [nullable numeric] [Precision = 18 Scale = 0]
MappedNullableDataTypes.DecimalAsSmallmoney ---> [nullable smallmoney] [Precision = 10 Scale = 4]
MappedNullableDataTypes.DoubleAsDoublePrecision ---> [nullable float] [Precision = 53]
MappedNullableDataTypes.DoubleAsFloat ---> [nullable float] [Precision = 53]
MappedNullableDataTypes.FloatAsReal ---> [nullable real] [Precision = 24]
MappedNullableDataTypes.GuidAsUniqueidentifier ---> [nullable uniqueidentifier]
MappedNullableDataTypes.Int ---> [int] [Precision = 10 Scale = 0]
MappedNullableDataTypes.LongAsBigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypes.SByteAsSmallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypes.SbyteAsTinyint ---> [nullable tinyint] [Precision = 3 Scale = 0]
MappedNullableDataTypes.ShortAsSmallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypes.StringAsCharacterVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsCharVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsNationalCharacterVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsNationalCharVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsNtext ---> [nullable ntext] [MaxLength = 1073741823]
MappedNullableDataTypes.StringAsNvarcharMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.StringAsText ---> [nullable text] [MaxLength = 2147483647]
MappedNullableDataTypes.StringAsVarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.TimeSpanAsTime ---> [nullable time] [Precision = 7]
MappedNullableDataTypes.UintAsBigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypes.UintAsInt ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypes.UlongAsBigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypes.UlongAsDecimal200 ---> [nullable decimal] [Precision = 20 Scale = 0]
MappedNullableDataTypes.UShortAsInt ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypes.UShortAsSmallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypesWithIdentity.BoolAsBit ---> [nullable bit]
MappedNullableDataTypesWithIdentity.ByteAsTinyint ---> [nullable tinyint] [Precision = 3 Scale = 0]
MappedNullableDataTypesWithIdentity.BytesAsImage ---> [nullable image] [MaxLength = 2147483647]
MappedNullableDataTypesWithIdentity.BytesAsVarbinaryMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.BytesAsVaryingMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.CharAsAsCharVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.CharAsCharacterVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.CharAsInt ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.CharAsNationalCharacterVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.CharAsNationalCharVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.CharAsNtext ---> [nullable ntext] [MaxLength = 1073741823]
MappedNullableDataTypesWithIdentity.CharAsNvarcharMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.CharAsText ---> [nullable text] [MaxLength = 2147483647]
MappedNullableDataTypesWithIdentity.CharAsVarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.DateTimeAsDate ---> [nullable date] [Precision = 0]
MappedNullableDataTypesWithIdentity.DateTimeAsDatetime ---> [nullable datetime] [Precision = 3]
MappedNullableDataTypesWithIdentity.DateTimeAsDatetime2 ---> [nullable datetime2] [Precision = 7]
MappedNullableDataTypesWithIdentity.DateTimeAsSmalldatetime ---> [nullable smalldatetime] [Precision = 0]
MappedNullableDataTypesWithIdentity.DateTimeOffsetAsDatetimeoffset ---> [nullable datetimeoffset] [Precision = 7]
MappedNullableDataTypesWithIdentity.Decimal ---> [nullable decimal] [Precision = 18 Scale = 0]
MappedNullableDataTypesWithIdentity.DecimalAsDec ---> [nullable decimal] [Precision = 18 Scale = 0]
MappedNullableDataTypesWithIdentity.DecimalAsMoney ---> [nullable money] [Precision = 19 Scale = 4]
MappedNullableDataTypesWithIdentity.DecimalAsNumeric ---> [nullable numeric] [Precision = 18 Scale = 0]
MappedNullableDataTypesWithIdentity.DecimalAsSmallmoney ---> [nullable smallmoney] [Precision = 10 Scale = 4]
MappedNullableDataTypesWithIdentity.DoubkleAsDoublePrecision ---> [nullable float] [Precision = 53]
MappedNullableDataTypesWithIdentity.DoubleAsFloat ---> [nullable float] [Precision = 53]
MappedNullableDataTypesWithIdentity.FloatAsReal ---> [nullable real] [Precision = 24]
MappedNullableDataTypesWithIdentity.GuidAsUniqueidentifier ---> [nullable uniqueidentifier]
MappedNullableDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.Int ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.LongAsBigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypesWithIdentity.SByteAsSmallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypesWithIdentity.SbyteAsTinyint ---> [nullable tinyint] [Precision = 3 Scale = 0]
MappedNullableDataTypesWithIdentity.ShortAsSmallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypesWithIdentity.StringAsCharacterVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsCharVaryingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsNationalCharacterVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsNationalCharVaryingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsNtext ---> [nullable ntext] [MaxLength = 1073741823]
MappedNullableDataTypesWithIdentity.StringAsNvarcharMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.StringAsText ---> [nullable text] [MaxLength = 2147483647]
MappedNullableDataTypesWithIdentity.StringAsVarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.TimeSpanAsTime ---> [nullable time] [Precision = 7]
MappedNullableDataTypesWithIdentity.UintAsBigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypesWithIdentity.UintAsInt ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.UlongAsBigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypesWithIdentity.UlongAsDecimal200 ---> [nullable decimal] [Precision = 20 Scale = 0]
MappedNullableDataTypesWithIdentity.UShortAsInt ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.UshortAsSmallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedPrecisionAndScaledDataTypes.DecimalAsDec52 ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypes.DecimalAsDecimal52 ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypes.DecimalAsNumeric52 ---> [numeric] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedPrecisionAndScaledDataTypesWithIdentity.DecimalAsDec52 ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypesWithIdentity.DecimalAsDecimal52 ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypesWithIdentity.DecimalAsNumeric52 ---> [numeric] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedPrecisionAndScaledDataTypesWithIdentity.Int ---> [int] [Precision = 10 Scale = 0]
MappedScaledDataTypes.DateTimeAsDatetime23 ---> [datetime2] [Precision = 3]
MappedScaledDataTypes.DateTimeOffsetAsDatetimeoffset3 ---> [datetimeoffset] [Precision = 3]
MappedScaledDataTypes.DecimalAsDec3 ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypes.DecimalAsDecimal3 ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypes.DecimalAsNumeric3 ---> [numeric] [Precision = 3 Scale = 0]
MappedScaledDataTypes.FloatAsDoublePrecision3 ---> [real] [Precision = 24]
MappedScaledDataTypes.FloatAsFloat3 ---> [real] [Precision = 24]
MappedScaledDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedScaledDataTypesWithIdentity.DateTimeAsDatetime23 ---> [datetime2] [Precision = 3]
MappedScaledDataTypesWithIdentity.DateTimeOffsetAsDatetimeoffset3 ---> [datetimeoffset] [Precision = 3]
MappedScaledDataTypesWithIdentity.DecimalAsDec3 ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypesWithIdentity.DecimalAsDecimal3 ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypesWithIdentity.DecimalAsNumeric3 ---> [numeric] [Precision = 3 Scale = 0]
MappedScaledDataTypesWithIdentity.FloatAsDoublePrecision3 ---> [real] [Precision = 24]
MappedScaledDataTypesWithIdentity.FloatAsFloat3 ---> [real] [Precision = 24]
MappedScaledDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedScaledDataTypesWithIdentity.Int ---> [int] [Precision = 10 Scale = 0]
MappedSizedDataTypes.BytesAsBinary3 ---> [nullable binary] [MaxLength = 3]
MappedSizedDataTypes.BytesAsBinaryVarying3 ---> [nullable varbinary] [MaxLength = 3]
MappedSizedDataTypes.BytesAsVarbinary3 ---> [nullable varbinary] [MaxLength = 3]
MappedSizedDataTypes.CharAsAsCharVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.CharAsCharacterVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.CharAsNationalCharacterVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.CharAsNationalCharVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.CharAsNvarchar3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.CharAsVarchar3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedSizedDataTypes.StringAsChar3 ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypes.StringAsCharacter3 ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypes.StringAsCharacterVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsCharVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsNationalCharacter3 ---> [nullable nchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsNationalCharacterVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsNationalCharVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsNchar3 ---> [nullable nchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsNvarchar3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.StringAsVarchar3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.BytesAsBinary3 ---> [nullable binary] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.BytesAsBinaryVarying3 ---> [nullable varbinary] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.BytesAsVarbinary3 ---> [nullable varbinary] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.CharAsAsCharVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.CharAsCharacterVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.CharAsNationalCharacterVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.CharAsNationalCharVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.CharAsNvarchar3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.CharAsVarchar3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedSizedDataTypesWithIdentity.Int ---> [int] [Precision = 10 Scale = 0]
MappedSizedDataTypesWithIdentity.StringAsChar3 ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsCharacter3 ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsCharacterVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsCharVarying3 ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsNationalCharacter3 ---> [nullable nchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsNationalCharacterVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsNationalCharVarying3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsNchar3 ---> [nullable nchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsNvarchar3 ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.StringAsVarchar3 ---> [nullable varchar] [MaxLength = 3]
MaxLengthDataTypes.ByteArray5 ---> [nullable varbinary] [MaxLength = 5]
MaxLengthDataTypes.ByteArray9000 ---> [nullable varbinary] [MaxLength = -1]
MaxLengthDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MaxLengthDataTypes.String3 ---> [nullable nvarchar] [MaxLength = 3]
MaxLengthDataTypes.String9000 ---> [nullable nvarchar] [MaxLength = -1]
StringForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
StringForeignKeyDataType.StringKeyDataTypeId ---> [nullable nvarchar] [MaxLength = 450]
StringKeyDataType.Id ---> [nvarchar] [MaxLength = 450]
UnicodeDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
UnicodeDataTypes.StringAnsi ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringAnsi3 ---> [nullable varchar] [MaxLength = 3]
UnicodeDataTypes.StringAnsi9000 ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringDefault ---> [nullable nvarchar] [MaxLength = -1]
UnicodeDataTypes.StringUnicode ---> [nullable nvarchar] [MaxLength = -1]
";

            Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
        }

        private static readonly string EOL = Environment.NewLine;

        [Fact]
        public void Can_get_column_types_from_built_model()
        {
            using (var context = CreateContext())
            {
                var typeMapper = context.GetService<IRelationalTypeMapper>();

                foreach (var property in context.Model.GetEntityTypes().SelectMany(e => e.GetDeclaredProperties()))
                {
                    var columnType = property.Relational().ColumnType;
                    Assert.NotNull(columnType);

                    if (property[RelationalAnnotationNames.ColumnType] == null)
                    {
                        Assert.Equal(
                            columnType.ToLowerInvariant(),
                            typeMapper.FindMapping(property).StoreType.ToLowerInvariant());
                    }
                }
            }
        }

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;

        public class BuiltInDataTypesSqlServerFixture : BuiltInDataTypesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                MakeRequired<MappedDataTypes>(modelBuilder);

                modelBuilder.Entity<BuiltInDataTypes>()
                    .Property(dt => dt.TestDecimal)
                    .HasColumnType("decimal(18,2)");

                modelBuilder.Entity<MappedDataTypes>(
                    b =>
                        {
                            b.HasKey(e => e.Int);
                            b.Property(e => e.Int).ValueGeneratedNever();
                        });

                modelBuilder.Entity<MappedNullableDataTypes>(
                    b =>
                        {
                            b.HasKey(e => e.Int);
                            b.Property(e => e.Int).ValueGeneratedNever();
                        });

                modelBuilder.Entity<MappedSizedDataTypes>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();

                modelBuilder.Entity<MappedScaledDataTypes>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();

                modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();

                modelBuilder.Entity<MappedDataTypes>();
                modelBuilder.Entity<MappedNullableDataTypes>();
                modelBuilder.Entity<MappedSizedDataTypes>();
                modelBuilder.Entity<MappedScaledDataTypes>();
                modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>();
                modelBuilder.Entity<MappedDataTypesWithIdentity>();
                modelBuilder.Entity<MappedNullableDataTypesWithIdentity>();
                modelBuilder.Entity<MappedSizedDataTypesWithIdentity>();
                modelBuilder.Entity<MappedScaledDataTypesWithIdentity>();
                modelBuilder.Entity<MappedPrecisionAndScaledDataTypesWithIdentity>();
                modelBuilder.Entity<MappedDataTypesWithIdentity>();
                modelBuilder.Entity<MappedNullableDataTypesWithIdentity>();
                modelBuilder.Entity<MappedSizedDataTypesWithIdentity>();
                modelBuilder.Entity<MappedScaledDataTypesWithIdentity>();
                modelBuilder.Entity<MappedPrecisionAndScaledDataTypesWithIdentity>();
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(
                    c => c
                        .Log(RelationalEventId.QueryClientEvaluationWarning));

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();
        }

        protected class MappedDataTypes
        {
            [Column(TypeName = "int")]
            public int Int { get; set; }

            [Column(TypeName = "bigint")]
            public long LongAsBigInt { get; set; }

            [Column(TypeName = "smallint")]
            public short ShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public byte ByteAsTinyint { get; set; }

            [Column(TypeName = "int")]
            public uint UintAsInt { get; set; }

            [Column(TypeName = "bigint")]
            public ulong UlongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public ushort UShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public sbyte SByteAsTinyint { get; set; }

            [Column(TypeName = "bit")]
            public bool BoolAsBit { get; set; }

            [Column(TypeName = "money")]
            public decimal DecimalAsMoney { get; set; }

            [Column(TypeName = "smallmoney")]
            public decimal DecimalAsSmallmoney { get; set; }

            [Column(TypeName = "float")]
            public double DoubleAsFloat { get; set; }

            [Column(TypeName = "real")]
            public float FloatAsReal { get; set; }

            [Column(TypeName = "double precision")]
            public double DoubleAsDoublePrecision { get; set; }

            [Column(TypeName = "date")]
            public DateTime DateTimeAsDate { get; set; }

            [Column(TypeName = "datetimeoffset")]
            public DateTimeOffset DateTimeOffsetAsDatetimeoffset { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime DateTimeAsDatetime2 { get; set; }

            [Column(TypeName = "smalldatetime")]
            public DateTime DateTimeAsSmalldatetime { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime DateTimeAsDatetime { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan TimeSpanAsTime { get; set; }

            [Column(TypeName = "varchar(max)")]
            public string StringAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public string StringAsAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public string StringAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public string StringAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public string StringAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public string StringAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public string StringAsText { get; set; }

            [Column(TypeName = "ntext")]
            public string StringAsNtext { get; set; }

            [Column(TypeName = "varbinary(max)")]
            public byte[] BytesAsVarbinaryMax { get; set; }

            [Column(TypeName = "binary varying(max)")]
            public byte[] BytesAsBinaryVaryingMax { get; set; }

            [Column(TypeName = "image")]
            public byte[] BytesAsImage { get; set; }

            [Column(TypeName = "decimal")]
            public decimal Decimal { get; set; }

            [Column(TypeName = "dec")]
            public decimal DecimalAsDec { get; set; }

            [Column(TypeName = "numeric")]
            public decimal DecimalAsNumeric { get; set; }

            [Column(TypeName = "uniqueidentifier")]
            public Guid GuidAsUniqueidentifier { get; set; }

            [Column(TypeName = "bigint")]
            public uint UintAsBigint { get; set; }

            [Column(TypeName = "decimal(20,0)")]
            public ulong UlongAsDecimal200 { get; set; }

            [Column(TypeName = "int")]
            public ushort UShortAsInt { get; set; }

            [Column(TypeName = "smallint")]
            public sbyte SByteAsSmallint { get; set; }

            [Column(TypeName = "varchar(max)")]
            public char CharAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public char CharAsAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public char CharAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public char CharAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public char CharAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public char CharAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public char CharAsText { get; set; }

            [Column(TypeName = "ntext")]
            public char CharAsNtext { get; set; }

            [Column(TypeName = "int")]
            public char CharAsInt { get; set; }
        }

        protected class MappedSizedDataTypes
        {
            public int Id { get; set; }

            [Column(TypeName = "char(3)")]
            public string StringAsChar3 { get; set; }

            [Column(TypeName = "character(3)")]
            public string StringAsCharacter3 { get; set; }

            [Column(TypeName = "varchar(3)")]
            public string StringAsVarchar3 { get; set; }

            [Column(TypeName = "char varying(3)")]
            public string StringAsCharVarying3 { get; set; }

            [Column(TypeName = "character varying(3)")]
            public string StringAsCharacterVarying3 { get; set; }

            [Column(TypeName = "nchar(3)")]
            public string StringAsNchar3 { get; set; }

            [Column(TypeName = "national character(3)")]
            public string StringAsNationalCharacter3 { get; set; }

            [Column(TypeName = "nvarchar(3)")]
            public string StringAsNvarchar3 { get; set; }

            [Column(TypeName = "national char varying(3)")]
            public string StringAsNationalCharVarying3 { get; set; }

            [Column(TypeName = "national character varying(3)")]
            public string StringAsNationalCharacterVarying3 { get; set; }

            [Column(TypeName = "binary(3)")]
            public byte[] BytesAsBinary3 { get; set; }

            [Column(TypeName = "varbinary(3)")]
            public byte[] BytesAsVarbinary3 { get; set; }

            [Column(TypeName = "binary varying(3)")]
            public byte[] BytesAsBinaryVarying3 { get; set; }

            [Column(TypeName = "varchar(3)")]
            public char? CharAsVarchar3 { get; set; }

            [Column(TypeName = "char varying(3)")]
            public char? CharAsAsCharVarying3 { get; set; }

            [Column(TypeName = "character varying(3)")]
            public char? CharAsCharacterVarying3 { get; set; }

            [Column(TypeName = "nvarchar(3)")]
            public char? CharAsNvarchar3 { get; set; }

            [Column(TypeName = "national char varying(3)")]
            public char? CharAsNationalCharVarying3 { get; set; }

            [Column(TypeName = "national character varying(3)")]
            public char? CharAsNationalCharacterVarying3 { get; set; }
        }

        protected class MappedScaledDataTypes
        {
            public int Id { get; set; }

            [Column(TypeName = "float(3)")]
            public float FloatAsFloat3 { get; set; }

            [Column(TypeName = "double precision(3)")]
            public float FloatAsDoublePrecision3 { get; set; }

            [Column(TypeName = "datetimeoffset(3)")]
            public DateTimeOffset DateTimeOffsetAsDatetimeoffset3 { get; set; }

            [Column(TypeName = "datetime2(3)")]
            public DateTime DateTimeAsDatetime23 { get; set; }

            [Column(TypeName = "decimal(3)")]
            public decimal DecimalAsDecimal3 { get; set; }

            [Column(TypeName = "dec(3)")]
            public decimal DecimalAsDec3 { get; set; }

            [Column(TypeName = "numeric(3)")]
            public decimal DecimalAsNumeric3 { get; set; }
        }

        protected class MappedPrecisionAndScaledDataTypes
        {
            public int Id { get; set; }

            [Column(TypeName = "decimal(5,2)")]
            public decimal DecimalAsDecimal52 { get; set; }

            [Column(TypeName = "dec(5,2)")]
            public decimal DecimalAsDec52 { get; set; }

            [Column(TypeName = "numeric(5,2)")]
            public decimal DecimalAsNumeric52 { get; set; }
        }

        protected class MappedNullableDataTypes
        {
            [Column(TypeName = "int")]
            public int? Int { get; set; }

            [Column(TypeName = "bigint")]
            public long? LongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public short? ShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public byte? ByteAsTinyint { get; set; }

            [Column(TypeName = "int")]
            public uint? UintAsInt { get; set; }

            [Column(TypeName = "bigint")]
            public ulong? UlongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public ushort? UShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public sbyte? SbyteAsTinyint { get; set; }

            [Column(TypeName = "bit")]
            public bool? BoolAsBit { get; set; }

            [Column(TypeName = "money")]
            public decimal? DecimalAsMoney { get; set; }

            [Column(TypeName = "smallmoney")]
            public decimal? DecimalAsSmallmoney { get; set; }

            [Column(TypeName = "float")]
            public double? DoubleAsFloat { get; set; }

            [Column(TypeName = "real")]
            public float? FloatAsReal { get; set; }

            [Column(TypeName = "double precision")]
            public double? DoubleAsDoublePrecision { get; set; }

            [Column(TypeName = "date")]
            public DateTime? DateTimeAsDate { get; set; }

            [Column(TypeName = "datetimeoffset")]
            public DateTimeOffset? DateTimeOffsetAsDatetimeoffset { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime? DateTimeAsDatetime2 { get; set; }

            [Column(TypeName = "smalldatetime")]
            public DateTime? DateTimeAsSmalldatetime { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime? DateTimeAsDatetime { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan? TimeSpanAsTime { get; set; }

            [Column(TypeName = "varchar(max)")]
            public string StringAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public string StringAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public string StringAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public string StringAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public string StringAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public string StringAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public string StringAsText { get; set; }

            [Column(TypeName = "ntext")]
            public string StringAsNtext { get; set; }

            [Column(TypeName = "varbinary(max)")]
            public byte[] BytesAsVarbinaryMax { get; set; }

            [Column(TypeName = "binary varying(max)")]
            public byte[] BytesAsBinaryVaryingMax { get; set; }

            [Column(TypeName = "image")]
            public byte[] BytesAsImage { get; set; }

            [Column(TypeName = "decimal")]
            public decimal? Decimal { get; set; }

            [Column(TypeName = "dec")]
            public decimal? DecimalAsDec { get; set; }

            [Column(TypeName = "numeric")]
            public decimal? DecimalAsNumeric { get; set; }

            [Column(TypeName = "uniqueidentifier")]
            public Guid? GuidAsUniqueidentifier { get; set; }

            [Column(TypeName = "bigint")]
            public uint? UintAsBigint { get; set; }

            [Column(TypeName = "decimal(20,0)")]
            public ulong? UlongAsDecimal200 { get; set; }

            [Column(TypeName = "int")]
            public ushort? UShortAsInt { get; set; }

            [Column(TypeName = "smallint")]
            public sbyte? SByteAsSmallint { get; set; }

            [Column(TypeName = "varchar(max)")]
            public char? CharAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public char? CharAsAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public char? CharAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public char? CharAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public char? CharAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public char? CharAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public char? CharAsText { get; set; }

            [Column(TypeName = "ntext")]
            public char? CharAsNtext { get; set; }

            [Column(TypeName = "int")]
            public char? CharAsInt { get; set; }
        }

        protected class MappedDataTypesWithIdentity
        {
            public int Id { get; set; }

            [Column(TypeName = "int")]
            public int Int { get; set; }

            [Column(TypeName = "bigint")]
            public long LongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public short ShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public byte ByteAsTinyint { get; set; }

            [Column(TypeName = "int")]
            public uint UintAsInt { get; set; }

            [Column(TypeName = "bigint")]
            public ulong UlongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public ushort UShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public sbyte SbyteAsTinyint { get; set; }

            [Column(TypeName = "bit")]
            public bool BoolAsBit { get; set; }

            [Column(TypeName = "money")]
            public decimal DecimalAsMoney { get; set; }

            [Column(TypeName = "smallmoney")]
            public decimal DecimalAsSmallmoney { get; set; }

            [Column(TypeName = "float")]
            public double DoubleAsFloat { get; set; }

            [Column(TypeName = "real")]
            public float FloatAsReal { get; set; }

            [Column(TypeName = "double precision")]
            public double DoubleAsDoublePrecision { get; set; }

            [Column(TypeName = "date")]
            public DateTime DateTimeAsDate { get; set; }

            [Column(TypeName = "datetimeoffset")]
            public DateTimeOffset DateTimeOffsetAsDatetimeoffset { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime DateTimeAsDatetime2 { get; set; }

            [Column(TypeName = "smalldatetime")]
            public DateTime DateTimeAsSmalldatetime { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime DateTimeAsDatetime { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan TimeSpanAsTime { get; set; }

            [Column(TypeName = "varchar(max)")]
            public string StringAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public string StringAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public string StringAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public string StringAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public string StringAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public string StringAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public string StringAsText { get; set; }

            [Column(TypeName = "ntext")]
            public string StringAsNtext { get; set; }

            [Column(TypeName = "varbinary(max)")]
            public byte[] BytesAsVarbinaryMax { get; set; }

            [Column(TypeName = "binary varying(max)")]
            public byte[] BytesAsBinaryVaryingMax { get; set; }

            [Column(TypeName = "image")]
            public byte[] BytesAsImage { get; set; }

            [Column(TypeName = "decimal")]
            public decimal Decimal { get; set; }

            [Column(TypeName = "dec")]
            public decimal DecimalAsDec { get; set; }

            [Column(TypeName = "numeric")]
            public decimal DecimalAsNumeric { get; set; }

            [Column(TypeName = "uniqueidentifier")]
            public Guid GuidAsUniqueidentifier { get; set; }

            [Column(TypeName = "bigint")]
            public uint UintAsBigint { get; set; }

            [Column(TypeName = "decimal(20,0)")]
            public ulong UlongAsDecimal200 { get; set; }

            [Column(TypeName = "int")]
            public ushort UShortAsInt { get; set; }

            [Column(TypeName = "smallint")]
            public sbyte SByteAsSmallint { get; set; }

            [Column(TypeName = "varchar(max)")]
            public char CharAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public char CharAsAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public char CharAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public char CharAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public char CharAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public char CharAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public char CharAsText { get; set; }

            [Column(TypeName = "ntext")]
            public char CharAsNtext { get; set; }

            [Column(TypeName = "int")]
            public char CharAsInt { get; set; }
        }

        protected class MappedSizedDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int Int { get; set; }

            [Column(TypeName = "char(3)")]
            public string StringAsChar3 { get; set; }

            [Column(TypeName = "character(3)")]
            public string StringAsCharacter3 { get; set; }

            [Column(TypeName = "varchar(3)")]
            public string StringAsVarchar3 { get; set; }

            [Column(TypeName = "char varying(3)")]
            public string StringAsCharVarying3 { get; set; }

            [Column(TypeName = "character varying(3)")]
            public string StringAsCharacterVarying3 { get; set; }

            [Column(TypeName = "nchar(3)")]
            public string StringAsNchar3 { get; set; }

            [Column(TypeName = "national character(3)")]
            public string StringAsNationalCharacter3 { get; set; }

            [Column(TypeName = "nvarchar(3)")]
            public string StringAsNvarchar3 { get; set; }

            [Column(TypeName = "national char varying(3)")]
            public string StringAsNationalCharVarying3 { get; set; }

            [Column(TypeName = "national character varying(3)")]
            public string StringAsNationalCharacterVarying3 { get; set; }

            [Column(TypeName = "binary(3)")]
            public byte[] BytesAsBinary3 { get; set; }

            [Column(TypeName = "varbinary(3)")]
            public byte[] BytesAsVarbinary3 { get; set; }

            [Column(TypeName = "binary varying(3)")]
            public byte[] BytesAsBinaryVarying3 { get; set; }

            [Column(TypeName = "varchar(3)")]
            public char? CharAsVarchar3 { get; set; }

            [Column(TypeName = "char varying(3)")]
            public char? CharAsAsCharVarying3 { get; set; }

            [Column(TypeName = "character varying(3)")]
            public char? CharAsCharacterVarying3 { get; set; }

            [Column(TypeName = "nvarchar(3)")]
            public char? CharAsNvarchar3 { get; set; }

            [Column(TypeName = "national char varying(3)")]
            public char? CharAsNationalCharVarying3 { get; set; }

            [Column(TypeName = "national character varying(3)")]
            public char? CharAsNationalCharacterVarying3 { get; set; }
        }

        protected class MappedScaledDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int Int { get; set; }

            [Column(TypeName = "float(3)")]
            public float FloatAsFloat3 { get; set; }

            [Column(TypeName = "double precision(3)")]
            public float FloatAsDoublePrecision3 { get; set; }

            [Column(TypeName = "datetimeoffset(3)")]
            public DateTimeOffset DateTimeOffsetAsDatetimeoffset3 { get; set; }

            [Column(TypeName = "datetime2(3)")]
            public DateTime DateTimeAsDatetime23 { get; set; }

            [Column(TypeName = "decimal(3)")]
            public decimal DecimalAsDecimal3 { get; set; }

            [Column(TypeName = "dec(3)")]
            public decimal DecimalAsDec3 { get; set; }

            [Column(TypeName = "numeric(3)")]
            public decimal DecimalAsNumeric3 { get; set; }
        }

        protected class MappedPrecisionAndScaledDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int Int { get; set; }

            [Column(TypeName = "decimal(5,2)")]
            public decimal DecimalAsDecimal52 { get; set; }

            [Column(TypeName = "dec(5,2)")]
            public decimal DecimalAsDec52 { get; set; }

            [Column(TypeName = "numeric(5,2)")]
            public decimal DecimalAsNumeric52 { get; set; }
        }

        protected class MappedNullableDataTypesWithIdentity
        {
            public int Id { get; set; }

            [Column(TypeName = "int")]
            public int? Int { get; set; }

            [Column(TypeName = "bigint")]
            public long? LongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public short? ShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public byte? ByteAsTinyint { get; set; }

            [Column(TypeName = "int")]
            public uint? UintAsInt { get; set; }

            [Column(TypeName = "bigint")]
            public ulong? UlongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public ushort? UshortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public sbyte? SbyteAsTinyint { get; set; }

            [Column(TypeName = "bit")]
            public bool? BoolAsBit { get; set; }

            [Column(TypeName = "money")]
            public decimal? DecimalAsMoney { get; set; }

            [Column(TypeName = "smallmoney")]
            public decimal? DecimalAsSmallmoney { get; set; }

            [Column(TypeName = "float")]
            public double? DoubleAsFloat { get; set; }

            [Column(TypeName = "real")]
            public float? FloatAsReal { get; set; }

            [Column(TypeName = "double precision")]
            public double? DoubkleAsDoublePrecision { get; set; }

            [Column(TypeName = "date")]
            public DateTime? DateTimeAsDate { get; set; }

            [Column(TypeName = "datetimeoffset")]
            public DateTimeOffset? DateTimeOffsetAsDatetimeoffset { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime? DateTimeAsDatetime2 { get; set; }

            [Column(TypeName = "smalldatetime")]
            public DateTime? DateTimeAsSmalldatetime { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime? DateTimeAsDatetime { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan? TimeSpanAsTime { get; set; }

            [Column(TypeName = "varchar(max)")]
            public string StringAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public string StringAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public string StringAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public string StringAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public string StringAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public string StringAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public string StringAsText { get; set; }

            [Column(TypeName = "ntext")]
            public string StringAsNtext { get; set; }

            [Column(TypeName = "varbinary(max)")]
            public byte[] BytesAsVarbinaryMax { get; set; }

            [Column(TypeName = "binary varying(max)")]
            public byte[] BytesAsVaryingMax { get; set; }

            [Column(TypeName = "image")]
            public byte[] BytesAsImage { get; set; }

            [Column(TypeName = "decimal")]
            public decimal? Decimal { get; set; }

            [Column(TypeName = "dec")]
            public decimal? DecimalAsDec { get; set; }

            [Column(TypeName = "numeric")]
            public decimal? DecimalAsNumeric { get; set; }

            [Column(TypeName = "uniqueidentifier")]
            public Guid? GuidAsUniqueidentifier { get; set; }

            [Column(TypeName = "bigint")]
            public uint? UintAsBigint { get; set; }

            [Column(TypeName = "decimal(20,0)")]
            public ulong? UlongAsDecimal200 { get; set; }

            [Column(TypeName = "int")]
            public ushort? UShortAsInt { get; set; }

            [Column(TypeName = "smallint")]
            public sbyte? SByteAsSmallint { get; set; }

            [Column(TypeName = "varchar(max)")]
            public char? CharAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public char? CharAsAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public char? CharAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public char? CharAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public char? CharAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public char? CharAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public char? CharAsText { get; set; }

            [Column(TypeName = "ntext")]
            public char? CharAsNtext { get; set; }

            [Column(TypeName = "int")]
            public char? CharAsInt { get; set; }
        }

        protected class ColumnInfo
        {
            public string TableName { get; set; }
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public bool? IsNullable { get; set; }
            public int? MaxLength { get; set; }
            public int? NumericPrecision { get; set; }
            public int? NumericScale { get; set; }
            public int? DateTimePrecision { get; set; }
        }
    }
}
