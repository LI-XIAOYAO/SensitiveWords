﻿using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SensitiveWordsTests;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SensitiveWords.Tests
{
    public class SensitiveWordsResolverTests : IClassFixture<WebApplicationFactory<Program>>
    {
        [Fact()]
        public void DesensitizeTest()
        {
            SensitiveWordsResolver.RegisterHomophoneRegexWordGroup(groupOptions => groupOptions
                .Add("hang", "一行|银行|行当")
                .Add("xing", "不行|行不")
                .Add("ti", "菩提|提问")
            );

            SensitiveWordsResolver.Config(options =>
            {
                options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "*", true).Add("啊啊|zf|666"));
                options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "?", true).Add("操|文|NM"));
            });

            Assert.Equal("**这是什么?作**……*** ??", "zf这是什么操作啊啊……666 nm".Desensitize());

            SensitiveWordsResolver.Config(options =>
            {
                options.Add(new SensitiveWordsOptions(HandleOptions.Default | HandleOptions.Output, ReplaceOptions.Character, "*", true, groupReplaceOptions: GroupReplaceOptions.GroupPriority)
                    .Add(@"\w{4}(\w+(?:[-+.]\w+)*)@\w+(?:[-.]\w+)*\.\w+(?:[-.]\w+)*")
                    .Add(@"(?:13[0-9]|14[0-14-9]|15[0-35-9]|16[25-7]|17[0-8]|18[0-9]|19[0-35-9])(\d{4})\d{4}")
                );
            });

            Assert.Equal("test***@mail.com", "test123@mail.com".Desensitize());
            Assert.Equal("test123@mail.com", "test123@mail.com".DesensitizeInput());
            Assert.Equal("test***@mail.com", "test123@mail.com".DesensitizeOutput());
            Assert.Equal("136****2333", "13623332333".DesensitizeOutput());

            SensitiveWordsResolver.Config(options =>
            {
                options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "*", true).AddFile("Words-Polit.txt"));
                options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "*", true).AddFile("Words-Price.txt"));
            });

            Assert.Equal("富强**和谐", "富强民主和谐".Desensitize());
            Assert.Equal("这个是**的", "这个是免费的".Desensitize());

            SensitiveWordsResolver.Config(options =>
                options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "*", true, groupReplaceOptions: GroupReplaceOptions.GroupPriority)
                    .Add(@"政治(?!老师|家)|\d{3}-\d{2}(\d{4})\d{2}|\d{4}-\d{2}(\d{3})\d{2}")
                    .SetTag("SWTest")
                )
            );

            var test = new Test
            {
                P1 = "特价商品",
                P3 = "呜呜呜~",
                P4 = "忽略的特价商品",
                P5 = new Test.Test1
                {
                    P1 = "自由",
                    P2 = "666..."
                },
                P6 = new Test.Test2
                {
                    P1 = "33333@163.com"
                },
                P7 = new System.Collections.Generic.List<Test.Test3>
                {
                    new Test.Test3{
                        P1 = "17611116666",
                        P2 = "18923331635",
                        P3 = "010-33445566"
                    },
                    new Test.Test3{
                        P1 = "真划算",
                        P2 = "如果暴力不是为了杀戮",
                        P3 = "一个政治老师在谈政治"
                    },
                },
                P8 = new System.Collections.Generic.Dictionary<string, string> {
                    { "1", "独立自主" }
                }
            };
            test.P6.Test = test;

            var testJson = "{\"P1\":\"**商品\",\"P2\":0,\"P3\":\"呜呜呜~\",\"P4\":\"忽略的特价商品\",\"P5\":{\"P1\":\"**\",\"P2\":\"***...\"},\"P6\":{\"P1\":\"3333*@163.com\"},\"P7\":[{\"P1\":\"1761111***6\",\"P2\":\"189****1635\",\"P3\":\"010-33****66\"},{\"P1\":\"真**\",\"P2\":\"如果**不是为了杀戮\",\"P3\":\"一个政治老师在谈**\"}],\"P8\":{\"1\":\"**自主\"}}";
            Assert.Equal(testJson, JsonConvert.SerializeObject(test.DesensitizeAll(), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

            SensitiveWordsResolver.Config(options =>
            {
                options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.PinYin, null, true, groupReplaceOptions: GroupReplaceOptions.GroupPriority).Add("尘埃|酒"));
                options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.JianPin, null, true, groupReplaceOptions: GroupReplaceOptions.GroupPriority).Add("菩提|本来"));
                options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Homophone, null, true, groupReplaceOptions: GroupReplaceOptions.GroupPriority).Add("终身"));
            });

            Assert.Equal("pt本无树，明镜亦非台。bl无一物，何处惹chenai！知足常乐，中伸不辱，今朝有jiu今朝醉，明日愁来明日愁。", "菩提本无树，明镜亦非台。本来无一物，何处惹尘埃！知足常乐，终身不辱，今朝有酒今朝醉，明日愁来明日愁。".Desensitize());

            SensitiveWordsResolver.Options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "*", true, false, GroupReplaceOptions.GroupPriority, false)
                .Add("你是不是傻啊|你是不是傻")
            );

            Assert.Equal("*****啊", "你是不是傻啊".Desensitize());

            SensitiveWordsResolver.Options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "*", true, false, GroupReplaceOptions.GroupPriority)
                .Add("你是不是笨啊|你是不是笨")
            );

            Assert.Equal("******", "你是不是笨啊".Desensitize());
        }

        [Fact()]
        public async Task MvcTest()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSensitiveWords(sensitiveWordsOptions =>
                        {
                            sensitiveWordsOptions.Add(new SensitiveWordsOptions(HandleOptions.Input, ReplaceOptions.Character, "*", true, false, GroupReplaceOptions.GroupPriority)
                                .Add("牛皮|反对"));
                            sensitiveWordsOptions.Add(new SensitiveWordsOptions(HandleOptions.Output, ReplaceOptions.Character, "*", true, false, GroupReplaceOptions.GroupPriority)
                                .Add(@"\w{4}(\w+(?:[-+.]\w+)*)@\w+(?:[-.]\w+)*\.\w+(?:[-.]\w+)*")
                                .Add(@"(?:13[0-9]|14[0-14-9]|15[0-35-9]|16[25-7]|17[0-8]|18[0-9]|19[0-35-9])(\d{4})\d{4}")
                            );
                        });
                    });
                });

            SensitiveWordsResolver.RegisterHomophoneRegexWordGroup(groupOptions =>
                groupOptions.AddFile("Homophone.txt")
            );

            var httpClient = application.CreateClient();
            var response = await httpClient.GetStringAsync("User/GetUser?name=专家只会吹牛皮");
            Assert.Equal("{\"name\":\"专家只会吹**\",\"mail\":\"1122****@mail.com\",\"phone\":\"166****3333\"}", response);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void NodeTest(bool isMaxMatch)
        {
            SensitiveWordsResolver.Config(options =>
            {
                SensitiveWordsResolver.Options.Clear();

                options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "*", true, isMaxMatch: isMaxMatch, whiteSpaceOptions: isMaxMatch ? WhiteSpaceOptions.IgnoreWhiteSpace | WhiteSpaceOptions.IgnoreNewLine : WhiteSpaceOptions.Default)
                    .AddFile(  "Words.txt"));
            });

            var array = new[] { "ABD", "abde", "ABCGH", "A", "AB", "ABC", "BCD", "E", "EF", "AD", "ADF", "AE", "JLB", "JBLL", "JLFD", "JLFDE", "JLFB", "JLFBA", "JLFBD", "CO" };

            var wordsNodes = WordsNodes.Build(array);
            var words = wordsNodes.GetWords();
            foreach (var item in words)
            {
                Debug.Write($"{item} ");
            }

            var text = "KABCOOJLFDEEF";

            var nodeCaptures = wordsNodes.Matches(text, isMaxMatch);
            var replace = wordsNodes.Replace(text, c => $"({c.Value})", isMaxMatch);
            if (isMaxMatch)
            {
                Assert.Equal("K(ABC)OO(JLFDE)(EF)", replace);
            }
            else
            {
                Assert.Equal("K(A)B(CO)O(JLFD)(E)(E)F", replace);
            }

            Debug.WriteLine(string.Empty);
            Debug.WriteLine(text);
            Debug.WriteLine(replace);

            text = "他家老大对老  二说狗屁\r\n专家KJHGO";
            replace = text.Desensitize();

            if (isMaxMatch)
            {
                Assert.Equal("他家老大对**说****KJHGO", replace);
            }
            else
            {
                Assert.Equal("他家老大对老  二说**\r\n专家KJHGO", replace);
            }

            Debug.WriteLine(string.Empty);
            Debug.WriteLine(text);
            Debug.WriteLine(replace);
        }
    }
}