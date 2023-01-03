# SensitiveWords
---
�ַ�������������������ִ���ģʽ��֧���ַ���ƴ������ƴ��ͬ���ֵ��滻ģʽ��֧������

#### ��װ
> Install-Package SensitiveWords

#### ʹ��
```c#
// Webע�뷽ʽ�����������Զ������������
services.AddSensitiveWords(sensitiveWordsOptions =>
{
    sensitiveWordsOptions.Add(new SensitiveWordsOptions(HandleOptions.Input, ReplaceOptions.Character, "*", true, false, GroupReplaceOptions.GroupPriority)
        .Add("ţƤ|����"));
    sensitiveWordsOptions.Add(new SensitiveWordsOptions(HandleOptions.Output, ReplaceOptions.Character, "*", true, false, GroupReplaceOptions.GroupPriority)
        .Add(@"\w{4}(\w+(?:[-+.]\w+)*)@\w+(?:[-.]\w+)*\.\w+(?:[-.]\w+)*")
        .Add(@"(?:13[0-9]|14[0-14-9]|15[0-35-9]|16[25-7]|17[0-8]|18[0-9]|19[0-35-9])(\d{4})\d{4}")
    );
});

// ֱ�ӵ���
SensitiveWordsResolver.Desensitize("...");
// ��չ����
"...".Desensitize();

```

#### ʾ��
```c#
using SensitiveWords;
...
{
	// ��ʼ������
	SensitiveWordsResolver.Config(options =>
	{
		options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "*", true).Add("����|zf|666"));
		options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "?", true).Add("��|��|NM"));
		options.Add(new SensitiveWordsOptions(HandleOptions.Default | HandleOptions.Output, ReplaceOptions.Character, "*", true, groupReplaceOptions: GroupReplaceOptions.GroupPriority)
            .Add(@"\w{4}(\w+(?:[-+.]\w+)*)@\w+(?:[-.]\w+)*\.\w+(?:[-.]\w+)*")
            .Add(@"(?:13[0-9]|14[0-14-9]|15[0-35-9]|16[25-7]|17[0-8]|18[0-9]|19[0-35-9])(\d{4})\d{4}")
        );
	});

	var text = "zf����ʲô������������666 nm";
	// ��չ��������
	text.Desensitize(); // Output: **����ʲô?��**����*** ??
	// ��������
	SensitiveWordsResolver.Desensitize(text); // Output: **����ʲô?��**����*** ??

	var mail = "test123@mail.com";
	var phone = "13623332333";

	// ����Ĭ��
	mail.Desensitize(); // Output: test***@mail.com
	// ��������
	mail.DesensitizeInput(); // Output: test123@mail.com
	phone.DesensitizeInput(); // Output: 13623332333
	// �������
	mail.DesensitizeOutput(); // Output: test***@mail.com
	phone.DesensitizeOutput(); // Output: 136****2333
	// ��������
	mail.DesensitizeAll(); // Output: test***@mail.com
	phone.DesensitizeAll(); // Output: 136****2333

	SensitiveWordsResolver.Config(options =>
		options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Character, "*", true, groupReplaceOptions: GroupReplaceOptions.GroupPriority)
			.Add(@"����(?!��ʦ|��)")
		)
	);
	"һ��������ʦ��̸���λ���".Desensitize(); // Output: һ��������ʦ��̸**����

	// ֧��ƴ�����滻ģʽ
	SensitiveWordsResolver.Config(options =>
    {
        options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.PinYin, null, true, groupReplaceOptions: GroupReplaceOptions.GroupPriority).Add("����|��"));
        options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.JianPin, null, true, groupReplaceOptions: GroupReplaceOptions.GroupPriority).Add("����|����"));
        options.Add(new SensitiveWordsOptions(HandleOptions.Default, ReplaceOptions.Homophone, null, true, groupReplaceOptions: GroupReplaceOptions.GroupPriority).Add("����"));
    });

	"���᱾�������������̨��������һ��δ��ǳ�����֪�㳣�֣������裬���оƽ������ճ������ճ".Desensitize(); // Output: pt���������������̨��bl��һ��δ���chenai��֪�㳣�֣����첻�裬����jiu�������ճ������ճ

}
```

#### ����
���ԣ�
- �������� `IgnoreSensitiveWordsAttribute`
- ָ�������ǩ���� `SensitiveWordsAttribute` ͨ�� `new SensitiveWordsOptions().SetTag("Tag")` ���ñ�ǩ

`SensitiveWordsOptions`�����д�����ѡ��
- ����
	- `HandleOptions` ����ѡ��
		- `Default`��Ĭ��
		- `Input`������
		- `Output`�����
	- `ReplaceOptions` �滻ѡ��
		- `Character`���ַ�
		- `PinYin`��ƴ��
		- `JianPin`����ƴ
		- `Homophone`��ͬ����
	- `Character` �滻�ַ��� `ReplaceOptions.Character` ʱ��Ч
	- `IgnoreCase` ���Դ�Сд
	- `ReplaceSingle` �滻Ϊ��������λ��Ĭ�� `false` ��λ�����дʳ���
	- `GroupReplaceOptions` ���滻ѡ��
		- `Default`��Ĭ���滻ƥ�䵽��ֵ
		- `GroupOnly`��ֻ�滻������
		- `GroupPriority`�������ȣ���������滻�鲻���ھ�Ĭ���滻
	- `Tag` ��ǩ����ͨ������ `SensitiveWordsAttribute` ָ������
- ����
	- `Add` ������дʣ����Ӣ������|�ָ�������������Ҫת�壬֧������
	- `AddFile` ����ļ����дʣ��ı����ݹ���һ��
	- `SetTag` ���ñ�ǩ

�����ִ�����ӣ�`SensitiveWordsResolver.RegisterHomophoneRegexWordGroup(...)` ���ڶ�����ʱ�޷����֣���ͨ����Ӵ���ȷ��
