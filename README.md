# SensitiveWords
字符串脱敏，输入输出多种处理模式，支持字符、拼音、简拼、同音字等替换模式，支持正则。
<!--TOC-->
- [安装](#安装)
- [使用](#使用)
- [示例](#示例)
- [其他](#其他)
<!--/TOC-->

#### 安装
> Install-Package SensitiveWords

#### 使用
```c#
// Web注入方式，根据配置自动处理输入输出
services.AddSensitiveWords(sensitiveWordsOptions =>
{
    sensitiveWordsOptions.Add(new SensitiveWordsOptions(HandleOptions.Input, ReplaceOptions.Character, "*", true, false, GroupReplaceOptions.GroupPriority)
        .Add("牛皮|反对"));
    sensitiveWordsOptions.Add(new SensitiveWordsOptions(HandleOptions.Output, ReplaceOptions.Character, "*", true, false, GroupReplaceOptions.GroupPriority)
        .Add(@"\w{4}(\w+(?:[-+.]\w+)*)@\w+(?:[-.]\w+)*\.\w+(?:[-.]\w+)*")
        .Add(@"(?:13[0-9]|14[0-14-9]|15[0-35-9]|16[25-7]|17[0-8]|18[0-9]|19[0-35-9])(\d{4})\d{4}")
    );
});

// 直接调用
SensitiveWordsResolver.Desensitize("...");
// 扩展方法
"...".Desensitize();

```

#### 示例
```c#
using SensitiveWords;
...
{
	// 初始化配置
	SensitiveWordsResolver.Config(options =>
	{
		options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "*", true).Add("啊啊|zf|666"));
		options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "?", true).Add("操|文|NM"));
		options.Add(new SensitiveWordsOptions(HandleOptions.Default | HandleOptions.Output, ReplaceOptions.Character, "*", true, groupReplaceOptions: GroupReplaceOptions.GroupPriority)
			.Add(@"\w{4}(\w+(?:[-+.]\w+)*)@\w+(?:[-.]\w+)*\.\w+(?:[-.]\w+)*")
			.Add(@"(?:13[0-9]|14[0-14-9]|15[0-35-9]|16[25-7]|17[0-8]|18[0-9]|19[0-35-9])(\d{4})\d{4}")
		);
	});

	var text = "zf这是什么操作啊啊……666 nm";
	// 扩展方法调用
	text.Desensitize(); // Output: **这是什么?作**……*** ??
	// 方法调用
	SensitiveWordsResolver.Desensitize(text); // Output: **这是什么?作**……*** ??

	var mail = "test123@mail.com";
	var phone = "13623332333";

	// 脱敏默认
	mail.Desensitize(); // Output: test***@mail.com
	// 脱敏输入
	mail.DesensitizeInput(); // Output: test123@mail.com
	phone.DesensitizeInput(); // Output: 13623332333
	// 脱敏输出
	mail.DesensitizeOutput(); // Output: test***@mail.com
	phone.DesensitizeOutput(); // Output: 136****2333
	// 脱敏所有
	mail.DesensitizeAll(); // Output: test***@mail.com
	phone.DesensitizeAll(); // Output: 136****2333

	SensitiveWordsResolver.Config(options =>
		options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "*", true, groupReplaceOptions: GroupReplaceOptions.GroupPriority)
			.Add(@"政治(?!老师|家)")
		)
	);
	"一个政治老师在谈政治话题".Desensitize(); // Output: 一个政治老师在谈**话题

	// 支持拼音等替换模式
	SensitiveWordsResolver.Config(options =>
	{
		options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.PinYin, null, true, groupReplaceOptions: GroupReplaceOptions.GroupPriority).Add("尘埃|酒"));
		options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.JianPin, null, true, groupReplaceOptions: GroupReplaceOptions.GroupPriority).Add("菩提|本来"));
		options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Homophone, null, true, groupReplaceOptions: GroupReplaceOptions.GroupPriority).Add("终身"));
	});

	"菩提本无树，明镜亦非台。本来无一物，何处惹尘埃！知足常乐，终身不辱，今朝有酒今朝醉，明日愁来明日愁。".Desensitize(); // Output: pt本无树，明镜亦非台。bl无一物，何处惹chenai！知足常乐，中伸不辱，今朝有jiu今朝醉，明日愁来明日愁。
}
```

#### 其他
特性：
- 忽略特性 `IgnoreSensitiveWordsAttribute`
- 指定处理标签特性 `SensitiveWordsAttribute` 通过 `new SensitiveWordsOptions().SetTag("Tag")` 配置标签

`SensitiveWordsOptions`：敏感词配置选项
- 属性
	- `HandleOptions` 处理选项
		- `Default`：默认
		- `Input`：输入
		- `Output`：输出
	- `ReplaceOptions` 替换选项
		- `Character`：字符
		- `PinYin`：拼音
		- `JianPin`：简拼
		- `Homophone`：同音字
	- `Character` 替换字符串 `ReplaceOptions.Character` 时生效
	- `IgnoreCase` 忽略大小写
	- `ReplaceSingle` 替换为单个不补位，默认 `false` 补位至敏感词长度
	- `GroupReplaceOptions` 组替换选项
		- `Default`：默认替换匹配到的值
		- `GroupOnly`：只替换正则组
		- `GroupPriority`：组优先，存在组就替换组不存在就默认替换
	- `Tag` 标签，可通过特性 `SensitiveWordsAttribute` 指定处理
- 方法
	- `Add` 添加敏感词，多个英文竖线|分隔，包含竖线需要转义，支持正则
	- `AddFile` 添加文件敏感词，文本内容规则一致
	- `SetTag` 设置标签

多音字词组添加：`SensitiveWordsResolver.RegisterHomophoneRegexWordGroup(...)` 存在多音字时无法区分，可通过添加词组确定
